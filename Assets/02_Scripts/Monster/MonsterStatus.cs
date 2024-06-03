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
        [SerializeField] private MonsterState state = null;
        [SerializeField, DisableInPlayMode] private string customStatLoadKey = string.Empty;
        [field: SerializeField, ReadOnly] public MonsterStat CurrentStat { get; private set; } = null;
        private MonsterStat originStat = null; // 수정금지
        public bool IsAlive => CurrentStat.HP > 0f;

        private void Start() {
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
            state.SetStat(CurrentStat);
            RestoreToOriginStat();
        }

        private void OnEnable()
        {
            RestoreToOriginStat();
        }
        
        private void RestoreToOriginStat()
        {
            if (originStat == null)
                return;
            CurrentStat.CopyValue(originStat);
        }

        public void OnDamaged(in DamageData data, bool useDamageText, Vector2 collisionPoint = default, float force = 0f) {
            if (!IsAlive)
                return;
            
            Vector2 knockBackDirection = Vector2.zero;
            if (collisionPoint != default && force > 0f) {
                knockBackDirection = (Vector2)transform.position - collisionPoint;
                knockBackDirection.Normalize();
            }
            
            // Damage Process
            float finalCalculatedDamageCount = data.CalculatedDamage;
            float tempHealthPoint = CurrentStat.HP - finalCalculatedDamageCount;
            if (tempHealthPoint <= 0f) {
                StageManager.Instance.AddCurrentRoomKillCount();
                CurrentStat.HP = 0f;
                state.OnDeath();
            }
            else {
                // Decrease Monster Health Point
                CurrentStat.HP = tempHealthPoint;
                state.OnTakeDamage();
                
                // Start KnockBack Process
                if (state.IsKnockBackable) {
                    state.StartKnockBackProcessCoroutine(knockBackDirection, force);
                }
            }
            
            // Damage Text Process 
            if (!useDamageText)
                return;
            int floatingCount = Mathf.RoundToInt(finalCalculatedDamageCount);
            if (knockBackDirection != Vector2.zero) {
                DamageTextManager.Instance.OnReflectingText(floatingCount, collisionPoint, knockBackDirection, false);   
            }
            else {
                Vector2 textStartPos = collisionPoint == default ? transform.position : collisionPoint;
                textStartPos.y += 1.25f;
                DamageTextManager.Instance.OnFloatingText(floatingCount, textStartPos, false);
            }
        }

        public void ForcedKillMonster() {
            if (!IsAlive)
                return;
            
            CurrentStat.HP = 0f;
            state.OnDeath();
        }

        public Transform GetCenterTr() {
            if (state != null && state.CenterPointTr) 
                return state.CenterPointTr;
            CatLog.ELog("Monster State or Center Point Transform is Null");
            return null;
        }
    }
}
