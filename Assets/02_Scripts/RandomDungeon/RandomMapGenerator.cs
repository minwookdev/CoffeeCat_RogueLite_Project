using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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

        [SerializeField] private int seed = default;
        [SerializeField] private Button generateButton = default;
        [SerializeField] private FieldView fieldView = default;
        [SerializeField] private BluePrintWithWeight[] bluePrints = default;

        public Field field { get; private set; }
        public bool IsGenerateCompleted { get; set; } = false;

        [Title("Generate Options")]
        public FieldBluePrint DefinitiveBluePrint = null; // 확정 생성 BluePrint
        [SerializeField] private BluePrintQueue bluePrintQueue = default;
        
        [Title("Events")]
        [SerializeField] private UnityEvent<Field> onGeneratedMapCompleted = null;
        [SerializeField] private UnityEvent onDisposeMapBefore = null;

        [Title("Bake PathFind Grid")]
        public bool IsBakePathFindGrid = false;
        [SerializeField] private PathFindGrid pathFindGrid;
        
        [Title("Debugging Options", TitleAlignment = TitleAlignments.Centered)]
        public bool IsDisplayRoomType = false;
        public bool IsDisplaySectionRectDrawer = false;
        public bool IsDisplaySectionIndex = false;
        public bool IsDisplayRoomRectDrawer = false;
        public Color RoomDrawerColor = Color.green;
        public Color SectionDrawerColor = Color.white;
        
        private void Start()
        {
            // Init Random Seed
            UnityRandom.InitState(seed);
            
            // Generate Map
            ExecuteGenerate();
            
            // Generate PathFind Grid
            if (IsBakePathFindGrid) {
                pathFindGrid.CreateGridDictionary(this);
            }
            
            // Add Generate Button Event
            generateButton.onClick.AddListener(ExecuteGenerate);
            if (bluePrintQueue.IsGrantSkillOnStart)
            {
                var player = RogueLiteManager.Instance.SpawnedPlayer;
                if (player)
                {  
                   player.EnableSkillSelect(); 
                }
            }
            InitDebugs();
            return;

            void ExecuteGenerate()
            {
                onDisposeMapBefore?.Invoke();
                
                var targetFieldBluePrint = (DefinitiveBluePrint) ? DefinitiveBluePrint : Raffle().BluePrint;
                Create(targetFieldBluePrint);
                
                onGeneratedMapCompleted?.Invoke(field);
            }
        }
        
        private void Create(BluePrintWithWeight bluePrintWeight) {
            Create(bluePrintWeight.BluePrint);
        }

        private void Create(FieldBluePrint bluePrint) {
            IsGenerateCompleted = false;
            field = FieldBuilder.Build(bluePrint);
            fieldView.DrawDungeon(field);
            IsGenerateCompleted = true;

            // CatLog.Log($"Map Generated Completed. MapType : {bluePrint.MapType}");
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
        }
        
        #endregion
    }
}