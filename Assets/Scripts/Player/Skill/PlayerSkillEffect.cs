using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using UnityEditor;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect : MonoBehaviour
    {
        protected Table_PlayerSkills skillData = null;

        public PlayerSkillEffect(PlayerSkillsKey skillKey)
        {
            skillData = DataManager.Instance.PlayerSkills[(int)skillKey];
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(skillData.name, true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }
        
        public void Fire(Transform playerTr)
        {
            SkillEffect(playerTr);
        }

        private void test(string skillName)
        {
            switch (skillName)
            {
                case "Explosion":
                    break;
            }
        }
        
        protected virtual void SkillEffect(Transform playerTr)
        {
            
        }
    }
}