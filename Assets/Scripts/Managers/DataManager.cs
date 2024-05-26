using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.Utils.JsonParser;
using CoffeeCat.Utils;
using UnityEditor.iOS;
using UnityEngine;

namespace CoffeeCat.FrameWork {
    public class DataManager : GenericSingleton<DataManager> {
        [ShowInInspector, ReadOnly] public MonsterStatDatas MonsterStats { get; private set; } = null;
        [ShowInInspector, ReadOnly] public MonsterSkillDatas MonsterSkills { get; private set; } = null;

        public bool IsDataLoaded { get; private set; } = false;

        protected override void Initialize() {
            MonsterStats = new MonsterStatDatas();
            MonsterSkills = new MonsterSkillDatas();
        }

        public void DataLoad() {
            if (IsDataLoaded) 
                return;
            JsonToClasses();
            IsDataLoaded = true;
        }

        public void DataReload() {
            JsonToClasses();
        }

        /// <summary>
        /// Load Json Data to Data Class
        /// </summary>
        private void JsonToClasses() {
            JsonParser jsonParser = new JsonParser();
            MonsterStats.Initialize(jsonParser);
            MonsterSkills.Initialize(jsonParser);
        }
    }

    public class PlayerSkillSelectData {
        public string Name;
        public string Desc;
        public Sprite Icon;
        public int Index;
        public int Type; // 0: Passive, 1: Active
        
        public PlayerSkillSelectData(string name, string desc, int index) {
            Name = name;
            Desc = desc;
            Index = index;
            /*Type = type;*/
            /*Icon = icon;*/
        }
    }
}
