using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

namespace CoffeeCat
{
    [SuppressMessage("ReSharper", "HeapView.ClosureAllocation")]
    [SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
    public class PlayerSkillEffect_Explosion : PlayerSkillEffect
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
                              var targets = FindAroundMonsters(skillData.AttackCount, skillData.SkillRange);
                              if (targets == null) return;

                              foreach (var target in targets)
                              {
                                  if (!target.IsAlive) continue;

                                  var skillObj =
                                      ObjectPoolManager.Instance.Spawn(skillData.SkillName, target.transform);
                                  var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                                  projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage,
                                                                skillData.SkillCoefficient);
                              }
                              currentCoolTime = 0;
                          });
        }

        public PlayerSkillEffect_Explosion(Transform playerTr, PlayerSkill playerSkillKey) :
            base(playerTr, playerSkillKey)
        {
        }
    }
}