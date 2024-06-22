using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat.UI
{
    public class SkillSetPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI slotIndex = null;
        [SerializeField] private Image mainSkillIcon = null;
        [SerializeField] private TextMeshProUGUI mainSkillLevel = null;
        [SerializeField] private TextMeshProUGUI mainSkillName = null;
        [SerializeField] private Image subAttackIcon = null;
        [SerializeField] private Image subStatIcon_1 = null;
        [SerializeField] private Image subStatIcon_2 = null;
        [SerializeField] private TextMeshProUGUI description = null;
        [SerializeField] private Button btnSlot = null;
        private int index = 0;

        public void Initialize(PlayerSkillSet skillSet, int index)
        {
            CatLog.Log("SkillSetPanel Initialize");

            this.index = index;
            transform.localScale = Vector3.one;
            slotIndex.text = $"Skill Set {(skillSet.SkillSetIndex + 1).ToString()}";
            // mainSkillIcon.sprite = skillSet.GetMainSkill();
            mainSkillLevel.text = skillSet.MainSkillData.SkillLevel.ToString();
            mainSkillName.text = skillSet.MainSkillData.SkillName;
            // subAttackIcon.sprite = skillSet.GetSubAttackSkill();
            // subStatIcon_1.sprite = skillSet.GetSubStatSkill_1();
            // subStatIcon_2.sprite = skillSet.GetSubStatSkill_2();
            description.text = skillSet.MainSkillData.Description;

            // 임시
            if (!skillSet.IsEmptySubAttackSkill())
                subAttackIcon.gameObject.SetActive(true);

            if (!skillSet.IsEmptySubStatSkill())
                subStatIcon_1.gameObject.SetActive(true);
        }

        public void AddListenerBtnSlot(PlayerSkillSelectData data)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;

            btnSlot.onClick.AddListener(() =>
            {
                if (player.CheckSelectableSlot(index, data))
                {
                    player.UpdateSubSkill(index, data);
                }
                else
                {
                    CatLog.WLog("Slot is not selectable");
                }
            });
        }

        public void EnableButton()
        {
            btnSlot.enabled = true;
        }

        public void DisableButton()
        {
            btnSlot.enabled = false;
        }
    }
}