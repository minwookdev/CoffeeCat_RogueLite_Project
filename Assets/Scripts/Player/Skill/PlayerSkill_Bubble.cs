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
        protected override void SkillEffect(Transform playerTr, PlayerStatus playerStat)
        {
            Observable.Interval(TimeSpan.FromSeconds(skillData.SkillCoolTime))
                      .Subscribe(_ =>
                      {
                          var skillObj = ObjectPoolManager.Instance.Spawn(skillData.SkillKey, playerTr.position);
                          skillObj.TryGetComponent(out PlayerSkillProjectile projectile);
                          projectile.SetDamageData(playerStat, skillData.SkillBaseDamage,
                                                   skillData.SkillCoefficient);
                      }).AddTo(playerTr.gameObject);
        }

        public PlayerSkill_Bubble(Table_PlayerSkills skillData) : base(skillData)
        {
        }
    }
}