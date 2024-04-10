using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.Utils.JsonParser;
using CoffeeCat.Utils;

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
            if (!IsDataLoaded) {
                JsonToClasses();
            }
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

            CatLog.Log("DataManager: Data Load Completed !");
        }
    }
}
