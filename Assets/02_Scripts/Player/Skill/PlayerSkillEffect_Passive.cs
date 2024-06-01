using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Passive : PlayerSkillEffect
    {
        // TODO : 시트에 enum변수 추가
        private enum PassiveSkillType
        {
            NONE,
            MAX_HP,
            MOVE_SPEED,
            ATTACK_POWER,
            ATTACK_DELAY,
            PROJECTILE_SPEED,
            DEFENSE,
            CRITICAL_CHANCE,
            CRITICAL_DAMAGE,
            COOL_TIME_REDUCE,
        }

        private PassiveSkillType passiveSkillType = PassiveSkillType.NONE;
        
        protected override void SkillEffect(PlayerStatus playerStat)
        {
            switch (passiveSkillType)
            {
                case PassiveSkillType.NONE:
                    break;
                case PassiveSkillType.MAX_HP:
                    break;
                case PassiveSkillType.MOVE_SPEED:
                    // playerStat.MoveSpeed += skillData.Delta;
                    break;
                case PassiveSkillType.ATTACK_POWER:
                    // playerStat.AttackPower += skillData.Delta;
                    break;
                case PassiveSkillType.ATTACK_DELAY:
                    break;
                case PassiveSkillType.PROJECTILE_SPEED:
                    break;
                case PassiveSkillType.DEFENSE:
                    break;
                case PassiveSkillType.CRITICAL_CHANCE:
                    break;
                case PassiveSkillType.CRITICAL_DAMAGE:
                    break;
                case PassiveSkillType.COOL_TIME_REDUCE:
                    // playerStat.CoolTimeReduce += skillData.Delta;
                    break;
                default:
                    break;
            }
        }

        protected PlayerSkillEffect_Passive(Transform playerTr, Table_PlayerActiveSkills skillData) : base(playerTr, skillData)
        {
            // passiveSkillType = skillData.PassiveSkillType;
        }
    }
}