// CODER	      :	MIN WOOK KIM
// MODIFIED DATE : 2023. 08. 23
// IMPLEMENTATION: Room에 필요한 RogueLite 데이터 클래스
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using RandomDungeonWithBluePrint;
using UnityRandom = UnityEngine.Random;

namespace CoffeeCat.RogueLite {
	public class RoomData {
		public RoomType RoomType { get; protected set; }                   // 룸 타입
		public int RoomIndex { get; protected set; } = 0;                  // 룸 인덱스
		public int Rarity { get; protected set; } = 0;                     // 룸의 레어도
		public bool IsCleared { get; protected set; } = false;             // 해당 룸의 클리어 여부
		public bool IsLocked { get; protected set; } = false;              // 현재 룸이 잠금 상태
		public bool IsPlayerInside { get; protected set; } = false;        // 플레이어가 방 안에 있는지
		public bool IsPlayerFirstEntered { get; protected set; }  = false; // 플레이어의 처음 방문 여부
		protected Action<bool> OnRoomLocked { get; private set; } = null;  // 방 잠금 상태 변경 시 실행할 액션
		protected InteractableObject interactableObject = null;            // 상호작용 오브젝트
		protected Vector3 interactiveObjectSpawnPos = Vector3.zero;
		
		public RoomData(RoomType roomType, int index, int rarity = 0, Room room = null) {
			RoomType = roomType;
			RoomIndex = index;
			Rarity = rarity;
			
			if (room != null)
			{
				interactiveObjectSpawnPos = room.FloorRectInt.center;
			}
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
			var loomDataStruct = new RoomDataStruct(this);
			if (!IsPlayerFirstEntered)
			{
				StageManager.Instance.InvokeRoomEnteringFirstEvent(loomDataStruct);
				IsPlayerFirstEntered = true;
			}
			StageManager.Instance.InvokeRoomEnteringEvent(loomDataStruct);
		}

		/// <summary>
		/// Room Leaving Callback
		/// </summary>
		public virtual void LeavesPlayer() {
			IsPlayerInside = false;
			var loomDataStruct = new RoomDataStruct(this);
			StageManager.Instance.InvokeRoomLeftEvent(loomDataStruct);
		}

		protected void SetInteractiveObject(InteractableType type)
		{
			if (interactableObject) {
				return;
			}
			interactableObject = ObjectPoolManager.Instance.Spawn<InteractableObject>(type.ToKey(), interactiveObjectSpawnPos);
		}

		public virtual void Dispose() {
			OnRoomLocked = null;
			if (!interactableObject) 
				return;
			if (ObjectPoolManager.IsExist) {
				ObjectPoolManager.Instance.Despawn(interactableObject.gameObject);	
			}
		}
	}

	public class BattleRoom : RoomData {
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
		
		public BattleRoom(Room room, int index, BattleRoomDataEntity entity) : base(RoomType.MonsterSpawnRoom, index, entity.Rarity) {
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
				SafeRegister.RequestRegist(spawnDataList[i].Key.ToStringEx());
			}
			// Preload Group Monsters Positions
			if (entity.GroupSpawnPointKey == AddressablesKey.NONE)
				return;
			groupSpawnPositionsKey = entity.GroupSpawnPointKey.ToStringEx();
			SafeRegister.RequestRegist(groupSpawnPositionsKey);
		}

		public override void EnteredPlayer()
		{
			IsPlayerInside = true;
			var roomDataStruct = new RoomDataStruct(this);
			if (!IsPlayerFirstEntered)
			{
				StageManager.Instance.InvokeRoomEnteringFirstEvent(roomDataStruct);
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
			
			StageManager.Instance.InvokeRoomEnteringEvent(roomDataStruct);
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
			IsLocked = false;
			OnRoomLocked?.Invoke(IsLocked);
			var roomDataStruct = new RoomDataStruct(this);
			StageManager.Instance.InvokeEventClearedRoomEvent(roomDataStruct);
			StageManager.Instance.ClearCurrentRoomKillCount();
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

		public override void Dispose() {
			base.Dispose();
			if (ObjectPoolManager.IsExist) {
				ObjectPoolManager.Instance?.DespawnAll(groupSpawnPositionsKey);
			}
			IsCleared = true;
		}
	}

	public class PlayerSpawnRoom : RoomData
	{
		public PlayerSpawnRoom(int index) : base(RoomType.PlayerSpawnRoom, index) { }
	}

	public class RewardRoom : RoomData {
		public RewardRoom(Room room, int index) : base(RoomType.RewardRoom, index, room: room) { }

		public override void EnteredPlayer() {
			base.EnteredPlayer();
			SetInteractiveObject(InteractableType.Reward);
			interactableObject.PlayParticle();
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();
			if (interactableObject) {
				interactableObject.StopParticle();
			}
		}
	}

	public class ShopRoom : RoomData {
		public ShopRoom(Room room, int index) : base(RoomType.ShopRoom, index, room: room) { }

		public override void EnteredPlayer() {
			base.EnteredPlayer();
			SetInteractiveObject(InteractableType.Shop);
			interactableObject.PlayParticle();
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();
			if (interactableObject) {
				interactableObject.StopParticle();
			}
		}
	}
	
	public class EmptyRoom : RoomData {
		public EmptyRoom(Room room, int index) : base(RoomType.EmptyRoom, index, room: room) { }
		
	}

	public class ExitRoomInteractable : RoomData {
		public ExitRoomInteractable(Room room, int index) : base(RoomType.ExitRoom, index, room: room) { }

		public override void EnteredPlayer() {
			base.EnteredPlayer();
			SetInteractiveObject(InteractableType.Floor);
			interactableObject.PlayParticle();
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();
			if (interactableObject) {
				interactableObject.StopParticle();
			}
		}
	}

	public struct RoomDataStruct
	{
		public RoomType RoomType;
		public int RoomIndex;

		public RoomDataStruct(RoomData roomData)
		{
			RoomType = roomData.RoomType;
			RoomIndex = roomData.RoomIndex;
		}
	}
}
