// CODER	      :	MIN WOOK KIM
// MODIFIED DATE : 2023. 08. 23
// IMPLEMENTATION: Room에 필요한 RogueLite 데이터 클래스
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using RandomDungeonWithBluePrint;
using UnityRandom = UnityEngine.Random;

namespace CoffeeCat.RogueLite {
	public class RoomData {
		public RoomType RoomType { get; protected set; } = RoomType.EmptyRoom; // 룸 타입 
		public int Rarity { get; protected set; } = 0;                         // 룸의 레어도
		public bool IsCleared { get; protected set; } = false;                 // 해당 룸의 클리어 여부
		public bool IsLocked { get; protected set; } = false;                  // 현재 룸이 잠금 상태
		public bool IsPlayerInside { get; protected set; } = false;			   // 플레이어가 방 안에 있는지
		protected Action<bool> roomLockAction = null;						   // 현재 Room의 GateObject Lock 처리

		public RoomData(Room room, RoomType roomType, int rarity = 0) {
			RoomType = roomType;
			Rarity = rarity;
			roomLockAction = room.RoomLockAction;
		}

		public virtual void Initialize() {
			
		}

		/// <summary>
		/// Room Entering Callback
		/// </summary>
		public virtual void EnteredPlayer() {
			IsPlayerInside = true;
			StageManager.Instance.InvokeRoomEnteringEvent(RoomType);
			// CatLog.Log("Entering Room. print room info log" + '\n' +
			//            $"type: {RoomType.ToStringExtended()}" + '\n' +
			//            $"rarity: {Rarity.ToString()}");
		}

		/// <summary>
		/// Room Leaving Callback
		/// </summary>
		public virtual void LeavesPlayer() {
			IsPlayerInside = false;
			// CatLog.Log("Leaves Room");
		}
	}

	public class BattleRoomData : RoomData {
		// Monster Spawn Variables
		private static readonly float spawnIntervalTime = 0.35f;
		public Vector2[] SpawnPositions { get; private set; } = null;
		private Vector2 roomCenterPos = default;
		private List<MonsterSpawnData> spawnDataList;
		private string groupSpawnPositionsKey;
		private float totalWeight = 0f;
		private int groupMonsterSpawnCount = 0;
		private int keepAverageCount = 0;
		
		// Room Clear Variables
		private int MaxSpawnCount = 0;
		private float EndureSeconds = 0f;
		
		public BattleRoomData(Room room, BattleRoomDataEntity entity) : base(room, RoomType.MonsterSpawnRoom, entity.Rarity) {
			SpawnPositions = GetMonsterSpawnPositions(room, tileRadius: 0.5f);
			roomCenterPos = room.FloorRectInt.center;
			MaxSpawnCount = entity.MaxSpawnMonster;
			keepAverageCount = entity.KeepAverageCount;
			EndureSeconds = entity.EndureSeconds;
			var weights = entity.SpawnWeights;
			spawnDataList = new List<MonsterSpawnData>();
			// Weights first
			for (int i = 0; i < weights.Length; i++) {
				// 동일한 Index의 SpawnKey가 존재하지 않거나 스폰 확률이 지정되어있지 않은 경우
				if (entity.SpawnKeys.Length <= i || entity.SpawnWeights[i] <= 0f) {
					continue;
				}

				var spawnData = new MonsterSpawnData() {
					Weight = weights[i],
					Key = entity.SpawnKeys[i]
				};
				
				// Add New Spawn Data
				spawnDataList.Add(spawnData);
				totalWeight += weights[i];
			}
			
			// Preload Monsters
			for (int i = 0; i < spawnDataList.Count; i++) {
				Preloader.Process(spawnDataList[i].Key.ToStringEx());
			}
			// Preload Group Monsters Positions
			if (entity.GroupSpawnPointKey == AddressablesKey.NONE)
				return;
			groupSpawnPositionsKey = entity.GroupSpawnPointKey.ToStringEx();
			Preloader.Process(groupSpawnPositionsKey);
		}

		public override void Initialize() {
			
		}

