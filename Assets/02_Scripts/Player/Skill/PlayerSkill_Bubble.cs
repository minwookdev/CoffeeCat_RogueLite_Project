using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkill_Bubble : PlayerSkillEffect
    {
        // TODO : 몬스터 스턴이 필요해
        // TODO : Battle Room 클리어 여부
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

                          var targets = FindAllMonsters();
                          
                          if (targets == null) return;

                          var skillObj = ObjectPoolManager.Instance.Spawn(skillData.SkillKey, playerTr.position);
                          var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                          projectile.AreaAttack(playerStat, targets, skillData.SkillBaseDamage, skillData.SkillCoefficient);
                          
                          currentCoolTime = 0;
                      }).AddTo(playerTr.gameObject);
        }

        public PlayerSkill_Bubble(Transform playerTr, Table_PlayerSkills skillData) : base(playerTr, skillData)
        {
        }
    }
}