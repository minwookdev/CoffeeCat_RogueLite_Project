using UnityEngine;
using UnityEngine.UI;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;

namespace CoffeeCat {
    public class InteractIcon : MonoBehaviour {
        [Title("Icon")]
        [SerializeField] private Image iconImg = null;
        
        [Title("Sprites")]
        [SerializeField] private Sprite floorSprite = null;
        [SerializeField] private Sprite bossSprite = null;
        [SerializeField] private Sprite rewardSprite = null;
        [SerializeField] private Sprite shopSprite = null;
        
        public void Enable(InteractableType type) {
            var sprite = GetIconSprite(type);
            if (!sprite) {
                return;
            }
            
            iconImg.sprite = sprite;
            gameObject.SetActive(true);
        }

        public void Disable() {
            gameObject.SetActive(false);
        }

        private Sprite GetIconSprite(InteractableType type) {
            return type switch {
                InteractableType.Floor  => floorSprite,
                InteractableType.Boss   => bossSprite,
                InteractableType.Reward => rewardSprite,
                InteractableType.Shop   => shopSprite,
                InteractableType.None   => throw new System.NotImplementedException(),
                _                       => throw new System.NotImplementedException()
            };
        }
    }
}
