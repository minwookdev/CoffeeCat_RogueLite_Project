using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkill_Beam : PlayerSkillEffect
    {
        protected override void SkillEffect(Transform playerTr, PlayerStatus playerStat)
        {
            Observable.Interval(TimeSpan.FromSeconds(skillData.SkillCoolTime))
                      .Subscribe(_ =>
                      {
                          Effect();
                      });
            return;

            void Effect()
            {
                var target = FindAroundMonster(playerTr, skillData.AttackCount);

                if (target == null)
                    return;

                var skillObj = ObjectPoolManager.Instance.Spawn(skillData.SkillKey, target.position);
                skillObj.TryGetComponent(out PlayerSkillProjectile projectile);
                projectile.SetDamageData(playerStat, skillData.SkillBaseDamage,
                                         skillData.SkillCoefficient);
            }
        }

        public PlayerSkill_Beam(Table_PlayerSkills skillData) : base(skillData)
        {
        }
    }
}