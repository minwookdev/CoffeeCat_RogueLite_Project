using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using RandomDungeonWithBluePrint;

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

    public class StageManager : SceneSingleton<StageManager> {
        [Title("StageManager", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Requires"), SerializeField] private RandomMapGenerator mapGen;
        [TabGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentRoomMonsterKilledCount { get; private set; } = 0;
        [TabGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentFloorMonsterKillCount { get; private set; } = 0;
        [TabGroup("Requires"), ShowInInspector, ReadOnly] public int TotalMonsterKilledCount { get; private set; } = 0;
        [TabGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentFloor { get; private set; } = 0;
        private Room playerCurrentRoom = null;
        public Room PlayerCurrentRoom => playerCurrentRoom;
        public RandomMapGenerator MapGen => mapGen;
        public bool IsPlayerInsideRoom => playerCurrentRoom != null;
        
        // TODO: Clean Up Many Events
        [Title("Events Map", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Events Map"), SerializeField] private UnityEvent<Field> onMapGenerateCompleted = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent onMapDisposeBefore = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnRoomEntering = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnRoomLeft = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnRoomFirstEntering = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnClearedRoom = null;
        
        [Title("Events Player", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnPlayerKilled = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnOpeningSkillSelectPanel = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<float, float> OnIncreasePlayerExp = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<float, float> OnIncreasePlayerHP = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<float, float> OnDecreasePlayerHP = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnSkillSelectCompleted = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnPlayerLevelUp = new();

        [Title("Events Monster", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Events Monster"), SerializeField] private UnityEvent<float> OnMonsterKilledByPlayer = new();
        
        private void Start() {
            mapGen.GenerateNextFloor(CurrentFloor);
            var queue = mapGen.BluePrintQueue;
            if (!queue) 
                return;
            if (queue.IsGrantSkillOnStart) {
                var player = RogueLiteManager.Inst.SpawnedPlayer;
                player.EnableSkillSelect();
            }
        }

        private void Update()
        {
            if (!RogueLiteManager.Inst.SpawnedPlayer) {
                return;
            }
            
            // 카메라 업데이트는 카메라에서 !
            CameraMovementUpdate();
        }

        private void CameraMovementUpdate() {
            Vector2 playerPos = RogueLiteManager.Inst.SpawnedPlayer.transform.position;
            Camera.main.transform.position = new Vector3(playerPos.x, playerPos.y, Camera.main.transform.position.z);
        }

        public void CreateGate(Field field)
        {
            var grouppedGates = field.Gates.GroupBy(g => g.Room);
            var spawnedGateList = new List<GateObject>();
            foreach (var group in grouppedGates)
            {
                var room = group.Key;
                spawnedGateList.Clear();
                
                foreach (var gate in group)
                {
                    var spawnedGate = ObjectPoolManager.Inst.Spawn<GateObject>("dungeon_door", Vector3.zero);
                    spawnedGate.Initialize(gate.Direction, gate.Position, gate.Room);
                    spawnedGateList.Add(spawnedGate);
                }
                room.SetGateObjects(spawnedGateList.ToArray());
            }
        }

        /// <summary>
        /// Despawn All Gates
        /// </summary>
        public void DespawnGates() {
            ObjectPoolManager.Inst.DespawnAll("dungeon_door");
        }

        public void SetPlayer(Field field) {
            // Not Founded Entry Room
            if (!field.TryFindRoomFromType(RoomType.PlayerSpawnRoom, out Room result)) {
                CatLog.WLog("Not Exist PlayerSpawn Room !");
                return;
            }
            
            // Set RogueLite Player Object
            RogueLiteManager.Inst.SetPlayerOnEnteredDungeon(result.Rect.center);
            playerCurrentRoom = result;
            playerCurrentRoom.RoomData.EnteredPlayer();
        }

        /// <summary>
        /// Despawn Players
        /// </summary>
        public void DisablePlayer() {
            RogueLiteManager.Inst.DisablePlayer();
        }

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
            RogueLiteManager.Inst.EnableInput();
        }
        
        public void DisableInput() {
            RogueLiteManager.Inst.DisableInput();
        }
        
        public void RestoreTimeScale() {
            RogueLiteManager.Inst.RestoreTimeScale();
        }
        
        public void TimeScaleZero() {
            RogueLiteManager.Inst.TimeScaleZero();
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
            var nextFloor = CurrentFloor + 1;
            mapGen.GenerateNextFloor(nextFloor);
            
            // Increase Current Floor After Generatr Next Map Successed 
            CurrentFloor = nextFloor;
        }

        public void RequestToTownScene() {
            InvokeEventMapDisposeBefore();
            mapGen.ClearMap();
            
            SceneManager.Inst.LoadSceneSingle(SceneName.TownScene, true, false);
        }
        
        #region Stage Events
        
        // Add Event ===================================================================================================
        
        public void AddEventRoomFirstEnteringEvent(UnityAction<RoomDataStruct> action) => OnRoomFirstEntering.AddListener(action);
        
        public void AddEventRoomEnteringEvent(UnityAction<RoomDataStruct> action) => OnRoomEntering.AddListener(action);

        public void AddEventRoomLeftEvent(UnityAction<RoomDataStruct> action) => OnRoomLeft.AddListener(action);
        
        public void AddEventClearedRoomEvent(UnityAction<RoomDataStruct> action) => OnClearedRoom.AddListener(action);
        
        public void AddEventMapGenerateCompleted(UnityAction<Field> action) => onMapGenerateCompleted.AddListener(action);
        
        public void AddEventMapDisposeBefore(UnityAction action) => onMapDisposeBefore.AddListener(action);
        
        // Remove Envet ================================================================================================
        
        public void RemoveEventMapGenerateCompleted(UnityAction<Field> action) => onMapGenerateCompleted.RemoveListener(action);
        
        public void RemoveEventMapDisposeBefore(UnityAction action) => onMapDisposeBefore.RemoveListener(action);
        
        public void RemoveEventRoomEntering(UnityAction<RoomDataStruct> action) => OnRoomEntering.RemoveListener(action);
        
        public void RemoveEventRoomLeft(UnityAction<RoomDataStruct> action) => OnRoomLeft.RemoveListener(action);
        
        public void RemoveEventRoomFirstEntering(UnityAction<RoomDataStruct> action) => OnRoomFirstEntering.RemoveListener(action);
        
        public void RemoveEventClearedRoom(UnityAction<RoomDataStruct> action) => OnClearedRoom.RemoveListener(action);
        
        // Invoke Event ================================================================================================
        
        public void InvokeEventRoomEnteringFirst(RoomDataStruct roomType) => OnRoomFirstEntering.Invoke(roomType);
        
        public void InvokeEventRoomEntering(RoomDataStruct roomType) => OnRoomEntering.Invoke(roomType);
        
        public void InvokeEventRoomLeft(RoomDataStruct roomType) => OnRoomLeft.Invoke(roomType);
        
        public void InvokeEventClearedRoom(RoomDataStruct roomType) => OnClearedRoom.Invoke(roomType);
        
        public void InvokeEventMapGenerateCompleted(Field field) => onMapGenerateCompleted.Invoke(field);

        public void InvokeEventMapDisposeBefore() => onMapDisposeBefore.Invoke();
        
        // =============================================================================================================
        
        #endregion
        
        #region Player Events
        
        // Add Event ===================================================================================================  
        
        public void AddEventIncreasePlayerExp(UnityAction<float, float> action) => OnIncreasePlayerExp.AddListener(action);
        
        public void AddvEventIncreasePlayerHP(UnityAction<float, float> action) => OnIncreasePlayerHP.AddListener(action);
        
        public void AddEventDecreasePlayerHP(UnityAction<float, float> action) => OnDecreasePlayerHP.AddListener(action);
        
        public void AddEventPlayerLevelUp(UnityAction action) => OnPlayerLevelUp.AddListener(action);
        
        public void AddEventSkillSelectCompleted(UnityAction action) => OnSkillSelectCompleted.AddListener(action);
        
        public void AddEventOpeningSkillSelectPanel(UnityAction action) => OnOpeningSkillSelectPanel.AddListener(action);
        
        public void AddEventOnPlayerKilled(UnityAction action) => OnPlayerKilled.AddListener(action);
        
        // Remove Envet ================================================================================================
        
        public void RemoveEventIncreasePlayerExp(UnityAction<float, float> action) => OnIncreasePlayerExp.RemoveListener(action);
        
        public void RemoveEventIncreasePlayerHP(UnityAction<float, float> action) => OnIncreasePlayerHP.RemoveListener(action);
        
        public void RemoveEventDecreasePlayerHP(UnityAction<float, float> action) => OnDecreasePlayerHP.RemoveListener(action);
        
        public void RemoveEventPlayerLevelUp(UnityAction action) => OnPlayerLevelUp.RemoveListener(action);
        
        // Invoke Event ================================================================================================
        
        public void InvokeEventIncreasePlayerExp(float currentExp, float maxExp) => OnIncreasePlayerExp?.Invoke(currentExp, maxExp);
        
        public void InvokeEventIncreasePlayerHP(float hp, float maxHp) => OnIncreasePlayerHP.Invoke(hp, maxHp);
        
        public void InvokeEventDecreasePlayerHP(float hp, float maxHp) => OnDecreasePlayerHP.Invoke(hp, maxHp);
        
        public void InvokeEventPlayerLevelUp() => OnPlayerLevelUp.Invoke();
        
        public void InvokeEventSkillSelectCompleted() => OnSkillSelectCompleted.Invoke();
        
        public void InvokeEventOpeningSkillSelectPanel() => OnOpeningSkillSelectPanel.Invoke();
        
        public void InvokeEventPlayerKilled() => OnPlayerKilled.Invoke();
        
        // =============================================================================================================
        
        #endregion
        
        #region Monster Events
        
        // Add Event ===================================================================================================  
        
        public void AddEventMonsterKilledByPlayer(UnityAction<float> action) => OnMonsterKilledByPlayer.AddListener(action);
        
        // Remove Envet ================================================================================================
        
        public void RemoveEventMonsterKilledByPlayer(UnityAction<float> action) => OnMonsterKilledByPlayer.RemoveListener(action);
        
        // Invoke Event ================================================================================================
        
        public void InvokeEventMonsterKilledByPlayer(float exp) => OnMonsterKilledByPlayer.Invoke(exp);
        
        // =============================================================================================================
        
        #endregion
    }
}
