using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
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
        [TabGroup("Events Map"), SerializeField] public UnityEvent<RoomDataStruct> OnRoomEntering = new();
        [TabGroup("Events Map"), SerializeField] public UnityEvent<RoomDataStruct> OnRoomLeft = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnRoomFirstEntering = new();
        [TabGroup("Events Map"), SerializeField] public UnityEvent<RoomDataStruct> OnClearedRoom = null;
        
        [Title("Events Player", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Events Player"), SerializeField] public UnityEvent OnPlayerKilled = new();
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
                // player.EnableSkillSelect();
            }
            
            // Debug
            InputManager.BindEvasionInput(RequestGenerateNextFloor);
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
            InvokeMapDisposeBefore();
            mapGen.ClearMap();
            
            SceneManager.Inst.LoadSceneSingle(SceneName.TownScene, true, false);
        }
        
        #region Events
        
        public void InvokeMonsterKilledByPlayer(float exp) => OnMonsterKilledByPlayer.Invoke(exp);
        
        public void AddListenerMonsterKilledByPlayer(UnityAction<float> action) => OnMonsterKilledByPlayer.AddListener(action);
        
        public void RemoveListenerMonsterKilledByPlayer(UnityAction<float> action) => OnMonsterKilledByPlayer.RemoveListener(action);

        public void InvokeEventPlayerKilledEvent(Player key) => OnPlayerKilled.Invoke();

        public void InvokeEventClearedRoomEvent(RoomDataStruct roomType) => OnClearedRoom.Invoke(roomType);
        
        public void InvokeRoomEnteringEvent(RoomDataStruct roomType) => OnRoomEntering.Invoke(roomType);

        public void InvokeRoomEnteringFirstEvent(RoomDataStruct roomType) => OnRoomFirstEntering.Invoke(roomType);

        public void InvokeRoomLeftEvent(RoomDataStruct roomType) => OnRoomLeft.Invoke(roomType);
        
        public void AddListenerRoomEnteringEvent(UnityAction<RoomDataStruct> action) => OnRoomEntering.AddListener(action);

        public void AddListenerRoomFirstEnteringEvent(UnityAction<RoomDataStruct> action) => OnRoomFirstEntering.AddListener(action);

        public void AddListenerRoomLeftEvent(UnityAction<RoomDataStruct> action) => OnRoomLeft.AddListener(action);
        
        public void AddListenerClearedRoomEvent(UnityAction<RoomDataStruct> action) => OnClearedRoom.AddListener(action);
        
        public void AddEventToOpeningSkillSelectPanel(UnityAction action) => OnOpeningSkillSelectPanel.AddListener(action);
        
        public void AddEventToSkillSelectCompleted(UnityAction action) => OnSkillSelectCompleted.AddListener(action);
        
        public void AddEventToMapGenerateCompleted(UnityAction<Field> action) => onMapGenerateCompleted.AddListener(action);
        
        public void InvokeMapGenerateCompleted(Field field) => onMapGenerateCompleted.Invoke(field);
        
        public void RemoveEventToMapGenerateCompleted(UnityAction<Field> action) => onMapGenerateCompleted.RemoveListener(action);
        
        public void AddEventToMapDisposeBefore(UnityAction action) => onMapDisposeBefore.AddListener(action);
        
        public void InvokeMapDisposeBefore() => onMapDisposeBefore.Invoke();
        
        public void RemoveEventToMapDisposeBefore(UnityAction action) => onMapDisposeBefore.RemoveListener(action);

        public void InvokeOpeningSkillSelectPanel() => OnOpeningSkillSelectPanel.Invoke();
        
        public void InvokeSkillSelectCompleted() => OnSkillSelectCompleted.Invoke();
        
        // Player Events
        
        public void AddListenerIncreasePlayerExp(UnityAction<float, float> action) => OnIncreasePlayerExp.AddListener(action);
        
        public void InvokeIncreasePlayerExp(float currentExp, float maxExp) => OnIncreasePlayerExp?.Invoke(currentExp, maxExp);
        
        public void RemoveListenerIncreasePlayerExp(UnityAction<float, float> action) => OnIncreasePlayerExp.RemoveListener(action);
        
        public void AddListenerIncreasePlayerHP(UnityAction<float, float> action) => OnIncreasePlayerHP.AddListener(action);
        
        public void InvokeIncreasePlayerHP(float hp, float maxHp) => OnIncreasePlayerHP.Invoke(hp, maxHp);
        
        public void RemoveListenerIncreasePlayerHP(UnityAction<float, float> action) => OnIncreasePlayerHP.RemoveListener(action);
        
        public void AddListenerDecreasePlayerHP(UnityAction<float, float> action) => OnDecreasePlayerHP.AddListener(action);
        
        public void InvokeDecreasePlayerHP(float hp, float maxHp) => OnDecreasePlayerHP.Invoke(hp, maxHp);
        
        public void RemoveListenerDecreasePlayerHP(UnityAction<float, float> action) => OnDecreasePlayerHP.RemoveListener(action);
        
        public void AddListenerPlayerLevelUp(UnityAction action) => OnPlayerLevelUp.AddListener(action);
        
        public void InvokePlayerLevelUp() => OnPlayerLevelUp.Invoke();
        
        public void RemoveListenerPlayerLevelUp(UnityAction action) => OnPlayerLevelUp.RemoveListener(action);
        
        #endregion
    }
}
