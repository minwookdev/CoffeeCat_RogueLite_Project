// CODER	      :	MIN WOOK KIM
// MODIFIED DATE : 2023. 08. 23
// IMPLEMENTATION: Room에 필요한 RogueLite 데이터 클래스
using System;
using System.Collections.Generic;
using System.Linq;
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
		public bool IsPlayerFirstEntered { get; protected set; }  = false;	   // 플레이어의 처음 방문 여부
		protected Action<bool> OnRoomLocked { get; private set; } = null;				   // 방 잠금 상태 변경 시 실행할 액션
		
		public RoomData(Room room, RoomType roomType, int rarity = 0) {
			RoomType = roomType;
			Rarity = rarity;
		}

		public virtual void Initialize() { }

		public void SetGateObjects(GateObject[] gateObjects)
		{
			if (gateObjects == null)
				return;
            
			OnRoomLocked += isLocked =>
			{
				for (int i = 0; i < gateObjects.Length; i++)
				{
					gateObjects[i].Lock(isLocked);
				}
			};
		}

		/// <summary>
		/// Room Entering Callback
		/// </summary>
		public virtual void EnteredPlayer() {
			IsPlayerInside = true;
			if (!IsPlayerFirstEntered)
			{
				StageManager.Instance.InvokeRoomEnteringFirstEvent(RoomType);
				IsPlayerFirstEntered = true;
			}
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
		private List<MonsterStatus> spawnedMonsters = null;
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
			spawnedMonsters = new List<MonsterStatus>();
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

		public override void EnteredPlayer()
		{
			IsPlayerInside = true;
			if (!IsPlayerFirstEntered)
			{
				StageManager.Instance.InvokeRoomEnteringFirstEvent(RoomType);
				IsPlayerFirstEntered = true;

				if (!IsCleared)
				{
					// Room Locked And Monster Spawn Start
					IsLocked = true;
					OnRoomLocked?.Invoke(IsLocked);
					SpawnGroupMonster(); // 그룹 몬스터 스폰
					ObservableUpdateBattleRoom();  // 일반 몬스터 스폰
				}
			}
			
			StageManager.Instance.InvokeRoomEnteringEvent(RoomType);
			return;

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
					var spawnedMonster = ObjectPoolManager.Instance.Spawn<MonsterStatus>(key, point);
					spawnedMonsters.Add(spawnedMonster);
					groupMonsterSpawnCount++;
				}
			}
			
			void ObservableUpdateBattleRoom() {
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
				          .Subscribe(currentKillCount => {
					          endureTimer += Time.deltaTime;
					          spawnTimer += Time.deltaTime;
					          
					          // Spawn Monster
					          if (spawnTimer >= spawnIntervalTime) {
						          SpawnMonster();
						          spawnTimer = 0f;
					          }
					          
					          // Check Room Clear Condition
					          IsCleared = IsClear(currentKillCount);
				          });
				return;

				// Monster Spawn 
				void SpawnMonster() {
					if (currentSpawnCount >= MaxSpawnCount) {
						return;
					}

					var activatedMonsters = spawnedMonsters.Count(monster => monster.IsAlive);
					if (activatedMonsters >= keepAverageCount)
						return;
					
					var spawnedMonster = ObjectPoolManager.Instance.Spawn<MonsterStatus>(RaffleSpawnMonster(), GetRandomPos());
					spawnedMonsters.Add(spawnedMonster);
					currentSpawnCount++;
				}

				// Battle Room Clear Condition
				bool IsClear(int killedCount) => (killedCount >= MaxSpawnCount || endureTimer >= EndureSeconds);
			}
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();

			if (!IsCleared) {
				// Return to Room
			}
		}

		private void OnCleared() {
			// Despawn All Alive Monsters
			var aliveMonsters = spawnedMonsters.Where(monster => monster.IsAlive);
			foreach (var monster in aliveMonsters) {
				monster.ForcedKillMonster();
			}
			spawnedMonsters.Clear();
			spawnedMonsters = null;
			
			IsCleared = true;
			IsLocked = true;
			StageManager.Instance.InvokeEventClearedRoomEvent(RoomType);
			StageManager.Instance.ClearCurrentRoomKillCount();
			CatLog.Log("On Cleared Battle Room");
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

	public class ExitRoom : RoomData
	{
		public ExitRoom(Room room, RoomType roomType, int rarity = 0) : base(room, roomType, rarity)
		{
			
		}
	}
}
