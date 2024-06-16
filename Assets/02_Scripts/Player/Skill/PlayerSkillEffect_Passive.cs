using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Passive : PlayerSkillEffect
    {
        private const string passiveAddressableKey = "Passive";

        protected override void SkillEffect(PlayerStat playerStat)
        {
            if (playerSkillData is not PlayerPassiveSkill skillData)
            {
                CatLog.WLog("PlayerSkillEffect_Passive : skillData is null");
                return;
            }

            switch (skillData.SkillName)
            {
                case "SpeedUp":
                    SpeedUp(playerStat, skillData);
                    break;
                case "DamageIncrease":
                    DamageIncrease(playerStat, skillData);
                    break;
                case "CoolTimeReduce":
                    CoolTimeReduce(skillData);
                    break;
            }

            PassiveSkillEffectSpawn();
        }

        public override void UpdateSkillData(PlayerSkill updateSkillData)
        {
            playerSkillData = updateSkillData;
            PassiveSkillEffectSpawn();
        }

        private void PassiveSkillEffectSpawn()
        {
            this.ObserveEveryValueChanged(_ => completedLoadResource)
                .Where(completed => completed)
                .Take(1)
                .Subscribe(_ =>
                {
                    var spawnObj = ObjectPoolManager.Inst.Spawn(passiveAddressableKey, playerTr);
                    spawnObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                });
        }

        public PlayerSkillEffect_Passive(Transform playerTr, PlayerSkill playerSkillData)
        {
            this.playerTr = playerTr;
            this.playerSkillData = playerSkillData;

            SafeLoader.RequestRegist(passiveAddressableKey, spawnCount:5, onCompleted: completed =>
            {
                completedLoadResource = completed;
                
                if (!completed)
                    CatLog.WLog("PlayerSkillEffect_Passive : Passive Skill Effect Load Failed");
            });
        }

        #region PassiveSkillEffect

        private void SpeedUp(PlayerStat stat, PlayerPassiveSkill skillData) => stat.MoveSpeed += skillData.Delta;

        private void DamageIncrease(PlayerStat stat, PlayerPassiveSkill skillData) =>
            stat.AttackPower += skillData.Delta;

        private void CoolTimeReduce(PlayerPassiveSkill skillData)
        {
            var player = playerTr.GetComponent<Player>();
            player.GetCoolTimeReduce(skillData.Delta);
        }

        #endregion
    }
}