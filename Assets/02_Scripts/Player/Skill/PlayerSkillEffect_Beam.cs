using System;
using System.Diagnostics.CodeAnalysis;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Beam : PlayerSkillEffect
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
                              var target = FindAroundMonster(skillData.AttackCount, skillData.SkillRange);

                              if (target == null) return;
                              if (!target.IsAlive) return;

                              var skillObj =
                                  ObjectPoolManager.Inst.Spawn(skillData.SkillName, target.GetCenterTr().position);
                              var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                              projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage,
                                                            skillData.SkillCoefficient);

                              currentCoolTime = 0;
                          });
        }
        
        public PlayerSkillEffect_Beam(Transform playerTr, string skillName) : base(playerTr, skillName) { }
    }
}