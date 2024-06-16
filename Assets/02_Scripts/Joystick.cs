using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace CoffeeCat {
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        [Title("JoyStick")]
        [SerializeField] private Canvas canvas = null;
        [SerializeField] private RectTransform rectTr = null;
        [SerializeField] private RectTransform stickRectTr = null;
        [SerializeField] private float stickMoveRangeSqrMagnitude = 4500f;
        [SerializeField, ReadOnly] private bool isTouched = false;
        
        [Title("Frame Images")]
        [SerializeField] private Image[] frameImages = null;
        private Vector2 stickDirection = Vector2.zero;
        private Vector2 stickInitPos = Vector2.zero;
        private Player player = null;

        private void Start() {
            stickInitPos = stickRectTr.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData) {
            if (!isTouched)
                return;
            stickRectTr.anchoredPosition = GetJoystickPosition(eventData.position);
            UpdateFrames();
        }

        public void OnPointerDown(PointerEventData eventData) {
            isTouched = true;
            stickRectTr.anchoredPosition = GetJoystickPosition(eventData.position);
            UpdateFrames();
        }

        public void OnPointerUp(PointerEventData eventData) {
            isTouched = false;
            stickRectTr.anchoredPosition = Vector2.zero;
            ClearFrames();
        }
        
        private Vector2 GetJoystickPosition(Vector2 screenTouchPos) {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTr, screenTouchPos, canvas.worldCamera, out Vector2 localTouchPos)) {
                return Vector2.zero;
            }
            var distance = localTouchPos - stickInitPos;
            return ClampJoystickPosition(distance);
        }
        
        private Vector2 ClampJoystickPosition(Vector2 distance) {
            var distanceSqr = distance.sqrMagnitude;
            if (distanceSqr > stickMoveRangeSqrMagnitude) {
                var direction = distance.normalized;
                distance = direction * Mathf.Sqrt(stickMoveRangeSqrMagnitude);
            }
            return distance;
        }

        private void UpdateFrames() {
            stickDirection = stickRectTr.anchoredPosition - stickInitPos;
            stickDirection.Normalize();
            int index = GetQuadrantIndex(stickDirection);
            for (int i = 0; i < frameImages.Length; i++) {
                frameImages[i].gameObject.SetActive(i == index);
            }
        }

        private int GetQuadrantIndex(Vector2 direction) {
            if (direction.x < 0) {
                return direction.y > 0 ? 0 : 2;
            }

            return direction.y > 0 ? 1 : 3;
        }

        private void ClearFrames() {
            stickDirection = Vector2.zero;
            for (int i = 0; i < frameImages.Length; i++) {
                frameImages[i].gameObject.SetActive(false);
            }
        }
    } 
}
