using System;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;
using CoffeeCat.FrameWork;

namespace CoffeeCat {
    public class InteractButton : MonoBehaviour {
        [Title("Common")]
        [SerializeField] private Button btn = null;
        [SerializeField] private Image floorImg = null;
        [SerializeField] private Image bossImg = null;
        [SerializeField] private Image rewardImg = null;
        [SerializeField] private Image shopImg = null;
        [SerializeField] private Image homeImg = null;

        private void Start() {
            btn.onClick.AddListener(InteractableButtonEvent);
        }

        public void Enable(InteractableType type) {
            floorImg.gameObject.SetActive(false);
            bossImg.gameObject.SetActive(false);
            rewardImg.gameObject.SetActive(false);
            shopImg.gameObject.SetActive(false);

            var targetImage = GetIconImage(type);
            if (!targetImage) {
                return;
            }
            
            targetImage.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Disable() {
            gameObject.SetActive(false);
        }
        
        private Image GetIconImage(InteractableType type) {
            return type switch {
                InteractableType.Floor  => floorImg,
                InteractableType.Boss   => bossImg,
                InteractableType.Reward => rewardImg,
                InteractableType.Shop   => shopImg,
                InteractableType.Town   => homeImg,
                InteractableType.None   => throw new NotImplementedException(),
                _                       => throw new NotImplementedException()
            };
        }

        private void InteractableButtonEvent() {
            InputManager.InvokeInteractInput();
        }
    }
}
