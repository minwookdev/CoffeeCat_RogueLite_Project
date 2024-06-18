using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using CoffeeCat.UI;

namespace CoffeeCat.FrameWork {
    public class UIPresenter : SceneSingleton<UIPresenter> {
        [Title("UI")]
        [SerializeField] private SkillSelectPanel skillSelector = null;
        [SerializeField] private Minimap minimap = null;
        [SerializeField] private Image hpSliderImage = null;
        [SerializeField] private Image expSliderImage = null;
        [SerializeField] private Button btnNextFloor = null;
        [SerializeField] private Button btnMap = null;

        [Title("Mobile Input")]
        [SerializeField] private MobileJoyStick joyStick = null;

        private void Start() {
            btnNextFloor.onClick.AddListener(() => {
                StageManager.Inst.RequestGenerateNextFloor();
                DisableNextFloorButton();
            });
            
            btnMap.onClick.AddListener(minimap.Open);
            
            StageManager.Inst.AddListenerIncreasePlayerHP(UpdatePlayerHPSlider);
            StageManager.Inst.AddListenerDecreasePlayerHP(UpdatePlayerHPSlider);
            StageManager.Inst.AddListenerIncreasePlayerExp(UpdatePlayerExpSlider);
            /*StageManager.Inst.AddEventToSkillSelectCompleted(() => {
                UpdatePlayerExpSlider(0f, 1f);
            });*/
        }

        public void OpenSkillSelectPanel(PlayerSkillSelectData[] datas) {
            skillSelector.Open(datas);
        }

        private void UpdatePlayerHPSlider(float current, float max) {
            hpSliderImage.fillAmount = current / max;
        }

        private void UpdatePlayerExpSlider(float current, float max) {
            expSliderImage.fillAmount = current / max;
        }

        public void EnableNextFloorButton() {
            btnNextFloor.gameObject.SetActive(true);
        }

        public void DisableNextFloorButton() {
            btnNextFloor.gameObject.SetActive(false);
        }

        public void HideJoyStick() => joyStick.gameObject.SetActive(false);

        public void ShowJoyStick() => joyStick.gameObject.SetActive(true);
    }
}
