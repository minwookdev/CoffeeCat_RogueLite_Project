using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkill_Beam : PlayerSkillEffect
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

                            var target = FindAroundMonster(skillData.AttackCount);

                            if (!target.IsAlive) return;

                            var skillObj = ObjectPoolManager.Instance.Spawn(skillData.SkillKey, target.transform.position);
                            var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                            projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage, skillData.SkillCoefficient);

                            currentCoolTime = 0;
                      }).AddTo(playerTr.gameObject);
        }

        public PlayerSkill_Beam(Transform playerTr, Table_PlayerSkills skillData) : base(playerTr, skillData)
        {
        }
    }
}