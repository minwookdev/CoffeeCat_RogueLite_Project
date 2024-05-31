using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using CoffeeCat.Utils;
using RandomDungeonWithBluePrint;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Grid = UnityEngine.Grid;

namespace CoffeeCat
{
    public enum RoomType
    {
        PlayerSpawnRoom,
        MonsterSpawnRoom,
        ShopRoom,
        BossRoom,
        RewardRoom,
        EmptyRoom,
        ExitRoom,
    }

    public class StageManager : PrelocatedSingleton<StageManager> {
        [Title("StageManager", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Requires"), SerializeField] private RandomMapGenerator generator;
        [TabGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentRoomMonsterKilledCount { get; private set; } = 0;
        [TabGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentFloorMonsterKillCount { get; private set; } = 0;
        [TabGroup("Requires"), ShowInInspector, ReadOnly] public int TotalMonsterKilledCount { get; private set; } = 0;
        [TabGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentFloor { get; private set; } = 0;
        private Room playerCurrentRoom = null;
        public Room PlayerCurrentRoom => playerCurrentRoom;
        public bool IsPlayerInsideRoom => playerCurrentRoom != null;
        
        [Title("Events", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Events"), SerializeField] private UnityEvent<Field> onMapGenerateCompleted = null;
        [TabGroup("Events"), SerializeField] private UnityEvent onMapDisposeBefore = null;
        [TabGroup("Events"), SerializeField] private UnityEvent<RoomType> OnRoomEntering = null;
        [TabGroup("Events"), SerializeField] private UnityEvent<RoomType> OnRoomFirstEntering = null;
        [TabGroup("Events"), SerializeField] private UnityEvent<RoomType> OnClearedRoom = null;
        [TabGroup("Events"), SerializeField] private UnityEvent OnMonsterKilled = null;
        [TabGroup("Events"), SerializeField] private UnityEvent OnPlayerKilled = null;
        [TabGroup("Events"), SerializeField] private UnityEvent OnOpeningSkillSelectPanel = null;
        [TabGroup("Events"), SerializeField] private UnityEvent OnSkillSelectCompleted = null;

        [Title("Player_TSet")]
        public TSet_PlayerStatus PlayerStatus = null;
        public TSet_PlayerSkills PlayerSkills = null;

        private void Update()
        {
            if (!RogueLiteManager.Instance.SpawnedPlayer) {
                return;
            }
            
            // 카메라 업데이트는 카메라에서 !
            CameraMovementUpdate();
        }

        private void CameraMovementUpdate() {
            Vector2 playerPos = RogueLiteManager.Instance.SpawnedPlayer.transform.position;
            Camera.main.transform.position = new Vector3(playerPos.x, playerPos.y, Camera.main.transform.position.z);
        }

        public void CreateGate(Field field)
        {
            var grouppedGates = field.Gates.GroupBy(g => g.Room);
            var spawnedGateList = new List<GateObject>();
            foreach (var group in grouppedGates)
            {
                var room = group.Key;
                if (room.RoomData == null)
                    continue;
                spawnedGateList.Clear();
                
                foreach (var gate in group)
                {
                    var spawnedGate = ObjectPoolManager.Instance.Spawn<GateObject>("dungeon_door", Vector3.zero);
                    spawnedGate.Initialize(gate.Direction, gate.Position, gate.Room);
                    spawnedGateList.Add(spawnedGate);
                }
                room.SetGateObjects(spawnedGateList.ToArray());
            }
        }

        public void SetPlayer(Field field) {
            // Not Founded Entry Room
            if (!field.TryFindRoomFromType(RoomType.PlayerSpawnRoom, out Room result)) {
                CatLog.WLog("Not Exist PlayerSpawn Room !");
                return;
            }
            
            // Set RogueLite Player Object
            RogueLiteManager.Instance.SetPlayerOnEnteredDungeon(result.Rect.center);
            playerCurrentRoom = result;
            playerCurrentRoom.RoomData.EnteredPlayer();
        }

        /// <summary>
        /// Despawn Players
        /// </summary>
        public void DisablePlayer() {
            RogueLiteManager.Instance.DisablePlayer();
        }

        /// <summary>
        /// Despawn All Gates
        /// </summary>
        public void DespawnGates() => ObjectPoolManager.Instance.DespawnAll("dungeon_door");

        public void SetPlayersRoom(Room enteredRoom) {
            // 현재 플레이어의 방을 정의하는 변수가 덮혀씌워지는 것을 체크
            if (playerCurrentRoom != null) {
                CatLog.WLog("Overriding Players CurrentRoom !");
            }
            
            playerCurrentRoom = enteredRoom;
        }

        public void ClearPlayersRoom(Room leavesRoom) {
            // 현재 플레이어의 방과 나가려는 방이 일치하는지 확인
            if (!ReferenceEquals(playerCurrentRoom, leavesRoom)) {
                CatLog.ELog("Player Leaves Room Check Error !");
                return;
            }

            playerCurrentRoom = null;
        }

        public void EnableInput() {
            RogueLiteManager.Instance.EnableInput();
        }
        
        public void DisableInput() {
            RogueLiteManager.Instance.DisableInput();
        }
        
        public void RestoreTimeScale() {
            RogueLiteManager.Instance.RestoreTimeScale();
        }
        
        public void TimeScaleZero() {
            RogueLiteManager.Instance.TimeScaleZero();
        }

        public void ClearCurrentRoomKillCount()
        {
            CurrentFloorMonsterKillCount += CurrentRoomMonsterKilledCount;
            CurrentRoomMonsterKilledCount = 0;
        }

        public void AddCurrentRoomKillCount()
        {
            CurrentRoomMonsterKilledCount++;
        }

        public void RequestGenerateNextFloor() {
            generator.GenerateNextFloor(CurrentFloor);
        }
        
        #region Events
        
        public void InvokeEventMonsterKilledEvent(MonsterStatus key) => OnMonsterKilled?.Invoke();

        public void InvokeEventPlayerKilledEvent(Player key) => OnPlayerKilled?.Invoke();

        public void InvokeEventClearedRoomEvent(RoomType roomType) => OnClearedRoom?.Invoke(roomType);
        
        public void InvokeRoomEnteringEvent(RoomType roomType) => OnRoomEntering?.Invoke(roomType);

        public void InvokeRoomEnteringFirstEvent(RoomType roomType) => OnRoomFirstEntering?.Invoke(roomType);

        public void AddListenerRoomEnteringEvent(UnityAction<RoomType> action) => OnRoomEntering.AddListener(action);

        public void AddListenerClearedRoomEvent(UnityAction<RoomType> action) => OnClearedRoom.AddListener(action);
        
        public void AddEventToOpeningSkillSelectPanel(UnityAction action) => OnOpeningSkillSelectPanel.AddListener(action);
        
        public void AddEventToSkillSelectCompleted(UnityAction action) => OnSkillSelectCompleted.AddListener(action);
        
        public void AddEventToMapGenerateCompleted(UnityAction<Field> action) => onMapGenerateCompleted.AddListener(action);
        
        public void InvokeMapGenerateCompleted(Field field) => onMapGenerateCompleted?.Invoke(field);
        
        public void RemoveEventToMapGenerateCompleted(UnityAction<Field> action) => onMapGenerateCompleted.RemoveListener(action);
        
        public void AddEventToMapDisposeBefore(UnityAction action) => onMapDisposeBefore.AddListener(action);
        
        public void InvokeMapDisposeBefore() => onMapDisposeBefore?.Invoke();
        
        public void RemoveEventToMapDisposeBefore(UnityAction action) => onMapDisposeBefore.RemoveListener(action);

        public void InvokeOpeningSkillSelectPanel() => OnOpeningSkillSelectPanel?.Invoke();
        
        public void InvokeSkillSelectCompleted() => OnSkillSelectCompleted?.Invoke();
        
        #endregion
    }
}
