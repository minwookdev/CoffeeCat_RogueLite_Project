using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using CoffeeCat.Utils;
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
        
        [Title("Events")]
        [SerializeField] private UnityEvent<Field> onGeneratedMapCompleted = null;
        [SerializeField] private UnityEvent onDisposeMapBefore = null;

        [Title("Bake PathFind Grid")]
        public bool IsBakePathFindGrid = false;
        [SerializeField] private PathFindGrid pathFindGrid;
        
        [Title("Debug")]
        public bool IsDisplayRoomType = false;
        public bool IsDisplayMonsterSpawnPoint = false;

        private void Awake()
        {
            UnityRandom.InitState(seed);
            
            // Add Event to Generate Button
            generateButton.onClick.AddListener(() =>
            {
                onDisposeMapBefore.Invoke();
                
                //Create(Raffle());
                ExecuteGenerate();
                DisplayRoomType();
                DisplayMonsterSpawnPoint();
                
                onGeneratedMapCompleted?.Invoke(field);
            });

            // 초기 맵 생성 실행
            ExecuteGenerate();

            void ExecuteGenerate() {
                var targetFieldBluePrint = (DefinitiveBluePrint) ? DefinitiveBluePrint : Raffle().BluePrint;
                Create(targetFieldBluePrint);
            }
        }

        private void Start()
        {
            // Awake에서 맵이 생성되었다면 이벤트 호출
            if (IsGenerateCompleted) {
                // UnityEvent에서 Manager에 접근할 가능성이 있기 때문에 Awake에서 호출을 피하기 위함
                onGeneratedMapCompleted?.Invoke(field);
                DisplayRoomType();
                DisplayMonsterSpawnPoint();
            }
            
            // Generate PathFind Grid
            if (IsBakePathFindGrid) {
                pathFindGrid.CreateGridDictionary(this);
            }

            var gatePoints = field.Gates.Select(gate => gate.Position);
            foreach (var point in gatePoints) {
                Vector2 spawnPoint = new Vector2(point.x, point.y);
                ObjectPoolManager.Instance.Spawn<Transform>("Guide_Circle2D", spawnPoint);
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

            CatLog.Log($"Map Generated Completed. MapType : {bluePrint.MapType}");
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

        /// <summary>
        /// BattleRoom의 MonsterSpawnPoint를 표시
        /// </summary>
        private void DisplayMonsterSpawnPoint() {
            if (!IsDisplayMonsterSpawnPoint) {
                return;
            }
            
            string key = "Guide_Circle2D";
            if (!ObjectPoolManager.Instance.IsExistInPoolDictionary(key)) {
                return;
            }

            ObjectPoolManager.Instance.DespawnAll(key);
            
            foreach (var room in field.Rooms) {
                if (room.RoomType != RoomType.MonsterSpawnRoom || room.RoomData is not BattleRoomData battleRoom) {
                    continue;
                }

                foreach (var position in battleRoom.SpawnPositions) {
                    ObjectPoolManager.Instance.Spawn<Transform>(key, position, Quaternion.identity);
                }
            }
        }
        
        /// <summary>
        /// RoomType을 표시
        /// </summary>
        private void DisplayRoomType() {
            if (!IsDisplayRoomType) {
                return;
            }

            string key = "Guide_Text";
            if (!ObjectPoolManager.Instance.IsExistInPoolDictionary(key))
                return;
            ObjectPoolManager.Instance.DespawnAll(key);
            
            foreach (var room in field.Rooms) {
                Vector2 spawnPoint = new Vector2(room.Rect.xMin, room.Rect.yMin);
                var text = ObjectPoolManager.Instance.Spawn<TextMeshPro>(key, spawnPoint, Quaternion.identity);
                text.SetText(room.RoomType.ToStringExtended());
            }
        }

        private void OnDrawGizmos() {
            if (field == null) {
                return;
            }

            foreach (var room in field.Rooms) {
                RectInt floorRectInt = room.FloorRectInt;
                Vector3 centerVec = new Vector3(floorRectInt.center.x, floorRectInt.center.y, 0f);
                Vector3 sizeVec = new Vector3(floorRectInt.width, floorRectInt.height, 0f);
                Gizmos.DrawWireCube(centerVec, sizeVec);
            }
        }
    }
}