		public override void EnteredPlayer() {
			base.EnteredPlayer();
			if (IsCleared) 
				return;
			
			// Room Locked And Monster Spawn Start
			IsLocked = true;
			roomLockAction?.Invoke(IsLocked);
			StageManager.Instance.InvokeRoomEnteringFirstEvent(RoomType);
			SpawnGroupMonster(); // 그룹 몬스터 스폰
			UpdateBattleRoom();  // 일반 몬스터 스폰

			void SpawnGroupMonster() {
				if (groupSpawnPositionsKey.Equals(string.Empty)) {
					return;
				}

				var groupSpawnPoint = ObjectPoolManager.Instance.Spawn<MonsterGroupSpawnPoint>(groupSpawnPositionsKey, roomCenterPos);
				var points = groupSpawnPoint.SpawnPositions;
				foreach (var point in points) {
					if (groupMonsterSpawnCount >= MaxSpawnCount) {
						break;
					}

					// Spawn Monster Group Spawn Position
					var key = RaffleSpawnMonster();
					ObjectPoolManager.Instance.Spawn(key, point);
					groupMonsterSpawnCount++;
				}
			}
			
			void UpdateBattleRoom() {
				// Variables
				float spawnTimer = 0f;
				float endureTimer = 0f;
				int currentSpawnCount = 0;
			
				// Subscribe Spawn Update Observable
				Observable.EveryUpdate()
				          .Skip(TimeSpan.Zero)
				          .Select(_ => StageManager.Instance.CurrentRoomMonsterKilledCount)
				          .TakeWhile(_ => !IsCleared)
				          .DoOnCompleted(OnCleared)
				          .Subscribe(currentRoomKillCount => {
					          // Check Group Monster All Kills
					          if (groupMonsterSpawnCount > currentRoomKillCount) {
						          return;
					          } // DelayTime When Group Monsters All Kill

					          // Update Timer
					          endureTimer += Time.deltaTime;
					          spawnTimer += Time.deltaTime;
					          
					          // Spawn Monster
					          if (spawnTimer >= spawnIntervalTime) {
						          SpawnMonster();
						          spawnTimer = 0f;
					          }
					          
					          // Debug
					          CatLog.Log("OnBattleRoom Update !");

					          // Check Clear
					          IsCleared = IsClear(currentRoomKillCount);
				          });

				// Monster Spawn 
				void SpawnMonster() {
					if (currentSpawnCount >= MaxSpawnCount) {
						return;
					}
					
					var spawnedMonster = 
						ObjectPoolManager.Instance.Spawn(RaffleSpawnMonster(), GetRandomPos());
					currentSpawnCount++;
				}

				// Battle Room Clear Condition
				bool IsClear(int killedCount) => (killedCount >= MaxSpawnCount || endureTimer >= EndureSeconds);
			}
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();

			if (!IsCleared) {
				// Return to Room...
			}
		}

		private void OnCleared() {
			IsCleared = true;
			IsLocked = true;
			StageManager.Instance.InvokeEventClearedRoomEvent(RoomType);

			// 방에 남아있는 모든 몬스터 처치
			// 몬스터 스폰 시 List또는 Queue에 담아두는 방식을 사용 !
			// 또는 조건에 따라 ObjectPoolManager.DisableAll 메서드를 사용
		}

		Vector2[] GetMonsterSpawnPositions(Room room, float tileRadius = 0.5f) {
			var floors = room.Floors;
			float floorXMin = floors[0].x, 
			      floorXMax = floors[0].x, 
			      floorYMin = floors[0].y, 
			      floorYMax = floors[0].y;
			foreach (var floor in floors) {
				if (floor.x < floorXMin) floorXMin = floor.x;
				if (floor.x > floorXMax) floorXMax = floor.x;
				if (floor.y < floorYMin) floorYMin = floor.y;
				if (floor.y > floorYMax) floorYMax = floor.y;
			}
			
			List<Vector2> positionList = new List<Vector2>();
			for (int i = 0; i < floors.Count; i++) {
				if (floors[i].x == floorXMin || floors[i].x == floorXMax ||
				    floors[i].y == floorYMin || floors[i].y == floorYMax) {
					continue;
				}

				Vector2 position = new Vector2(floors[i].x + tileRadius, floors[i].y + tileRadius);
				if (positionList.Contains(position)) {
					continue;
				}
				positionList.Add(position);
			}
			return positionList.ToArray();
		}

		private struct MonsterSpawnData {
			public float Weight;
			public AddressablesKey Key;
		}

		private string RaffleSpawnMonster() {
			float randomPoint = UnityRandom.value * totalWeight;
			int index = 0;
			for (int i = 0; i < spawnDataList.Count; i++) {
				if (randomPoint < spawnDataList[i].Weight) {
					index = i;
				}
				else {
					randomPoint -= spawnDataList[i].Weight;
				}
			}
			return spawnDataList[index].Key.ToStringEx();
		}

		private Vector2 GetRandomPos() {
			int index = UnityRandom.Range(0, SpawnPositions.Length);
			return SpawnPositions[index];
		}
	}

	public class PlayerSpawnRoomData : RoomData
	{
		private readonly string key1 = "skill_selector_fireball";
		private readonly string key2 = "skill_selector_lightning";
		
		public PlayerSpawnRoomData(Room room) : base(room, RoomType.PlayerSpawnRoom) {
			// Spawn Init Skill Selector Prefabs
			var playerSpawnPosition = room.Rect.center;
			var skillPrefab1Position = playerSpawnPosition.x -= 3f;
			var skillPrefab2Position = playerSpawnPosition.x += 3f;
			Preloader.Process(key1);
			Preloader.Process(key2);
		}

		public override void Initialize() {
			base.Initialize();
		}

		public override void EnteredPlayer() {
			base.EnteredPlayer();
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();
		}
	}

	public class RewardRoomData {
		
	}

	public class ShopRoomData {
		
	}
}
