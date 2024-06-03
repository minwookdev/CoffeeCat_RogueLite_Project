using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UnityEngine.UI;

namespace CoffeeCat {
    public class Joystick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler {
        [SerializeField] private Canvas canvas = null;
        [SerializeField] private RectTransform canvasRectTr = null;
        [SerializeField] private RectTransform rectTr = null;
        [SerializeField] private RectTransform pointerRectTr = null;
        [SerializeField, ReadOnly] private Vector2 joystickMoveableRange = Vector2.zero;
        [SerializeField] private bool isTouched = false;
        [SerializeField] private Image[] frameImages = null;
        private Vector2 directionVec;
        private Player player = null;

        private void Start() {
            joystickMoveableRange = new Vector2(rectTr.rect.width * 0.5f, rectTr.rect.height * 0.5f);
        }

        private void Update() {
            if (isTouched) {
                /*var anchoredPos = pointerRectTr.anchoredPosition;*/
                /*directionVec.x = anchoredPos.x switch {
                    > 0f => 1f,
                    < 0f => -1f,
                    _    => 0f
                };
                
                
                
                directionVec.y = anchoredPos.y switch {
                    > 0f => 1f,
                    < 0f => -1f,
                    _    => 0f
                };*/

                directionVec = pointerRectTr.anchoredPosition - Vector2.zero;
                directionVec.Normalize();
                UpdateFrame(directionVec);
                
                if (!player) {
                    if (!RogueLiteManager.Instance.SpawnedPlayer)
                        return;
                    player = RogueLiteManager.Instance.SpawnedPlayer;
                }
                player.Move(directionVec);
            }
        }

        public void OnDrag(PointerEventData eventData) {
            pointerRectTr.anchoredPosition = GetJoystickPosition(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            
        }

        public void OnEndDrag(PointerEventData eventData) {
            
        }
        
        public void OnPointerEnter(PointerEventData eventData) {
            
        }

        public void OnPointerExit(PointerEventData eventData) {
            
        }

        public void OnPointerDown(PointerEventData eventData) {
            isTouched = true;
            pointerRectTr.anchoredPosition = GetJoystickPosition(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            isTouched = false;
            pointerRectTr.anchoredPosition = Vector2.zero;
            directionVec = Vector2.zero;
            UpdateFrame(directionVec);
            player?.ClearMove();
        }
        
        private Vector2 GetJoystickPosition(PointerEventData eventData) {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTr, eventData.position, canvas.worldCamera, out Vector2 stickPos)) {
                return Vector2.zero;
            }

            // Correct Stick X and Y
            /*if (stickPos.x > joystickMoveableRange.x) {
                stickPos.x = joystickMoveableRange.x;
            }
            else if (stickPos.x < -joystickMoveableRange.x) {
                stickPos.x = -joystickMoveableRange.x;
            }
            if (stickPos.y > joystickMoveableRange.y) {
                stickPos.y = joystickMoveableRange.y;
            }
            else if (stickPos.y < -joystickMoveableRange.y) {
                stickPos.y = -joystickMoveableRange.y;
            }*/
            
            stickPos = ClampJoystickPosition(stickPos);
            return stickPos;
        }
        
        private Vector2 ClampJoystickPosition(Vector2 stickPos) {
            var distanceSqr = stickPos.sqrMagnitude;
            var maxDistanceSqr = 4500f;
            if (distanceSqr > maxDistanceSqr) {
                var direction = stickPos.normalized;
                stickPos = direction * Mathf.Sqrt(maxDistanceSqr);
            }
            return stickPos;
        }

        private void UpdateFrame(Vector2 direction) {
            switch (direction.x) {
                case < 0f when direction.y > 0f:
                    frameImages[0].gameObject.SetActive(true);
                    frameImages[1].gameObject.SetActive(false);
                    frameImages[2].gameObject.SetActive(false);
                    frameImages[3].gameObject.SetActive(false);
                    break;
                case > 0f when direction.y > 0f:
                    frameImages[0].gameObject.SetActive(false);
                    frameImages[1].gameObject.SetActive(true);
                    frameImages[2].gameObject.SetActive(false);
                    frameImages[3].gameObject.SetActive(false);
                    break;
                case < 0f when direction.y < 0f:
                    frameImages[0].gameObject.SetActive(false);
                    frameImages[1].gameObject.SetActive(false);
                    frameImages[2].gameObject.SetActive(true);
                    frameImages[3].gameObject.SetActive(false);
                    break;
                case > 0f when direction.y < 0f:
                    frameImages[0].gameObject.SetActive(false);
                    frameImages[1].gameObject.SetActive(false);
                    frameImages[2].gameObject.SetActive(false);
                    frameImages[3].gameObject.SetActive(true);
                    break;
                default:
                    frameImages[0].gameObject.SetActive(false);
                    frameImages[1].gameObject.SetActive(false);
                    frameImages[2].gameObject.SetActive(false);
                    frameImages[3].gameObject.SetActive(false);
                    break;
            }
        }
    } 
}
