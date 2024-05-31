using System;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.JsonParser;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat.Datas {
    [Serializable]
    public class MonsterStatDatas {
        [ShowInInspector, ReadOnly] public StringMonsterStatDictionary DataDictionary { get; private set; } = null;

        public void Initialize(JsonParser jsonParser) {
            // Get Data From Resources in Json
            MonsterStat[] datas = jsonParser.LoadFromJsonInResources<MonsterStat>("Entity/Json/MonsterStat");
            //CatLog.Log($"Datas 0 index name: {datas[0].Name}, desc: {datas[0].Desc}");

            // Add to Data Dictionary
            DataDictionary = new StringMonsterStatDictionary();
            for (int i = 0; i < datas.Length; i++) {
                DataDictionary.Add(datas[i].Name, datas[i]);
            }
        }
    }

    [Serializable]
    public class MonsterStat {
        // private fields (Show in Inspector)
        // [Required: SerializeField Attributes]
        // [Optional: public Property]
        //[SerializeField] private string name = string.Empty;
        //[SerializeField] private string desc = string.Empty;
        //[SerializeField] private float hp = default;
        //[SerializeField] private float mp = default;
        //
        //public string Name { get => name; }
        //public string Desc { get => desc; }
        //public float HP { get => hp; }
        //public float MP { get => mp; }

        // or

        // public fields
        // [Required: public access restrictor]
        public string Name = string.Empty;
        public string Desc = string.Empty;
        public float HP = default;
        public float MP = default;
        public float MinDamage = default;
        public float MaxDamage = default;
        public float Damage = 0f;
        public float DamageDeviation = 0.05f;
        public float CriticalChance = default;
        public float CriticalMultiplier = default;
        public float Defence = default;
        public float CriticalResist = default;
        public float Penetration = default;

        public MonsterStat DeepCopyMonsterStat() {
            var newStat = new MonsterStat();
            newStat.Name = this.Name;
            newStat.Desc = this.Desc;
            newStat.HP = this.HP;
            newStat.MP = this.MP;
            newStat.MinDamage = this.MinDamage;
            newStat.MaxDamage = this.MaxDamage;
            newStat.CriticalChance = this.CriticalChance;
            newStat.CriticalMultiplier = this.CriticalMultiplier;
            newStat.Defence = this.Defence;
            newStat.CriticalResist = this.CriticalResist;
            newStat.Penetration = this.Penetration;
            return newStat;
        }

        public void CopyValue(MonsterStat originStat) {
            this.Name = originStat.Name;
            this.Desc = originStat.Desc;
            this.HP = originStat.HP;
            this.MP = originStat.MP;
            this.MinDamage = originStat.MinDamage;
            this.MaxDamage = originStat.MaxDamage;
            this.CriticalChance = originStat.CriticalChance;
            this.CriticalMultiplier = originStat.CriticalMultiplier;
            this.Defence = originStat.Defence;
            this.CriticalResist = originStat.CriticalResist;
            this.Penetration = originStat.Penetration;
        }
    }
}
