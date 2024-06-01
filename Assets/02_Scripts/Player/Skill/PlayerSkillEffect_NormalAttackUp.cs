using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_NormalAttackUp : PlayerSkillEffect
    {
        protected override void SkillEffect(PlayerStat playerStat)
        {
            var skillData = playerSkillData as PlayerActiveSkill;
            if (skillData == null)
            {
                CatLog.WLog("PlayerSkillEffect_Explosion : skillData is null");
                return;
            }
            
            Observable.Interval(TimeSpan.FromSeconds(skillData.SkillCoolTime))
                      .Subscribe(_ =>
                      {
                          ObjectPoolManager.Instance.Spawn(skillData.SkillName, playerTr.position);
                          // TODO : Active 스킬에 Delta 추가
                          // playerStat.AttackPower += skillData.Delta;
                      }).AddTo(playerTr.gameObject);
        }

        protected PlayerSkillEffect_NormalAttackUp(Transform playerTr, PlayerSkill playerSkillData) :
            base(playerTr, playerSkillData)
        {
        }
    }
}