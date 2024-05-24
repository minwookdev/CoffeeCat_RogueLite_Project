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

        public PlayerSkillEffect(PlayerSkillsKey skillKey)
        {
            skillData = DataManager.Instance.PlayerSkills[(int)skillKey];
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(skillData.SkillName, true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }
        
        public void Fire(Transform playerTr)
        {
            CatLog.Log("Fire");
            SkillEffect(playerTr);
        }

        protected virtual void SkillEffect(Transform playerTr)
        {
            CatLog.Log("SkillEffect : base");
        }
    }
}