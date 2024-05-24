using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect : MonoBehaviour
    {
        protected Table_PlayerSkills skillData = null;

        protected virtual void SkillEffect(Transform playerTr)
        {
            
        }
        
        public void Fire(Transform playerTr)
        {
            SkillEffect(playerTr);
        }
        
        public PlayerSkillEffect(PlayerSkillsKey skillKey)
        {
            skillData = DataManager.Instance.PlayerSkills[(int)skillKey];
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(skillData.name, true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }
    }
}