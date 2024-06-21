using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillSet
    {
        [ShowInInspector, ReadOnly] private int skillSetIndex = 0;

        [Title("MainSkill")]
        [ShowInInspector, ReadOnly] private PlayerMainSkill mainSkillData = null;
        private readonly PlayerSkillEffect mainSkillEffect = null;

        [Title("SubSkill")]
        [ShowInInspector, ReadOnly] private PlayerSubAttackSkill subAttackSkill = null;
        [ShowInInspector, ReadOnly] private PlayerSubStatSkill subStatSkill_1 = null;
        [ShowInInspector, ReadOnly] private PlayerSubStatSkill subStatSkill_2 = DataManager.Inst.PlayerSubStatSkills.DataDictionary[0];

        public PlayerSkillSet(PlayerMainSkill mainSkill, int index)
        {
            skillSetIndex = index;
            mainSkillData = mainSkill;
            mainSkillEffect = SkillEffectManager.Inst.InstantiateMainSkillEffect(mainSkill);
        }

        public void MainSkillEffect(PlayerStat stat)
        {
            mainSkillEffect.OnDispose();
            mainSkillEffect.SkillEffect(stat, mainSkillData);
        }

        public void UpdateMainSkill(PlayerMainSkill skill)
        {
            mainSkillData = skill;
        }

        public void UpdateSubAttackSkill(PlayerSubAttackSkill skill)
        {
            subAttackSkill = skill;
        }

        public void UpdateSubStatSkill(PlayerSubStatSkill skill, int slotNum)
        {
            switch (slotNum)
            {
                case 1:
                    subStatSkill_1 = skill;
                    SkillEffectManager.Inst.UpdateSubStatSkillEffect(mainSkillData, skill);
                    break;
                case 2:
                    subStatSkill_2 = skill;
                    SkillEffectManager.Inst.UpdateSubStatSkillEffect(mainSkillData, skill);
                    break;
            }
        }
        
        public string GetMainSkill()
        {
            return mainSkillData.SkillName;
        }

        public string GetSubAttackSkill()
        {
            return subAttackSkill.SkillName;
        }
        
        public string GetSubStatSkill_1()
        {
            return subStatSkill_1.SkillName;
        }
        
        public bool IsEmptySubAttackSkill()
        {
            return subAttackSkill == null;
        }

        public bool IsMaxLevelMainSkill()
        {
            return mainSkillData.SkillLevel == 3;
        }
        
        public bool IsMaxLevelSubAttackSkill()
        {
            return subAttackSkill.SkillLevel == 3;
        }
        
        public bool IsEmptySubStatSkill()
        {
            return subStatSkill_1 == null;
        }
        
        public bool IsMaxLevelSubStatSkill()
        {
            return subStatSkill_1.SkillLevel == 3;
        }

        public void UnlockSubStatSkill()
        {
            subStatSkill_2 = null;
        }
    }
}