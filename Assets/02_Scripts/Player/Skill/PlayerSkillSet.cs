using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillSet
    {
        [ShowInInspector, ReadOnly] public int skillSetIndex { get; private set; } = 0;

        [Title("MainSkill")]
        [ShowInInspector, ReadOnly] private PlayerMainSkill mainSkill = null;   // Main Skill Data
        private PlayerSkillEffect mainSkillEffect = null;                       // Main Skill Effect
        
        [Title("SubSkill")]
        [ShowInInspector, ReadOnly] private PlayerSubAttackSkill subAttackSkill = null;
        [ShowInInspector, ReadOnly] private PlayerSubStatSkill subStatSkill_2 = null;
        [ShowInInspector, ReadOnly] private PlayerSubStatSkill subStatSkill_1 = null;

        public PlayerSkillSet(PlayerMainSkill skill, int index)
        {
            mainSkill = skill;
            skillSetIndex = index;
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