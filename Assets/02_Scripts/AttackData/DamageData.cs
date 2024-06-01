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
        public static DamageData GetData(ProjectileDamageData attacker, PlayerStat defender)
        {
            var data = new DamageData();
            data.SetIsCritical(attacker.CriticalChance, defender.CriticalResistance);                // 치명타 발생 계산
            data.ApplySkillDamage(attacker.SkillDamage, attacker.BaseDamage, attacker.SkillCoefficient); // *스킬 데미지 배율 공식 적용
            data.ApplyCriticalDamage(attacker.CriticalMultiplier);                                   // 치명타 발생 시 데미지에 치명타 배율 적용
            data.ApplyDefenceValue(defender.Defense, attacker.Penetration);                          // 방어자 방어 수치 및 관통력 수치 계산
            data.CorrectionValue();                                                                  // 마이너스 데미지 보정
            return data;
        }

        /// <summary>
        /// Attacker: Monster Skill(Projectile), Defender: Player 
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="skillStat"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static DamageData GetData(MonsterStat attacker, PlayerStat defender)
        {
            var data = new DamageData();
            data.SetIsCritical(attacker.CriticalChance, defender.CriticalResistance); // 치명타 발생 계산
            data.SetDamageMinMax(attacker.Damage, attacker.DamageDeviation);          // 최소 ~ 최대 데미지 범위 내 산출
            data.ApplyCriticalDamage(attacker.CriticalMultiplier);                    // 치명타 발생 시 데미지에 치명타 배율 적용
            data.ApplyDefenceValue(defender.Defense, attacker.Penetration);           // 방어자 방어 수치 및 관통력 수치 계산
            data.CorrectionValue();                                                   // 마이너스 데미지 보정
            return data;
        }

        /// <summary>
        /// Attacker: Player Projectile, Defender: Monster 
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static DamageData GetData(ProjectileDamageData attacker, MonsterStat defender)
        {
            var data = new DamageData();
            data.SetIsCritical(attacker.CriticalChance, defender.CriticalResist);       // 치명타 발생 계산
            data.ApplySkillDamage(attacker.SkillDamage, attacker.BaseDamage, attacker.SkillCoefficient); // *스킬 데미지 배율 공식 적용
            data.ApplyCriticalDamage(attacker.CriticalMultiplier);                                   // 치명타 발생 시 데미지에 치명타 배율 적용
            data.ApplyDefenceValue(defender.Defence, attacker.Penetration);                          // 방어자 방어 수치 및 관통력 수치 계산
            data.CorrectionValue();                                                                  // 마이너스 데미지 보정
            return data;
        }

        /// <summary>
        /// Attacker: Player, Defender: Monster
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static DamageData GetData(PlayerStatus attacker, MonsterStat defender)
        {
            var data = new DamageData();
            data.SetIsCritical(attacker.CriticalChance, defender.CriticalResist); // 치명타 발생 계산
            data.SetDamageMinMax(attacker.AttackPower, attacker.DamageDeviation); // 최소 ~ 최대 데미지 범위 내 산출
            data.ApplyCriticalDamage(attacker.CriticalMultiplier);                // 치명타 발생 시 데미지에 치명타 배율 적용
            data.ApplyDefenceValue(defender.Defence, attacker.Penetration);       // 방어자 방어 수치 및 관통력 수치 계산
            data.CorrectionValue();                                               // 마이너스 데미지 보정
            return data;
        }

        private void SetDamageMinMax(float damageValue, float deviationValue) {
            CalculatedDamage = Random.Range(damageValue - (damageValue * deviationValue), damageValue + (damageValue * deviationValue));
        }
        
        private void SetIsCritical(float criticalChance, float criticalResistance) {
            IsCritical = criticalChance - criticalResistance >= Random.Range(1f, 100f);
        }

        private void ApplySkillDamage(float skillDamage, float attackerBaseDamage, float skillCoefficient) {
            CalculatedDamage = skillDamage + (attackerBaseDamage * skillCoefficient);
        }

        private void ApplyCriticalDamage(float criticalMultiplier) {
            CalculatedDamage *= IsCritical ? criticalMultiplier : 1f;
        }

        private void ApplyDefenceValue(float defenceValue, float penetrationValue) {
            float calculatedDefenceValue = defenceValue - (defenceValue * penetrationValue);
            calculatedDefenceValue = (calculatedDefenceValue < 0f) ? 0f : calculatedDefenceValue;
            CalculatedDamage -= calculatedDefenceValue;
        }

        private void CorrectionValue() {
            CalculatedDamage = (CalculatedDamage < 0f) ? 0f : CalculatedDamage;
        }
    }

    [System.Serializable]
    public class ProjectileDamageData {
        private float baseDamage;
        private float criticalChance;
        private float criticalMultiplier;
        private float penetration;
        private float skillDamage;
        private float skillCoefficient;
        
        public float BaseDamage => baseDamage;
        public float CriticalChance => criticalChance;
        public float CriticalMultiplier => criticalMultiplier;
        public float Penetration => penetration;
        public float SkillDamage => skillDamage;
        public float SkillCoefficient => skillCoefficient;

        public ProjectileDamageData(PlayerStat stat, float skillBaseDamage = 0f, float skillCoefficient = 1f) 
        {
            // Calculate Min/Max Damage
            baseDamage = Random.Range(stat.AttackPower - (stat.AttackPower * stat.DamageDeviation), stat.AttackPower + (stat.AttackPower * stat.DamageDeviation));
            criticalChance = stat.CriticalChance;
            criticalMultiplier = stat.CriticalMultiplier;
            penetration = stat.Penetration;
            skillDamage = skillBaseDamage;
            this.skillCoefficient = skillCoefficient;
        }
    }
    
    public class PlayerStatExample {
        public float Damage = 0f;               // Default: 0f
        public float DamageDeviation = 0.05f;   // Default: 5 %
        public float CriticalChance = 0.03f;    // Default: 3 % (max: 1f)
        public float CriticalMultiplier = 1.5f; // Default: 150 %   
        public float CriticalResist = 0f;       // Default: 0f
        public float Penetration = 0f;          // Default: 0% ~ 100% (max: 1f)
        public float Defence = 0f;              // Default: 0f
        // public float CoolTimeRatio = 1f;     // Default: 1f
    }
}
