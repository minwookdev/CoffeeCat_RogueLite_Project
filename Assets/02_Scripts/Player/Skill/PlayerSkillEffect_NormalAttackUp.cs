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
            
            var spawnObj = ObjectPoolManager.Instance.Spawn(playerSkillData.SkillName, playerTr);
            spawnObj.transform.localPosition = Vector3.zero;
        }

        public PlayerSkillEffect_NormalAttackUp(Transform playerTr, PlayerSkill playerSkillData) : base(playerTr, playerSkillData)
        {
            this.playerTr = playerTr;
            player = playerTr.GetComponent<Player>();
            player.UpgradeNormalAttack();
            
            var spawnObj = ObjectPoolManager.Instance.Spawn(playerSkillData.SkillName, playerTr);
            spawnObj.transform.localPosition = Vector3.zero;
        }
    }
}