using System;
using System.Diagnostics.CodeAnalysis;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    [SuppressMessage("ReSharper", "HeapView.ClosureAllocation")]
    [SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
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
            
            updateDisposable =
                Observable.EveryUpdate()
                          .Select(_ => currentCoolTime += Time.deltaTime)
                          .Where(_ => currentCoolTime >= skillData.SkillCoolTime)
                          .Subscribe(_ =>
                          {
                              var target = FindAroundMonster(skillData.AttackCount, skillData.SkillRange);

                              if (target == null) return;
                              if (!target.IsAlive) return;

                              var skillObj =
                                  ObjectPoolManager.Instance.Spawn(skillData.SkillName, target.GetCenterTr().position);
                              var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                              projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage,
                                                            skillData.SkillCoefficient);

                              currentCoolTime = 0;
                          });
        }

        public PlayerSkillEffect_Beam(Transform playerTr, PlayerSkill playerSkillData) : base(playerTr, playerSkillData)
        {
        }
    }
}