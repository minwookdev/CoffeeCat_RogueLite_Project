using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using UnityEditor;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect
    {
        protected Table_PlayerSkills skillData = null;
        public Table_PlayerSkills SkillData => skillData;

        public PlayerSkillEffect(Table_PlayerSkills skillData)
        {
            this.skillData = skillData;
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(skillData.SkillKey, true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }
        
        public void Fire(Transform playerTr, PlayerStatus playerStat)
        {
            SkillEffect(playerTr, playerStat);
        }

        protected virtual void SkillEffect(Transform playerTr, PlayerStatus playerStat)
        {
        }
    }
}