using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillSet
    {
        private int skillSlotIndex = 0;
        private PlayerMainSkill mainSkill = null;           // Main Skill Data
        private PlayerSkillEffect mainSkillEffect = null;   // Main Skill Effect
        private PlayerSubAttackSkill subAttackSkill = null;
        private PlayerSubStatSkill subStatSkill_1 = null;
        private PlayerSubStatSkill subStatSkill_2 = null;

        public PlayerSkillSet(PlayerMainSkill skill, int index)
        {
            mainSkill = skill;
            skillSlotIndex = index;
            subStatSkill_2 = DataManager.Inst.PlayerSubStatSkills.DataDictionary[0]; // 0 : skill Slot locked
        }

        public void SetSubAttackSkill(PlayerSubAttackSkill skill)
        {
            subAttackSkill = skill;
        }
        
        public void SetSubStatSkill(PlayerSubStatSkill skill, int slotNum)
        {
            switch (slotNum)
            {
                case 1:
                    subStatSkill_1 = skill;
                    SkillEffectManager.Inst.UpdateSubStatSkillEffect(mainSkill, skill);
                    break;
                case 2:
                    subStatSkill_2 = skill;
                    break;
            }
        }
        
        public void UnlockSubStatSkill()
        {
            subStatSkill_2 = null;
        }

        public bool IsEmptySubAttackSkill()
        {
            return subAttackSkill == null;
        }
    }
}