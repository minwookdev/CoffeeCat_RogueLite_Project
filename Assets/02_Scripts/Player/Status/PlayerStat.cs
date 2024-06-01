using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerStatus
    {
        // 기본 스탯
        public float MaxHp = 0;
        public float CurrentHp = 0;
        public float Defence = 0;
        public float MoveSpeed = 0;
        public float InvincibleTime = 0;
        
        // 발사체 스탯
        public float AttackDelay = 0;
        public float AttackRange = 0;
        public float ProjectileSpeed = 0;
        
        // 공격력 스탯
        public float AttackPower = 0;
        public float CriticalChance = 0;
        public float CriticalResistance = 0;
        public float CriticalMultiplier = 0;
        public float DamageDeviation = 0;
        public float Penetration = 0;

        public void StatEnhancement(PlayerStatEnhanceData enhanceData)
        {
            MaxHp += enhanceData.MaxHp;
            Defence += enhanceData.Defence;
            MoveSpeed += enhanceData.MoveSpeed;
            AttackRange += enhanceData.AttackRange;
            AttackPower += enhanceData.AttackPower;
        }
    }
    
    public class PlayerStatEnhanceData
    {
        public float MaxHp = 0;
        public float Defence = 0;
        public float MoveSpeed = 0;
        public float AttackRange = 0;
        public float AttackPower = 0;
    }
}