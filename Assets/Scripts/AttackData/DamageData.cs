using UnityEngine;

namespace CoffeeCat.Datas {
    public struct DamageData
    {
        // Keep Less Than 16Byte
        public float CalculatedDamage; // 4B
        public bool IsCritical;        // 1B
        // TODO: +- 값 보정

        /// <summary>
        /// Attacker: Monster, Defender: Player
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static DamageData GetData(ProjectileDamageData attacker, PlayerStatus defender)
        {
            var damageData = new DamageData();

            // 치명타 발생 계산
            damageData.IsCritical = (attacker.CriticalChance - defender.CriticalResistance) >= Random.Range(1f, 100f);

            // 최소 ~ 최대 데미지 범위 내 산출
            /*damageData.CalculatedDamage = Random.Range(attackerStat.MinDamage, attackerStat.MaxDamage);*/

            // *스킬 데미지 배율 공식 적용
            damageData.CalculatedDamage = attacker.SkillDamage + (attacker.Damage * attacker.SkillCoefficient);

            // 치명타 발생 시 데미지에 치명타 배율 적용
            damageData.CalculatedDamage *= (damageData.IsCritical) ? attacker.CriticalMultiplier : 1f;

            // 방어자 방어 수치 및 관통력 수치 계산
            float defenceValue = defender.Defence - (defender.Defence * attacker.Penetration);
            defenceValue = (defenceValue < 0f) ? 0f : defenceValue;
            damageData.CalculatedDamage -= defenceValue;

            // 마이너스 데미지 보정
            damageData.CalculatedDamage = (damageData.CalculatedDamage < 0f) ? 0f : damageData.CalculatedDamage;
            return damageData;
        }

        /// <summary>
        /// Attacker: Monster Skill(Projectile), Defender: Player 
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="skillStat"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static DamageData GetData(MonsterStat attacker, PlayerStatus defender)
        {
            var damageData = new DamageData();

            // 치명타 발생 계산
            damageData.IsCritical = (attacker.CriticalChance - defender.CriticalResistance) >= Random.Range(1f, 100f);

            // 최소 ~ 최대 데미지 범위 내 산출
            damageData.CalculatedDamage = Random.Range(attacker.Damage - (attacker.Damage * attacker.DamageDeviation),
                                                       attacker.Damage + (attacker.Damage * attacker.DamageDeviation));

            // *스킬 데미지 배율 공식 적용
            /*damageData.CalculatedDamage = data.SkillDamage + (data.Damage * data.SkillCoefficient);*/

            // 치명타 발생 시 데미지에 치명타 배율 적용
            damageData.CalculatedDamage *= (damageData.IsCritical) ? attacker.CriticalMultiplier : 1f;

            // 방어자 방어 수치 및 관통력 수치 계산
            float defenceValue = defender.Defence - (defender.Defence * attacker.Penetration);
            defenceValue = (defenceValue < 0f) ? 0f : defenceValue;
            damageData.CalculatedDamage -= defenceValue;

            // 마이너스 데미지 보정
            damageData.CalculatedDamage = (damageData.CalculatedDamage < 0f) ? 0f : damageData.CalculatedDamage;
            return damageData;
        }

        /// <summary>
        /// Attacker: Player Projectile, Defender: Monster 
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static DamageData GetData(ProjectileDamageData attacker, MonsterStat defender)
        {
            var damageData = new DamageData();

            // 치명타 발생 계산
            damageData.IsCritical = (attacker.CriticalChance - defender.CriticalResist) >= Random.Range(1f, 100f);

            // 최소 ~ 최대 데미지 범위 내 산출
            /*damageData.CalculatedDamage = Random.Range(attackerStat.MinDamage, attackerStat.MaxDamage);*/

            // *스킬 데미지 배율 공식 적용
            damageData.CalculatedDamage = attacker.SkillDamage + (attacker.Damage * attacker.SkillCoefficient);

            // 치명타 발생 시 데미지에 치명타 배율 적용
            damageData.CalculatedDamage *= (damageData.IsCritical) ? attacker.CriticalMultiplier : 1f;

            // 방어자 방어 수치 및 관통력 수치 계산
            float defenceValue = defender.Defence - (defender.Defence * attacker.Penetration);
            defenceValue = (defenceValue < 0f) ? 0f : defenceValue;
            damageData.CalculatedDamage -= defenceValue;

            // 마이너스 데미지 보정
            damageData.CalculatedDamage = (damageData.CalculatedDamage < 0f) ? 0f : damageData.CalculatedDamage;
            return damageData;
        }

        /// <summary>
        /// Attacker: Player, Defender: Monster
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static DamageData GetData(PlayerStatus attacker, MonsterStat defender)
        {
            var damageData = new DamageData();

            // 치명타 발생 계산
            damageData.IsCritical = (attacker.CriticalChance - defender.CriticalResist) >= Random.Range(1f, 100f);

            // 최소 ~ 최대 데미지 범위 내 산출
            damageData.CalculatedDamage = Random.Range(attacker.AttackPower - (attacker.AttackPower * attacker.DamageDeviation),
                                                       attacker.AttackPower + (attacker.AttackPower * attacker.DamageDeviation));

            // *스킬 데미지 배율 공식 적용
            /*damageData.CalculatedDamage = data.SkillDamage + (data.Damage * data.SkillCoefficient);*/

            // 치명타 발생 시 데미지에 치명타 배율 적용
            damageData.CalculatedDamage *= (damageData.IsCritical) ? attacker.CriticalMultiplier : 1f;

            // 방어자 방어 수치 및 관통력 수치 계산
            float defenceValue = defender.Defence - (defender.Defence * attacker.Penetration);
            defenceValue = (defenceValue < 0f) ? 0f : defenceValue;
            damageData.CalculatedDamage -= defenceValue;

            // 마이너스 데미지 보정
            damageData.CalculatedDamage = (damageData.CalculatedDamage < 0f) ? 0f : damageData.CalculatedDamage;
            return damageData;
        }
    }

    [System.Serializable]
    public class ProjectileDamageData {
        public float Damage;
        public float CriticalChance;
        public float CriticalMultiplier;
        public float Penetration;
        public float SkillDamage = 100f;
        public float SkillCoefficient = 1f;

        public static ProjectileDamageData GetData(PlayerStatus stat) {
            var data = new ProjectileDamageData();
            // 최소 ~ 최대 데미지 범위 내 산출
            data.Damage = Random.Range(stat.AttackPower - (stat.AttackPower * stat.DamageDeviation), stat.AttackPower + (stat.AttackPower * stat.DamageDeviation));
            data.CriticalChance = stat.CriticalChance;
            data.CriticalMultiplier = stat.CriticalMultiplier;
            data.Penetration = stat.Penetration;
            // data.SkillDamage = 100f;
            // data.SkillCoefficient = 1f;
            return data;
        }
    }
    
    public class PlayerStatExample {
        public float Damage = 0f;               // Default: 0f
        public float DamageDeviation = 0.05f;   // Default: 5 %
        public float CriticalChance = 0.03f;    // Default: 3 % (max: 1f)
        public float CriticalMultiplier = 1.5f; // Default: 150 %   
        public float CriticalResist = 0f;       // Default: 0f
        public float Penetration = 0f;          // Default: 0% ~ 100% (max: 1f)
        public float Defence = 0f;                // Default: 0f
        // public float CoolTimeRatio = 1f;        // Default: 1f
    }
}
