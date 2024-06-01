using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Passive : PlayerSkillEffect
    {
        protected override void SkillEffect(PlayerStat playerStat)
        {
            if (playerSkillData is not PlayerPassiveSkill skilldata)
            {
                CatLog.WLog("PlayerSkillEffect_Passive : skillData is null");
                return;
            }

            switch (skilldata.SkillName)
            {
                case "SpeedUp": SpeedUp(playerStat, skilldata);
                    break;
                case "CoolTimeReduce": CoolTimeReduce(skilldata);
                    break;
                case "DamageIncrease": DamageIncrease(playerStat, skilldata);
                    break;
            }
            
            var spawnObj = ObjectPoolManager.Instance.Spawn("Passive", playerTr);
            spawnObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        }

        public override void UpdateSkillData(PlayerSkill updateSkillData)
        {
            playerSkillData = updateSkillData;
            var spawnObj = ObjectPoolManager.Instance.Spawn("Passive", playerTr);
            spawnObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        }

        public PlayerSkillEffect_Passive(Transform playerTr, PlayerSkill playerSkillData)
        {
            this.playerTr = playerTr;
            this.playerSkillData = playerSkillData;
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>("Passive", true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }

        #region PassiveSkillEffect

        private void SpeedUp(PlayerStat stat, PlayerPassiveSkill skillData)
        {
            stat.MoveSpeed += skillData.Delta;
        }
        
        private void CoolTimeReduce(PlayerPassiveSkill skillData)
        {
            var player = playerTr.GetComponent<Player>();
            player.GetCoolTimeReduce(skillData.Delta);
        }
        
        private void DamageIncrease(PlayerStat stat, PlayerPassiveSkill skillData)
        {
            stat.AttackPower += skillData.Delta;
        }

        #endregion
    }
}