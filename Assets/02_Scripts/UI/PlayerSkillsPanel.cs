using System;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UnityEngine;

namespace CoffeeCat.UI
{
    public class PlayerSkillsPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform skillSetPanelTr = null;
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
            
            StageManager.Inst.AddEventToSkillSelectCompleted(() => gameObject.SetActive(false));
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

        public void Open()
        {
            foreach (var panel in skillSetPanels)
                panel.DisableButton();

            gameObject.SetActive(true);
        }

        public void OpenForSlotSelect(PlayerSkillSelectData data)
        {
            gameObject.SetActive(true);
            
            foreach (var panel in skillSetPanels)
            {
                panel.EnableButton();
                panel.AddListenerBtnSlot(data);
            }
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