using CoffeeCat;
using CoffeeCat.Utils;
using UnityEngine;

namespace RandomDungeonWithBluePrint {
    public class GateObject : MonoBehaviour {
        // Managed Field
        [SerializeField] private GameObject lockedObject = null;
        
        // Fields
        private Transform tr = null;
        private BoxCollider2D boxCollider = null;
        private Room refRoom = null;
        
        public void Initialize(int direction, Vector2Int position, Room room) {
            refRoom = room;
            tr = GetComponent<Transform>();
            tr.position = GetPosition(direction, position);
            tr.rotation = GetRotation(direction);
            boxCollider = GetComponent<BoxCollider2D>();
            
            Vector2 GetPosition(int constDirection, Vector2Int worldPos) {
                switch (constDirection) {
                    case Constants.Direction.Up:     
                        return new Vector2(worldPos.x + Constants.TileRadius, worldPos.y + Constants.TileDiameter * 2f);
                    case Constants.Direction.Down:   
                        return new Vector2(worldPos.x + Constants.TileRadius, worldPos.y - Constants.TileDiameter);
                    case Constants.Direction.Left:   
                        return new Vector2(worldPos.x + Constants.TileDiameter * 2f, worldPos.y + Constants.TileRadius);
                    case Constants.Direction.Right : 
                        return new Vector2(worldPos.x - Constants.TileDiameter, worldPos.y + Constants.TileRadius);
                    default: PrintErrorLog();
                        return default;
                }
            }

            Quaternion GetRotation(int constDirection) {
                switch (constDirection) {
                    case Constants.Direction.Up:    return Quaternion.Euler(0f, 0f, 180f);
                    case Constants.Direction.Down:  return Quaternion.identity;
                    case Constants.Direction.Right: return Quaternion.Euler(0f, 0f, -90f);
                    case Constants.Direction.Left:  return Quaternion.Euler(0f, 0f, 90f);
                    default: PrintErrorLog();
                        return default;
                }
            }

            void PrintErrorLog() {
                CatLog.ELog("Not Implemented This Constant Direction in Gate Object !");
            }
        }

        public void Lock(bool isLock) {
            lockedObject.gameObject.SetActive(isLock);
            boxCollider.isTrigger = !isLock;
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            // Check Only Players Collider
            if (!collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player"))) {
                return;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            // Check Only Players Collider
            if (!collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
                return;

            // Player Position
            Vector2 playerPosition = collision.transform.position;
            // Entered Player in Room
            if (refRoom.IsInsideRoom(playerPosition)) {
                if (refRoom.RoomData.IsPlayerInside) {
                    // 플레이어가 이미 방에 있던 상태
                    return;
                }
                refRoom.RoomData.EnteredPlayer();
                StageManager.Inst.SetPlayersRoom(refRoom);
            }
            // Leaves Player From Room
            else {
                if (!refRoom.RoomData.IsPlayerInside) {
                    // 플레이어가 이미 방에서 탈출한 상태 Or 아직 방에 진입하지 않은 상태
                    return;
                }
                refRoom.RoomData.LeavesPlayer();
                StageManager.Inst.ClearPlayersRoom(refRoom);
            }
        }
    }
}
