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
        private readonly Player player = null;

        public override void UpdateSkillData(PlayerSkill updateSkillData)
        {
            player.UpgradeNormalAttack();
            SpawnSkillEffect();
        }

        public PlayerSkillEffect_NormalAttackUp(Transform playerTr, PlayerSkill playerSkillData) : base(playerTr, playerSkillData)
        {
            this.playerTr = playerTr;
            player = playerTr.GetComponent<Player>();
            player.UpgradeNormalAttack();
            SpawnSkillEffect();
        }

        private void SpawnSkillEffect()
        {
            this.ObserveEveryValueChanged(_ => completedLoadResource)
                .Where(_ => completedLoadResource)
                .Take(1)
                .Subscribe(_ =>
                {
                    var spawnObj = ObjectPoolManager.Instance.Spawn(playerSkillData.SkillName, playerTr);
                    spawnObj.transform.localPosition = Vector3.zero;
                });
        }
    }
}