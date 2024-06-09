using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.UI;
using UnityEngine.UI;

namespace CoffeeCat.FrameWork {
    public class UIPresenter : PrelocatedSingleton<UIPresenter> {
        [Title("UI")]
        [SerializeField] private SkillSelectPanel skillSelector = null;
        [SerializeField] private Image hpSliderImage = null;
        [SerializeField] private Button nextFloorButton = null;

        private void Start() {
            nextFloorButton.onClick.AddListener(() => {
                StageManager.Instance.RequestGenerateNextFloor();
                DisableNextFloorButton();
            });
        }

        public void OpenSkillSelectPanel(PlayerSkillSelectData[] datas) {
            skillSelector.Open(datas);
        }

        public void UpdatePlayerHPSlider(float current, float max) {
            hpSliderImage.fillAmount = current / max;
        }

        public void EnableNextFloorButton() {
            nextFloorButton.gameObject.SetActive(true);
        }

        public void DisableNextFloorButton() {
            nextFloorButton.gameObject.SetActive(false);
        }
    }
}
