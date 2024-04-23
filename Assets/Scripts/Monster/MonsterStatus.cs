using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;

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

        private void Damaged(in AttackData attackData) {
            float finalCalculatedDamageCount = attackData.CalculatedDamage;
            float tempHealthPoint = CurrentStat.HP - finalCalculatedDamageCount;
            if (tempHealthPoint < 0f) {
                CurrentStat.HP = 0f;

                // Set Monster Death
            }
            else {
                CurrentStat.HP = tempHealthPoint;
            }

            //int floatingDamgeCount = Mathf.RoundToInt(damageCount);
        }

        private void Attack(in AttackData attackData) {

        }
    }
}
