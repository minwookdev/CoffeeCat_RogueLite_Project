using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Bubble : PlayerSkillEffect
    {
        // TODO : 피해범위 보여주기
        // TODO : Battle Room 클리어 여부
        protected override void SkillEffect(PlayerStatus playerStat)
        {
            var currentCoolTime = skillData.SkillCoolTime;
            
            Observable.EveryUpdate()
                      .Select(_ => currentCoolTime += Time.deltaTime)
                      .Where(_ => currentCoolTime >= skillData.SkillCoolTime)
                      .Skip(TimeSpan.Zero)
                      .Subscribe(_ =>
                      {
                          var targets = FindAllMonsters();
                          
                          if (targets == null) return;

                          var skillObj = ObjectPoolManager.Instance.Spawn(skillData.SkillKey, playerTr.position);
                          var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                          projectile.AreaAttack(playerStat, targets, skillData.SkillBaseDamage, skillData.SkillCoefficient);
                          
                          currentCoolTime = 0;
                      }).AddTo(playerTr.gameObject);
        }

        public PlayerSkillEffect_Bubble(Transform playerTr, Table_PlayerActiveSkills skillData) : base(playerTr, skillData)
        {
        }
    }
}