using UnityEngine;

namespace CoffeeCat.Datas {
    // Keep Less Than 16Byte
    public struct AttackData {
        public float CalculatedDamage; // 4B
        public bool IsCritical;        // 1B
        
        public static AttackData GetMonsterCollisionData(MonsterStat attackerStat, TempPlayerStat defenderStat) 
        {
            var attackData = new AttackData()
            {
                CalculatedDamage = 0f, IsCritical = false
            };

            // 단순 충돌 대미지니까 치명타 발생 X

            // 최소 ~ 최대 데미지 범위 내 산출
            attackData.CalculatedDamage = Random.Range(attackerStat.MinDamage, attackerStat.MaxDamage);

            // 방어자 방어 수치 및 관통력 수치 계산
            float penetratedArmorValue = defenderStat.Defence - attackerStat.Penetration;
            penetratedArmorValue = (penetratedArmorValue < 0f) ? 0f : penetratedArmorValue;
            attackData.CalculatedDamage -= penetratedArmorValue;

            // 마이너스 데미지 보정
            attackData.CalculatedDamage = (attackData.CalculatedDamage < 0f) ? 0f : attackData.CalculatedDamage;
            return attackData;
        }
        
        public static AttackData GetMonsterAttackData(MonsterStat attackerStat, TempPlayerStat defenderStat) {
            var attackData = new AttackData() {
                CalculatedDamage = 0f, IsCritical = false
            };

            // 치명타 발생 계산
            attackData.IsCritical = (attackerStat.CriticalChance - defenderStat.CriticalResist) >= Random.Range(1f, 100f);

            // 최소 ~ 최대 데미지 범위 내 산출
            attackData.CalculatedDamage = Random.Range(attackerStat.MinDamage, attackerStat.MaxDamage);

            // 치명타 발생 시 데미지에 치명타 배율 적용
            attackData.CalculatedDamage *= (attackData.IsCritical) ? attackerStat.CriticalDamageMultiplier : 1f;

            // 방어자 방어 수치 및 관통력 수치 계산
            float penetratedArmorValue = defenderStat.Defence - attackerStat.Penetration;
            penetratedArmorValue = (penetratedArmorValue < 0f) ? 0f : penetratedArmorValue;
            attackData.CalculatedDamage -= penetratedArmorValue;

            // 마이너스 데미지 보정
            attackData.CalculatedDamage = (attackData.CalculatedDamage < 0f) ? 0f : attackData.CalculatedDamage;
            return attackData;
        }

        public static AttackData GetMonsterSkillAttackData(MonsterStat attackerStat, MonsterSkillStat attackerSkillStat, TempPlayerStat defenderStat) {
            var attackData = new AttackData() { 
                CalculatedDamage= 20f, IsCritical = false
            };

            // 치명타 발생 계산
            attackData.IsCritical = (attackerStat.CriticalChance - defenderStat.CriticalResist) >= Random.Range(1f, 100f);

            // 최소 ~ 최대 데미지 범위 내 산출
            attackData.CalculatedDamage = Random.Range(attackerStat.MinDamage, attackerStat.MaxDamage);

            // *스킬 데미지 배율 공식 적용
            attackData.CalculatedDamage = attackerSkillStat.Damage + (attackData.CalculatedDamage * attackerSkillStat.Ratio);

            // 치명타 발생 시 데미지에 치명타 배율 적용
            attackData.CalculatedDamage *= (attackData.IsCritical) ? attackerStat.CriticalDamageMultiplier : 1f;

            // 방어자 방어 수치 및 관통력 수치 계산
            float penetratedArmorValue = defenderStat.Defence - attackerStat.Penetration;
            penetratedArmorValue = (penetratedArmorValue < 0f) ? 0f : penetratedArmorValue;
            attackData.CalculatedDamage -= penetratedArmorValue;

            // 마이너스 데미지 보정
            attackData.CalculatedDamage = (attackData.CalculatedDamage < 0f) ? 0f : attackData.CalculatedDamage;
            return attackData;
        }

        public static AttackData GetPlayerAttackData(TempPlayerStat playerStat, MonsterStat monsterStat) {
            var attackData = new AttackData() {
                CalculatedDamage = 0f, IsCritical = false
            };
            return attackData;
        }

#if UNITY_EDITOR
        #region DAMAGE_FORMULA
        private void IsCriticalCalc(float attackerCritChangeValue, float defenderCritResistValue) {
            IsCritical = (attackerCritChangeValue - defenderCritResistValue) >= Random.Range(1f, 100f); // 저항을 계산한 치명타 발생 체크
        }

        private void CalcDamageInRange(float attackerMinDamageValue, float attackerMaxDamageValue) {
            CalculatedDamage = Random.Range(attackerMinDamageValue, attackerMaxDamageValue); // (최소~최대)범위 내 데미지 수치 구함
        }

        private void CalcCriticalMultiplier(float attackerCriticalChangeValue) {
            CalculatedDamage = CalculatedDamage * ((IsCritical) ? attackerCriticalChangeValue : 1f); // 치명타 유무에 따른 치명타 승수 계산
        }

        private void CalcArmorAndPenetration(float attackerPenetrationValue, float defenderArmorValue) {
            float penetratedArmorValue = (defenderArmorValue - attackerPenetrationValue) < 0f ? 0f : defenderArmorValue; // 1) 관통력 수치만큼 방어 수치 차감
            CalculatedDamage = CalculatedDamage - penetratedArmorValue;                                                  // 2) 데미지 수치에서 관통력 적용된 방어력 수치만큼 차감
        }

        private void CalcDamageBySkillRatio(float attackerSkillDefaultDamageValue, float attackerSkillRatioValue) {
            CalculatedDamage = attackerSkillDefaultDamageValue + (CalculatedDamage * attackerSkillRatioValue); // attackerSkillRatioValue = 0f(0%) ~ 1f(100%)
        }
        #endregion
#endif
    }

    [System.Serializable]
    public class TempPlayerStat {
        public float HP = 0f;
        public float MinDamage = 0f;
        public float MaxDamage = 0f;
        public float CriticalChance = 0f;
        public float CriticalDamageMultiplier = 1.25f; // Default
        public float Defence = 0f;
        public float CriticalResist = 0f;
        public float Penetration = 0f;
    }
}
