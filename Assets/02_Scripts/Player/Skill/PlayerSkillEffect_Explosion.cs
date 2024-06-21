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
    public class PlayerSkillEffect_Explosion : PlayerSkillEffect
    {
        public override void SkillEffect(PlayerStat playerStat, PlayerMainSkill skillData)
        {
            var currentCoolTime = skillData.SkillCoolTime;
            
            updateDisposable =
                Observable.EveryUpdate()
                          .Select(_ => currentCoolTime += Time.deltaTime)
                          .Where(_ => currentCoolTime >= skillData.SkillCoolTime)
                          .Where(_ => completedLoadResource)
                          .Subscribe(_ =>
                          {
                              var targets = FindAroundMonsters(skillData.AttackCount, skillData.SkillRange);
                              if (targets == null) return;

                              foreach (var target in targets)
                              {
                                  if (!target.IsAlive) continue;

                                  var skillObj =
                                      ObjectPoolManager.Inst.Spawn(skillData.SkillName, target.GetCenterTr().position);
                                  var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                                  projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage,
                                                                skillData.SkillCoefficient);
                              }
                              currentCoolTime = 0;
                          });
        }
        
        public PlayerSkillEffect_Explosion(Transform playerTr, string skillName) : base(playerTr, skillName) { }
    }
}