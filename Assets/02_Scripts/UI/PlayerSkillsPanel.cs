using System;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat.UI
{
    public class PlayerSkillsPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform skillSetPanelTr = null;
        [SerializeField] private GameObject notSelectablePanel = null;
        [SerializeField] private Button notSlectableBtn = null;
        [SerializeField] private Button btnClose = null;
        private List<SkillSetPanel> skillSetPanels = new List<SkillSetPanel>();
        private const string skillSetAddressablekey = "SkillSet";

        public void Initialize(List<PlayerSkillSet> skillSets)
        {
            SafeLoader.Regist(skillSetAddressablekey, spawnCount: 5, onCompleted: completed =>
            {
                if (!completed)
                    CatLog.WLog($"{skillSetAddressablekey} Load Failed");
                else
                {
                    InitializeSkillSetPanel(skillSets);
                }
            });
            
            btnClose.onClick.AddListener(Close);
            notSlectableBtn.onClick.AddListener(SelectablePanelClose);
            StageManager.Inst.AddEventSkillSelectCompleted(() => gameObject.SetActive(false));
        }

        private void InitializeSkillSetPanel(List<PlayerSkillSet> skillSets)
        {
            for (var i = 0; i < skillSets.Count; i++)
            {
                var skillSetObj = ObjectPoolManager.Inst.Spawn<SkillSetPanel>(skillSetAddressablekey, skillSetPanelTr);
                skillSetObj.Initialize(skillSets[i], i);
                skillSetPanels.Add(skillSetObj);
            }
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
        
        public void OpenNotSelectablePanel()
        {
            notSelectablePanel.SetActive(true);
        }

        private void SelectablePanelClose()
        {
            notSelectablePanel.SetActive(false);
        }

        public void Open()
        {
            btnClose.gameObject.SetActive(true);
            
            foreach (var panel in skillSetPanels)
                panel.DisableButton();

            gameObject.SetActive(true);
        }

        public void OpenForSlotSelect(PlayerSkillSelectData data)
        {
            btnClose.gameObject.SetActive(false);
            
            foreach (var panel in skillSetPanels)
            {
                panel.EnableButton();
                panel.ClearBtnSlotEvent();
                panel.AddListenerBtnSlot(data);
            }
            
            gameObject.SetActive(true);
        }

        public void RefreshPlayerSkillsPanel(List<PlayerSkillSet> skillSets)
        {
            for (int i = 0; i < skillSets.Count; i++)
            {
                try
                {
                    skillSetPanels[i].Initialize(skillSets[i], i);
                }
                catch (ArgumentOutOfRangeException)
                {
                    var skillSetObj =
                        ObjectPoolManager.Inst.Spawn<SkillSetPanel>(skillSetAddressablekey, skillSetPanelTr);
                    skillSetObj.Initialize(skillSets[i], i);
                    skillSetPanels.Add(skillSetObj);
                }
            }
        }
    }
}