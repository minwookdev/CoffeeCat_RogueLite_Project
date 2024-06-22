using System;
using CoffeeCat.Utils;
using UnityEngine;
using UnityEngine.Events;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.FrameWork {
    public class InputManager : DynamicSingleton<InputManager> {
        // Fields
        private InputCanvas inputCanvas = null;
        private MobileJoyStick joyStick = null;
        private InteractButton interactButton = null;
        private InteractIcon interactIcon = null;
            
        // Events
        // Direction Input
        private readonly UnityEvent<Vector2> onDirectionInputBeginEvent = new();  // Direction Input Start
        private readonly UnityEvent<Vector2> onDirectionInputUpdateEvent = new(); // Direction Input Update
        private readonly UnityEvent onDirectionInputEndEvent = new();             // Direction Input End
        
        // Evasion Input
        private readonly UnityEvent onEvasionInputEvent = new();
        
        // Interactable Events
        private readonly UnityEvent onInteractInputEvent = new();
        
#if UNITY_EDITOR || UNITY_STANDALONE
        private bool isDirectionInputBegined = false;
        
        // Consts
        private static readonly string axisRowKey = "Horizontal";
        private static readonly string axisColKey = "Vertical";
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        private void Update() {
            UpdateDirectionInputStandalone();
            UpdateEvasionInputStandalone();
            UpdateInteractInputStandalone();
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

        private void UpdateInteractInputStandalone() {
            if (Input.GetKeyDown(KeyCode.E)) {
                InvokeInteractInput();
            }
        }
#endif

        private void Start() {
            LoadInputCanvas();
        }
        
        private void LoadInputCanvas() {
            ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(AddressablesKey.InputCanvas.ToKey(), true, (loadedGameObject) => {
                if (!loadedGameObject) {
                    CatLog.ELog("Input Canvas Load Failed");
                    return;
                }
                
                var instanced = Instantiate(loadedGameObject, Vector3.zero, Quaternion.identity);
                if (!instanced || !instanced.TryGetComponent(out inputCanvas)) {
                    CatLog.ELog("InputCanvas Setup Failed.");
                    return;
                }
                
                DontDestroyOnLoad(inputCanvas);
                var uicam = GameObject.FindGameObjectWithTag(Defines.TAG_UICAM);
                if (!uicam || !uicam.TryGetComponent(out Camera uicamera)) {
                    CatLog.ELog("UI Camera Not Found");
                    return;
                }
                
                inputCanvas.SetCanvas(uicamera);
                joyStick = inputCanvas.JoyStick;
                interactButton = inputCanvas.InteractButton;
                interactIcon = inputCanvas.InteractIcon;
                
                interactButton.Disable();
                interactIcon.Disable();
                
#if UNITY_EDITOR
                ShowJoyStick();
#elif UNITY_ANDROID || UNITY_IOS
                ShowJoyStick();
#else
                HideJoyStick();
#endif
            });
        }

        private void HideJoyStick() {
            joyStick.gameObject.SetActive(false);
        }

        private void ShowJoyStick() {
            joyStick.gameObject.SetActive(true);
        }
        
        public void EnableInteractable(InteractableType type) {
#if UNITY_EDITOR
            interactIcon.Enable(type);
            interactButton.Enable(type);
#elif UNITY_STANDALONE
            interactableIcon.Enable(type);
#elif UNITY_ANDROID || UNITY_IOS
            interactableButton.Enable(type);
#endif
        }

        public void DisableInteractable() {
#if UNITY_EDITOR
            interactIcon.Disable();
            interactButton.Disable();
#elif UNITY_STANDALONE
            interactableIcon.Disable();
#elif UNITY_ANDROID || UNITY_IOS
            interactableButton.Disable();
#endif
        }

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
        
        #region Interact Events
        
        public static void BindInteractInput(UnityAction unityAction) {
            if (!IsExist)
                return;
            inst.onInteractInputEvent.AddListener(unityAction);
        }
        
        public static void ReleaseInteractInput(UnityAction unityAction) {
            if (!IsExist)
                return;
            inst.onInteractInputEvent.RemoveListener(unityAction);
        }
        
        public static void InvokeInteractInput() {
            if (!IsExist)
                return;
            inst.onInteractInputEvent.Invoke();
        }
        
        #endregion

        #endregion
    }
}
