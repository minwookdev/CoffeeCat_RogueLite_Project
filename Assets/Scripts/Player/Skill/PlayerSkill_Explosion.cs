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
        protected override void SkillEffect(Transform playerTr, PlayerStatus playerStat)
        {
            Observable.Interval(TimeSpan.FromSeconds(skillData.SkillCoolTime))
                      .Subscribe(_ =>
                      {
                          // TODO : 몬스터가 없어서 null일 경우 쿨타임은?
                          // TODO : 몬스터가 죽으면 return

                          var targets = FindAroundMonsters(playerTr, skillData.AttackCount);

                          if (targets == null)
                              return;

                          foreach (var target in targets)
                          {
                              var skillObj = ObjectPoolManager.Instance.Spawn(skillData.SkillKey, target.position);
                              skillObj.TryGetComponent(out PlayerSkillProjectile projectile);
                              projectile.SetDamageData(playerStat, skillData.SkillBaseDamage,
                                                       skillData.SkillCoefficient);
                          }
                      }).AddTo(playerTr.gameObject);
        }

        public PlayerSkill_Explosion(Table_PlayerSkills skillKey) : base(skillKey)
        {
        }
    }
}