using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerStatus : MonoBehaviour
    {
        // 기본 스탯
        public float hp = 0;
        public float maxHp = 0;
        public float defense = 0;
        public float moveSpeed = 0;
        public float invincibleTime = 0;
        
        // 발사체 스탯
        public float attackDelay = 0;
        public float attackRange = 0;
        public float projectileSpeed = 0;
        
        // 공격력 스탯
        public float attackPower = 0;
        public float criticalChance = 0;
        public float criticalResistance = 0;
        public float criticalDamageMultiplier = 0;
        public float penetration = 0;
    }
}