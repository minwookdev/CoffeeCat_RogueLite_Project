using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_NormalAttackUp : PlayerSkillEffect
    {
        protected override void SkillEffect(PlayerStatus playerStat)
        {
            // Active Attack Skill
            // Active StatEnhance Skill
            
            Observable.Interval(TimeSpan.FromSeconds(skillData.SkillCoolTime))
                      .Subscribe(_ =>
                      {
                          ObjectPoolManager.Instance.Spawn(skillData.SkillKey, playerTr.position);
                          // TODO : Active 스킬에 Delta 추가
                          // playerStat.AttackPower += skillData.Delta;
                      }).AddTo(playerTr.gameObject);
        }

        protected PlayerSkillEffect_NormalAttackUp(Transform playerTr, Table_PlayerActiveSkills skillData) :
            base(playerTr, skillData)
        {
        }
    }
}