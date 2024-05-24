using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Datas;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillProjectile : PlayerProjectile
    {
        private ProjectileDamageData damageData = null;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out MonsterStatus monster))
            {
                
            }
        }

        public void SetDamageData(PlayerStatus playerStatus,  float skillBaseDamage = 0f, float skillCoefficient = 1f)
        {
            damageData = new ProjectileDamageData(playerStatus, skillBaseDamage, skillCoefficient);
        }
    }
}