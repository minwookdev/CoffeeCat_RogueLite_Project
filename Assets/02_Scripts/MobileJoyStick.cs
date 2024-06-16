using System;
using CoffeeCat.FrameWork;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace CoffeeCat {
    public class MobileJoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
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

        private void Start() {
            // Set Stick Start Position
            stickInitPos = stickRectTr.anchoredPosition;
            
            // Only Enable in Mobile
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return;
#elif UNITY_STANDALONE
            gameObject.SetActive(false);
#endif
        }

        private void OnDisable() {
            Clear();
        }

        public void OnDrag(PointerEventData eventData) {
            if (!isTouched) {
                return;
            }
            
            UpdateStickPosition(eventData.position);
            UpdateDirectionFromStickPosition();
            UpdateFrameDirection();
            InputManager.InvokeDirectionInputUpdateEvent(stickDirection);
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (isTouched) {
                return;
            }
            
            isTouched = true;
            UpdateStickPosition(eventData.position);
            UpdateDirectionFromStickPosition();
            UpdateFrameDirection();
            InputManager.InvokeDirectionInputBeginEvent(stickDirection);
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (!isTouched) {
                return;
            }
            
            Clear();
            InputManager.InvokeDirectionInputEndEvent();
        }
        
        private void UpdateStickPosition(Vector2 screenTouchPos) {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTr, screenTouchPos, canvas.worldCamera, out Vector2 localTouchPos)) {
                localTouchPos = Vector2.zero;
            }
            var distance = localTouchPos - stickInitPos;
            var clampedStickPos =  ClampJoystickPosition(distance);
            stickRectTr.anchoredPosition = clampedStickPos;
        }
        
        private Vector2 ClampJoystickPosition(Vector2 distance) {
            var distanceSqr = distance.sqrMagnitude;
            if (distanceSqr > stickMoveRangeSqrMagnitude) {
                var direction = distance.normalized;
                distance = direction * Mathf.Sqrt(stickMoveRangeSqrMagnitude);
            }
            return distance;
        }

        private void UpdateDirectionFromStickPosition() {
            stickDirection = stickRectTr.anchoredPosition - stickInitPos;
            stickDirection.Normalize();
        }

        private void UpdateFrameDirection() {
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
        
        private void Clear() {
            isTouched = false;
            stickDirection = Vector2.zero;
            stickRectTr.anchoredPosition = Vector2.zero;
            for (int i = 0; i < frameImages.Length; i++) {
                frameImages[i].gameObject.SetActive(false);
            }
        }
    } 
}
