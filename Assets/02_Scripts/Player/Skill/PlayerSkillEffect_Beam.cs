using System;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Beam : PlayerSkillEffect
    {
        protected override void SkillEffect(PlayerStat playerStat)
        {
            if (playerSkillData is not PlayerActiveSkill skillData)
            {
                CatLog.WLog("PlayerSkillEffect_Explosion : skillData is null");
                return;
            }
            
            var currentCoolTime = skillData.SkillCoolTime;

            Observable.EveryUpdate()
                      .Select(_ => currentCoolTime += Time.deltaTime)
                      .Where(_ => currentCoolTime >= skillData.SkillCoolTime)
                      .Skip(TimeSpan.Zero)
                      .Subscribe(_ =>
                      {
                          var target = FindAroundMonster(skillData.AttackCount, skillData.SkillRange);

                          if (target == null) return;
                          if (!target.IsAlive) return;

                          var skillObj =
                              ObjectPoolManager.Instance.Spawn(skillData.SkillName, target.transform.position);
                          var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                          projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage,
                                                        skillData.SkillCoefficient);

                          currentCoolTime = 0;
                      }).AddTo(playerTr.gameObject);
        }

        public PlayerSkillEffect_Beam(Transform playerTr, PlayerSkill playerSkillData) : base(playerTr, playerSkillData)
        {
        }
    }
}