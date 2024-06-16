using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.UI;
using UnityEngine.UI;

namespace CoffeeCat.FrameWork {
    public class UIPresenter : SceneScopedSingleton<UIPresenter> {
        [Title("UI")]
        [SerializeField] private SkillSelectPanel skillSelector = null;
        [SerializeField] private Minimap minimap = null;
        [SerializeField] private Image hpSliderImage = null;
        [SerializeField] private Button btnNextFloor = null;
        [SerializeField] private Button btnMap = null;

        private void Start() {
            btnNextFloor.onClick.AddListener(() => {
                StageManager.Instance.RequestGenerateNextFloor();
                DisableNextFloorButton();
            });
            
            btnMap.onClick.AddListener(minimap.Open);
        }

        public void OpenSkillSelectPanel(PlayerSkillSelectData[] datas) {
            skillSelector.Open(datas);
        }

        public void UpdatePlayerHPSlider(float current, float max) {
            hpSliderImage.fillAmount = current / max;
        }

        public void EnableNextFloorButton() {
            btnNextFloor.gameObject.SetActive(true);
        }

        public void DisableNextFloorButton() {
            btnNextFloor.gameObject.SetActive(false);
        }
    }
}
