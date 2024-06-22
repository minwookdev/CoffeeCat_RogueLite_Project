using System;
using UnityEngine;

namespace CoffeeCat {
    public class InputCanvas : MonoBehaviour {
        [SerializeField] private Canvas canvas = null;
        [field: SerializeField] public MobileJoyStick JoyStick { get; private set; } = null;
        [field: SerializeField] public InteractButton InteractButton { get; private set; } = null;
        [field: SerializeField] public InteractIcon InteractIcon { get; private set; } = null;
        [field: SerializeField] public GameObject fireBtnGameObject { get; private set; } = null;

        public void SetCanvas(Camera uiCamera) {
            canvas.worldCamera = uiCamera;
        }

        private void Start() {
            fireBtnGameObject.gameObject.SetActive(false);
        }
    }
}
