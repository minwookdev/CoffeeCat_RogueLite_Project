using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using CoffeeCat.Utils;
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using Spine.Unity.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat
{
    public class Minimap : MonoBehaviour
    {
        private readonly Dictionary<int, Minimap_Room> minimapRooms = new Dictionary<int, Minimap_Room>();
        private const float minimapRatio = 10f;
        private const string roomPanelKey = "RoomPanel";
        private const string branchKey = "MinimapBranch";
        
        [SerializeField] private RectTransform panelTr = null;
        [SerializeField] private RectTransform branchTr = null;
        [SerializeField] private Button btnClose = null;
        
        private void Start()
        {
            btnClose.onClick.AddListener(Close);
        }

        public void Initialize(Field field)
        {
            StartCoroutine(WaitLoadResources(field));
        }
        
        private void RoadResources()
        {
            SafeLoader.RequestRegist(branchKey, spawnCount: 15, onCompleted: complete =>
            {
                if (!complete)
                    CatLog.WLog("Minimap : MinimapBranch Load Failed");
            });
            
            SafeLoader.RequestRegist(roomPanelKey, spawnCount: 15, onCompleted: complete =>
            {
                if (!complete)
                    CatLog.WLog("Minimap : RoomPanel Load Failed");
            });
        }
        
        private IEnumerator WaitLoadResources(Field field)
        {
            RoadResources();
            yield return new WaitUntil(() => ObjectPoolManager.Instance.IsExistInPoolDictionary(branchKey));
            yield return new WaitUntil(() => ObjectPoolManager.Instance.IsExistInPoolDictionary(roomPanelKey));
            MinimapGenerate(field);
        }
        
        private void MinimapGenerate(Field field)
        {
            minimapRooms.Clear();
            panelTr.sizeDelta = new Vector2(field.Size.x, field.Size.y) * minimapRatio;
            
            foreach (var room in field.Rooms)
            {
                var roomObj = ObjectPoolManager.Instance.Spawn(roomPanelKey, panelTr);
                var minimapRoom = roomObj.GetComponent<Minimap_Room>();
                minimapRoom.Initialize(room, minimapRatio);
                minimapRooms.Add(room.RoomData.RoomIndex, minimapRoom);
            }

            foreach (var connection in field.Connections)
            {
                var spawnBranchObj = ObjectPoolManager.Instance.Spawn(branchKey, branchTr);
                
                var rectTransform = spawnBranchObj.GetComponent<RectTransform>();
                rectTransform.localScale = Vector3.one;
                rectTransform.anchoredPosition = Vector2.zero;
                
                var line = spawnBranchObj.GetComponent<LineRenderer>();
                line.SetPosition(0, field.GetSection(connection.From).Rect.center * minimapRatio);
                line.SetPosition(1, field.GetSection(connection.To).Rect.center * minimapRatio);
            }

            ActivePlayerSpawnRoomPanel();
            StageManager.Instance.AddListenerRoomEnteringEvent(EnterdRoom);
            StageManager.Instance.AddListenerRoomLeftEvent(LeftRoom);
            StageManager.Instance.AddListenerClearedRoomEvent(ClearedRoom);
        }

        private void ActivePlayerSpawnRoomPanel()
        {
            var spawnRoom = StageManager.Instance.PlayerCurrentRoom;
            var roomPanel = minimapRooms[spawnRoom.RoomData.RoomIndex];
            roomPanel.EnterdRoom();
        }

        private void EnterdRoom(RoomDataStruct roomData)
        {
            var roomPanel = minimapRooms[roomData.RoomIndex];
            roomPanel.EnterdRoom();
        }
        
        private void LeftRoom(RoomDataStruct roomData)
        {
            var roomPanel = minimapRooms[roomData.RoomIndex];
            roomPanel.LeftRoom();
        }
        
        private void ClearedRoom(RoomDataStruct roomData)
        {
            var roomPanel = minimapRooms[roomData.RoomIndex];
            roomPanel.ClearedRoom();
        }

        public void Open() 
        {
            panelTr.gameObject.SetActive(true);
        }
        
        private void Close()
        {
            panelTr.gameObject.SetActive(false);
        }
    }
}