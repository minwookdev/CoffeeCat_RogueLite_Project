using System;
using System.Linq;
using CoffeeCat;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityRandom = UnityEngine.Random;
using PathFindGrid = CoffeeCat.Pathfinding2D.Grid;

namespace RandomDungeonWithBluePrint
{
    public class RandomMapGenerator : MonoBehaviour
    {
        [Serializable]
        public class BluePrintWithWeight
        {
            public FieldBluePrint BluePrint = default;
            public int Weight = default;
        }
        
        [Title("Generate Options")]
        [SerializeField] private int seed = default;
        [SerializeField] private FieldView fieldView = null;
        [SerializeField] public BluePrintQueue bluePrintQueue = null;
        [SerializeField] private FieldBluePrint TestBluePrint = null; // 확정 생성 BluePrint
        [SerializeField] private BluePrintWithWeight[] bluePrints = null;
        public Field field { get; private set; }
        public BluePrintQueue BluePrintQueue => bluePrintQueue;

        [Title("Bake PathFind Grid (Not Using)")]
        [SerializeField, ReadOnly] private bool isBakePathFindGrid = false;
        [SerializeField, ReadOnly] private PathFindGrid pathFindGrid;
        
        [Title("Debugging Options", TitleAlignment = TitleAlignments.Centered)]
        public bool IsDisplayRoomType = false;
        public bool IsDisplaySectionRectDrawer = false;
        public bool IsDisplaySectionIndex = false;
        public bool IsDisplayRoomRectDrawer = false;
        private bool initializedDebugObservable = false;
        public Color RoomDrawerColor = Color.green;
        public Color SectionDrawerColor = Color.white;

        /*private void Awake() {
            // Init Random Seed
            UnityRandom.InitState(seed);
        }*/

        public void GenerateNextFloor(int currentFloor) {
            var normalMapBluePrints = bluePrintQueue.NormalMapBluePrints;
            if (normalMapBluePrints.Length <= currentFloor) {
                CatLog.Log("Reached The Maximum Floor.");
                return;
            }
            
            var bluePrint = normalMapBluePrints[currentFloor];
            ExecuteGenerate(bluePrint);
        }
        
        private void ExecuteGenerate(FieldBluePrint bluePrint) {
            if (TestBluePrint != null) {
                bluePrint = TestBluePrint;
                CatLog.WLog("Override Test BluePrint: [GENERATE TEST MODE MAP");
            }
            
            if (!bluePrint) {
                CatLog.ELog("BluePrint Is Null");
                return;
            }
            
            // Invoke Event Before Dispose Generated Map  
            if (field != null) {
                StageManager.Instance.InvokeMapDisposeBefore();
            }
            
            // Clear And ReGenerate Dungeon Map
            Create(bluePrint);
            StageManager.Instance.InvokeMapGenerateCompleted(field);
            
            // Bake PathFind Grid
            if (isBakePathFindGrid) {
                pathFindGrid.CreateGridDictionary(this);
            }
            
            InitDebugs();
        }
        
        private void Create(BluePrintWithWeight bluePrintWeight) {
            Create(bluePrintWeight.BluePrint);
        }

        private void Create(FieldBluePrint bluePrint) {
            field?.Dispose();
            field = FieldBuilder.Build(bluePrint);
            fieldView.DrawDungeon(field);
        }

        private BluePrintWithWeight Raffle()
        {
            var candidate = bluePrints.ToList();
            var rand = UnityRandom.Range(0, candidate.Sum(c => c.Weight));
            var pick = 0;
            for (var i = 0; i < candidate.Count; i++)
            {
                if (rand < candidate[i].Weight)
                {
                    pick = i;
                    break;
                }

                rand -= candidate[i].Weight;
            }

            return candidate[pick];
        }

        #region Debug_Drawer
        
