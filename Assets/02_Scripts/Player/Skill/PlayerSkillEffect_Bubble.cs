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
        // TODO : 피해범위 보여주기
        // TODO : Battle Room 클리어 여부
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
                          var targets = FindAllMonsters();
                          
                          if (targets == null) return;

                          var skillObj = ObjectPoolManager.Instance.Spawn(skillData.SkillName, playerTr.position);
                          var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                          projectile.AreaAttack(playerStat, targets, skillData.SkillBaseDamage, skillData.SkillCoefficient);
                          
                          currentCoolTime = 0;
                      }).AddTo(playerTr.gameObject);
        }

        public PlayerSkillEffect_Bubble(Transform playerTr, PlayerSkill playerSkillData) : base(playerTr, playerSkillData)
        {
        }
    }
}