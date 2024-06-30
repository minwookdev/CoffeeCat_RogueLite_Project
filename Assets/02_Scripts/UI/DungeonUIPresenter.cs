using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using CoffeeCat.UI;

namespace CoffeeCat.FrameWork {
    public class DungeonUIPresenter : SceneSingleton<DungeonUIPresenter> {
        [Title("UI")]
        [SerializeField] private SkillSelectPanel skillSelector = null;
        [SerializeField] private PlayerSkillsPanel playerSkillsPanel = null;
        [SerializeField] private Minimap minimap = null;
        [SerializeField] private Image hpSliderImage = null;
        [SerializeField] private Image expSliderImage = null;
        [SerializeField] private Button btnMap = null;

        private void Start() {
            btnMap.onClick.AddListener(minimap.Open);
            
            StageManager.Inst.AddvEventIncreasePlayerHP(UpdatePlayerHPSlider);
            StageManager.Inst.AddEventDecreasePlayerHP(UpdatePlayerHPSlider);
            StageManager.Inst.AddEventIncreasePlayerExp(UpdatePlayerExpSlider);
            /*StageManager.Inst.AddEventToSkillSelectCompleted(() => {
                UpdatePlayerExpSlider(0f, 1f);
            });*/
        }

        public void OpenSkillSelectPanel(PlayerSkillSelectData[] datas) {
            skillSelector.Open(datas);
        }

        public void InitializePlayerSkillsPanel(List<PlayerSkillSet> skillSets)
        {
            playerSkillsPanel.Initialize(skillSets);
        }
        
        public void RefreshPlayerSkillsPanel(List<PlayerSkillSet> skillSets)
        {
            playerSkillsPanel.RefreshPlayerSkillsPanel(skillSets);
        }

        public void SkillsPanelOpenForSkillSelect(PlayerSkillSelectData data)
        {
            playerSkillsPanel.OpenForSlotSelect(data);
        }
        
        public void OpenNotSelectablePanel()
        {
            playerSkillsPanel.OpenNotSelectablePanel();
        }
        
        public void OpenPlayerSkillsPanel()
        {
            playerSkillsPanel.Open();
        }

        private void UpdatePlayerHPSlider(float current, float max) {
            hpSliderImage.fillAmount = current / max;
        }

        private void UpdatePlayerExpSlider(float current, float max) {
            expSliderImage.fillAmount = current / max;
        }
    }
}