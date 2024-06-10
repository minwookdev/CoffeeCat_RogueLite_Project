using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using CoffeeCat.Utils;
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    // TODO : Pool Manager에 등록 방식 변경 (현재 : Caching)
    public class Minimap : MonoBehaviour
    {
        [ShowInInspector] private Dictionary<int, Minimap_Room> minimapRooms = new Dictionary<int, Minimap_Room>();
        private const float minimapRatio = 10f;
        private const string roomPanelKey = "RoomPanel";
        
        [SerializeField] private RectTransform rectTr = null;
        
        private void Start()
        {
            StageManager.Instance.AddEventToMapGenerateCompleted(MinimapGenerate);
        }

        private void MinimapGenerate(Field field)
        {
            minimapRooms.Clear();
            rectTr.sizeDelta = new Vector2(field.Size.x, field.Size.y) * minimapRatio;
            
            foreach (var room in field.Rooms)
            {
                var roomObj = ObjectPoolManager.Instance.Spawn(roomPanelKey, rectTr);
                var minimapRoom = roomObj.GetComponent<Minimap_Room>();
                minimapRoom.Initialize(room, minimapRatio);
                minimapRooms.Add(room.RoomData.RoomIndex, minimapRoom);
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
    }
}