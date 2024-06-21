using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_NormalAttack : PlayerSkillEffect
    {
        private readonly Player player = null;

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
                              var targetMonster = FindAroundMonster(skillData.AttackCount, skillData.SkillRange);

                              if (targetMonster == null) return;

                              var targetMonsterStatus = targetMonster.GetComponent<MonsterStatus>();
                              var targetDirection = targetMonsterStatus.GetCenterTr().position - player.ProjectileTr.position;
                              targetDirection = targetDirection.normalized;

                              var spawnObj =
                                  ObjectPoolManager.Inst.Spawn(skillData.SkillName, player.ProjectileTr.position);
                              var projectile = spawnObj.GetComponent<PlayerNormalProjectile>();
                              projectile.Fire(playerStat, skillData, player.ProjectileTr.position, targetDirection);

                              currentCoolTime = 0;
                              player.StartAttack();
                          });
        }

        public PlayerSkillEffect_NormalAttack(Transform playerTr, string skillName, Player player) : base(playerTr, skillName)
        {
            this.player = player;
        }
    }
}