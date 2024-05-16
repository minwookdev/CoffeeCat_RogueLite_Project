using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using EMonsterState = CoffeeCat.MonsterState.EnumMonsterState;

namespace CoffeeCat {
    public class MonsterStatus : MonoBehaviour {
        [Title("MONSTER STATUS")]
        [SerializeField, DisableInPlayMode] private string customStatLoadKey = string.Empty;
        [SerializeField, ReadOnly] public MonsterStat CurrentStat { get; private set; } = null;
        private MonsterStat originStat = null; // 수정금지
        private MonsterState state = null;

        private void Start() {
            state = GetComponent<MonsterState>();
            
            // Get Origin Stat Data
            customStatLoadKey = (customStatLoadKey.Equals(string.Empty)) ? gameObject.name : customStatLoadKey;
            var monsterStatsDataDictionary = DataManager.Instance.MonsterStats.DataDictionary;
            if (!monsterStatsDataDictionary.TryGetValue(customStatLoadKey, out MonsterStat result)) {
                CatLog.WLog($"Not Found Monster Stat Data. Key: {customStatLoadKey}");
                return;
            }

            // DeepCopy Stat Data From Origin Stat
            originStat = result.DeepCopyMonsterStat();
            CurrentStat = new MonsterStat();
            CurrentStat.CopyValue(originStat);
            state.SetStat(CurrentStat);
        }

        private void Update() {
            if (!Input.GetKeyDown(KeyCode.P)) return;
            var testAttackData = new DamageData() { CalculatedDamage = 20f };
            OnDamaged(testAttackData, true);
        }

        public void OnDamaged(in DamageData data, bool useDamageText, Vector2 collisionPoint = default, float force = 0f) {
            if (state.State == EMonsterState.Death)
                return;
            
            // KnockBack Process
            Vector2 knockBackDirection = Vector2.zero;
            if (collisionPoint != default && force > 0f) {
                knockBackDirection = (Vector2)transform.position - collisionPoint;
                knockBackDirection.Normalize();
                state.AddForceToDirection(knockBackDirection, force);
            }
            
            // Damage Process
            float finalCalculatedDamageCount = data.CalculatedDamage;
            float tempHealthPoint = CurrentStat.HP - finalCalculatedDamageCount;
            if (tempHealthPoint < 0f) {
                CurrentStat.HP = 0f;
                state.OnDeath();
            }
            else {
                CurrentStat.HP = tempHealthPoint;
                state.OnTakeDamage();
            }

            if (!useDamageText)
                return;
            
            int floatingCount = Mathf.RoundToInt(finalCalculatedDamageCount);
            CatLog.Log($"damage count: {floatingCount.ToString()}");

            if (knockBackDirection != Vector2.zero) {
                DamageTextManager.Instance.OnReflectingText(floatingCount, collisionPoint, knockBackDirection);   
            }
            else {
                Vector2 flaotingStartPos = transform.position;
                flaotingStartPos.y += 1.2f;
                DamageTextManager.Instance.OnFloatingText(floatingCount, transform.position);
            }
        }

        private void Attack(in DamageData damageData) {

        }
    }
}
