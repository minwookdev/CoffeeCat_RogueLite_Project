using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using UnityEngine;
using UniRx;

namespace CoffeeCat
{
    public class PlayerSkill_Explosion : PlayerSkillEffect
    {
        protected override void SkillEffect(PlayerStatus playerStat)
        {
            float currentCoolTime;
            currentCoolTime = skillData.SkillCoolTime;

            Observable.EveryUpdate()
                      .Skip(TimeSpan.Zero)
                      .Subscribe(_ =>
                      {
                          currentCoolTime += Time.deltaTime;

                          if (currentCoolTime < skillData.SkillCoolTime) return;
                          
                          var targets = FindAroundMonsters(skillData.AttackCount);
                          
                          if (targets == null) return;
                          
                          foreach (var target in targets)
                          {
                              if (!target.IsAlive) continue;
                              
                              var skillObj = ObjectPoolManager.Instance.Spawn(skillData.SkillKey, target.transform.position);
                              var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                              projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage, skillData.SkillCoefficient);
                          }

                          currentCoolTime = 0;
                      }).AddTo(playerTr.gameObject);
            
            // TODO : 플레이어 비활성화시 구독 해제할 것인지, 파괴시 구독 해제할 것인지
        }

        public PlayerSkill_Explosion(Transform playerTr, Table_PlayerSkills skillKey) : base(playerTr, skillKey)
        {
        }
    }
}