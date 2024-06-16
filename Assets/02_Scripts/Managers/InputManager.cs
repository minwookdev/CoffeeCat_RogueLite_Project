using System;
using UnityEngine;
using UnityEngine.Events;

namespace CoffeeCat.FrameWork {
    public class InputManager : DynamicSingleton<InputManager> {
        // Fields
        private bool isDirectionInputBegined = false;

        // Events
        // Direction Input
        private readonly UnityEvent<Vector2> onDirectionInputBeginEvent = new();  // Direction Input Start
        private readonly UnityEvent<Vector2> onDirectionInputUpdateEvent = new(); // Direction Input Update
        private readonly UnityEvent onDirectionInputEndEvent = new();             // Direction Input End
        
        // Evasion Input
        private readonly UnityEvent onEvasionInputEvent = new();
        
#if UNITY_EDITOR || UNITY_STANDALONE
        // Consts
        private static readonly string axisRowKey = "Horizontal";
        private static readonly string axisColKey = "Vertical";
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        private void Update() {
            UpdateDirectionInputStandalone();
            UpdateEvasionInputStandalone();
        }
        
        private void UpdateDirectionInputStandalone() {
            var hor = Input.GetAxisRaw(axisRowKey);
            var ver = Input.GetAxisRaw(axisColKey);
            if (hor == 0 && ver == 0) {
                if (!isDirectionInputBegined) {
                    return;
                }
                isDirectionInputBegined = false;
                InvokeDirectionInputEndEvent();
                return;
            }
            
            var inputDirection = new Vector2(hor, ver);
            inputDirection.Normalize();
            if (!isDirectionInputBegined) {
                isDirectionInputBegined = true;
                InvokeDirectionInputBeginEvent(inputDirection);
                
                // If you want to distinguish between the frame where input starts and the frame where it is updated, uncomment it.
                // return;
            }
            
            InvokeDirectionInputUpdateEvent(inputDirection);
        }
        
        private void UpdateEvasionInputStandalone() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                InvokeEvasionInput();
            }
        }
#endif
        
        #region Events
        
        #region Direction Input
        
        public static void BindDirectionInputUpdateEvent(UnityAction<Vector2> unityAction) {
            if (!IsExist)
                return;
            inst.onDirectionInputUpdateEvent.AddListener(unityAction);
        }

        public static void ReleaseDirectionInputUpdateEvent(UnityAction<Vector2> unityAction) {
            if (!IsExist)
                return;
            inst.onDirectionInputUpdateEvent.RemoveListener(unityAction);
        }

        public static void InvokeDirectionInputUpdateEvent(Vector2 direction) {
            if (!IsExist)
                return;
            inst.onDirectionInputUpdateEvent.Invoke(direction);
        }

        public static void BindDirectionInputBeginEvent(UnityAction<Vector2> unityAction) {
            if (!IsExist)
                return;
            inst.onDirectionInputBeginEvent.AddListener(unityAction);
        }

        public static void ReleaseDirectionInputBeginEvent(UnityAction<Vector2> unityAction) {
            if (!IsExist)
                return;
            inst.onDirectionInputBeginEvent.RemoveListener(unityAction);
        }

        public static void InvokeDirectionInputBeginEvent(Vector2 direction) {
            if (!IsExist)
                return;
            inst.onDirectionInputBeginEvent.Invoke(direction);
        }

        public static void BindDirectionInputEndEvent(UnityAction unityAction) {
            if (!IsExist)
                return;
            inst.onDirectionInputEndEvent.AddListener(unityAction);
        }

        public static void ReleaseDirectionInputEndEvent(UnityAction unityAction) {
            if (!IsExist)
                return;
            inst.onDirectionInputEndEvent.RemoveListener(unityAction);
        }

        public static void InvokeDirectionInputEndEvent() {
            if (!IsExist)
                return;
            inst.onDirectionInputEndEvent.Invoke();
        }

        #endregion
        
        #region Evasion Input

        public static void BindEvasionInput(UnityAction unityAction) {
            if (!IsExist)
                return;
            inst.onEvasionInputEvent.AddListener(unityAction);
        }

        public static void ReleaseEvasionInput(UnityAction unityAction) {
            if (!IsExist)
                return;
            inst.onEvasionInputEvent.RemoveListener(unityAction);
        }

        public static void InvokeEvasionInput() {
            if (!IsExist)
                return;
            inst.onEvasionInputEvent.Invoke();
        }

        #endregion

        #endregion
    }
}
