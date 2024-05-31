using System;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.JsonParser;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat.Datas {
    [Serializable]
    public class MonsterSkillDatas {
        [ShowInInspector, ReadOnly] public StringMonsterSkillDictionary DataDictionary { get; private set; } = null;

        public void Initialize(JsonParser jsonParser) {
            MonsterSkillStat[] datas = jsonParser.LoadFromJsonInResources<MonsterSkillStat>("Entity/Json/MonsterSkillStat");
            DataDictionary = new StringMonsterSkillDictionary();
            for (int i = 0; i < datas.Length; i++) {
                DataDictionary.Add(datas[i].Key, datas[i]);
            }
        }
    }

    [Serializable]
    public class MonsterSkillStat {
        public string Key = string.Empty; // Projectile Spawn Key
        public string Name = string.Empty;
        public string Desc = string.Empty;
        public float Damage = default;
        public float Ratio = default;
        public float Cost = default;
    }
}