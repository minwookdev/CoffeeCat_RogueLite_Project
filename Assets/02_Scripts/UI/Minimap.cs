using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using CoffeeCat.Utils;
using RandomDungeonWithBluePrint;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat
{
    public class Minimap : MonoBehaviour
    {
        private readonly Dictionary<int, Minimap_Room> minimapRooms = new Dictionary<int, Minimap_Room>();
        private List<Minimap_Branch> minimapBranches = new List<Minimap_Branch>();
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
            var requests = new SafeLoader.Req[] {
                new() {
                    Key = branchKey,
                    SpawnCount = 15
                },
                new() {
                    Key = roomPanelKey,
                    SpawnCount = 15
                }
            };

            SafeLoader.RegistAll(requests, success => {
                if (!success) {
                    CatLog.ELog("Minimap Generating Failed !");
                    return;
                }

                MinimapGenerate(field);
            });
        }
        
        private void MinimapGenerate(Field field)
        {
            minimapRooms.Clear();
            minimapBranches.Clear();
            panelTr.sizeDelta = new Vector2(field.Size.x, field.Size.y) * minimapRatio;
            
            foreach (var room in field.Rooms)
            {
                var roomObj = ObjectPoolManager.Inst.Spawn(roomPanelKey, panelTr);
                var minimapRoom = roomObj.GetComponent<Minimap_Room>();
                minimapRoom.Initialize(room, minimapRatio);
                minimapRooms.Add(room.RoomData.RoomIndex, minimapRoom);
            }

            foreach (var connection in field.Connections)
            {
                var spawnObj = ObjectPoolManager.Inst.Spawn(branchKey, branchTr);
                var minimapBranch = spawnObj.GetComponent<Minimap_Branch>();
                var fromSection = field.GetSection(connection.From);
                var toSection = field.GetSection(connection.To);
                
                minimapBranch.SetBranch(fromSection, toSection, minimapRatio);
                minimapBranch.gameObject.SetActive(false);
                minimapBranches.Add(minimapBranch);
            }

            ActivePlayerSpawnRoomPanel();
            StageManager.Inst.AddListenerRoomEnteringEvent(EnterdRoom);
            StageManager.Inst.AddListenerRoomLeftEvent(LeftRoom);
            StageManager.Inst.AddListenerClearedRoomEvent(ClearedRoom);
        }

        private void ActivePlayerSpawnRoomPanel()
        {
            var spawnRoom = StageManager.Inst.PlayerCurrentRoom;
            var roomPanel = minimapRooms[spawnRoom.RoomData.RoomIndex];
            roomPanel.EnterdRoom();
            
            foreach (var branch in minimapBranches)
            {
                branch.EnterdRoom(spawnRoom.RoomData.RoomIndex);
                branch.CheckConnectSection();
            }
        }

        private void EnterdRoom(RoomDataStruct roomData)
        {
            var roomPanel = minimapRooms[roomData.RoomIndex];
            roomPanel.EnterdRoom();
            
            foreach (var branch in minimapBranches)
            {
                branch.EnterdRoom(roomData.RoomIndex);
                branch.CheckConnectSection();
            }
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
            gameObject.SetActive(true);
        }
        
        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}