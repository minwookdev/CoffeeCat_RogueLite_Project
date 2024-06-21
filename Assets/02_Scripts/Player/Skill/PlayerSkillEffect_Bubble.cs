using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Bubble : PlayerSkillEffect
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
                              var targets = FindAllMonsters();
                              if (targets == null) return;

                              DisplayDamageRange();
                              var skillObj = ObjectPoolManager.Inst.Spawn(skillData.SkillName, playerTr.position);
                              var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                              projectile.AreaAttack(playerStat, targets, skillData.SkillBaseDamage,
                                                    skillData.SkillCoefficient);

                              currentCoolTime = 0;
                          });
        }

        public PlayerSkillEffect_Bubble(Transform playerTr, string skillName) :
            base(playerTr, skillName)
        {
        }
    }
}