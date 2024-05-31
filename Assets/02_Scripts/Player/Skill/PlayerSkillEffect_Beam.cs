using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Beam : PlayerSkillEffect
    {
        protected override void SkillEffect(PlayerStatus playerStat)
        {
            var currentCoolTime = skillData.SkillCoolTime;

            Observable.EveryUpdate()
                      .Select(_ => currentCoolTime += Time.deltaTime)
                      .Where(_ => currentCoolTime >= skillData.SkillCoolTime)
                      .Skip(TimeSpan.Zero)
                      .Subscribe(_ =>
                      {
                          var target = FindAroundMonster(skillData.AttackCount);

                          if (target == null) return;
                          if (!target.IsAlive) return;

                          var skillObj =
                              ObjectPoolManager.Instance.Spawn(skillData.SkillKey, target.transform.position);
                          var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                          projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage,
                                                        skillData.SkillCoefficient);

                          currentCoolTime = 0;
                      }).AddTo(playerTr.gameObject);
        }

        public PlayerSkillEffect_Beam(Transform playerTr, Table_PlayerActiveSkills skillData) : base(playerTr, skillData)
        {
        }
    }
}