        private void InitDebugs() {
            if (IsDisplayRoomType) {
                DisplayRoomType();
            }

            if (IsDisplaySectionIndex) {
                DisplaySectionIndex();
            }
            
            if (initializedDebugObservable)
                return;
            
            // Init Observable Room Type
            this.ObserveEveryValueChanged(_ => IsDisplayRoomType)
                .Skip(0)
                .TakeUntilDestroy(this)
                .Subscribe(isEnable => {
                    if (isEnable) {
                        DisplayRoomType();
                    }
                    else {
                        ClearRoomTypeText();
                    }
                })
                .AddTo(this);
            
            // Init Observable Section Index
            this.ObserveEveryValueChanged(_ => IsDisplaySectionIndex)
                .Skip(0)
                .TakeUntilDestroy(this)
                .Subscribe(isEnable => {
                    if (isEnable) {
                        DisplaySectionIndex();
                    }
                    else {
                        ClearSectionIndexText();
                    }
                })
                .AddTo(this);
            
            initializedDebugObservable = true;
        }
        
        /// <summary>
        /// RoomType을 표시
        /// </summary>
        private void DisplayRoomType() {
            ClearRoomTypeText();
            
            foreach (var room in field.Rooms) {
                Vector2 spawnPoint = new Vector2(room.Rect.xMin, room.Rect.yMin);
                var text = ObjectPoolManager.Instance.Spawn<TextMeshPro>("editor_text_room_type", spawnPoint, Quaternion.identity);
                text.SetText(room.RoomType.ToStringExtended());
            }
        }

        private void ClearRoomTypeText() {
            if (!ObjectPoolManager.Instance.IsExistInPoolDictionary("editor_text_room_type"))
                return;
            ObjectPoolManager.Instance.DespawnAll("editor_text_room_type");
        }

        private void DisplaySectionIndex() {
            ClearSectionIndexText();
            
            var sections = field?.Sections;
            if (sections == null)
                return;
            
            for (int i = 0; i < sections.Count; i++) {
                var rect = sections[i].Rect;
                var point = new Vector2(rect.xMin, rect.yMax);
                var text = ObjectPoolManager.Instance.Spawn<TextMeshPro>("editor_text_section_index", point, Quaternion.identity);
                text.SetText("< " + sections[i].Index.ToString() + " >");
            }
        }
        
        private void ClearSectionIndexText() {
            if (!ObjectPoolManager.Instance.IsExistInPoolDictionary("editor_text_section_index"))
                return;
            ObjectPoolManager.Instance.DespawnAll("editor_text_section_index");
        }
        
        private void OnDrawGizmos() {
#if UNITY_EDITOR
            if (field == null) {
                return;
            }
            
            // Draw Room Rect
            if (IsDisplaySectionRectDrawer) {
                var sections = field?.Sections;
                if (sections == null)
                    return;
                
                Gizmos.color = SectionDrawerColor;
                for (int i = 0; i < sections.Count; i++) {
                    if (sections[i] == null)
                        continue;
                    var rect = sections[i].Rect;
                    var center = new Vector3(rect.center.x, rect.center.y, 0f);
                    var size = new Vector3(rect.width, rect.height, 0f);
                    Gizmos.DrawWireCube(center, size);
                }
            }
            
            // Draw Section Rect
            if (IsDisplayRoomRectDrawer) {
                var rooms = field?.Rooms;
                if (rooms == null)
                    return;
                
                Gizmos.color = RoomDrawerColor;
                for (var i = 0; i < rooms.Count; i++) {
                    var room = rooms[i];
                    if (room == null)
                        continue;
                    RectInt floorRectInt = room.FloorRectInt;
                    Vector3 centerVec = new Vector3(floorRectInt.center.x, floorRectInt.center.y, 0f);
                    Vector3 sizeVec = new Vector3(floorRectInt.width, floorRectInt.height, 0f);
                    Gizmos.DrawWireCube(centerVec, sizeVec);
                }
            }

            // Restore Gizmos Color 
            Gizmos.color = Color.white;
#endif
        }
        
        #endregion
    }
}