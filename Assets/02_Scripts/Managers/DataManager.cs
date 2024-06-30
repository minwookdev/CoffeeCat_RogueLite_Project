using System;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.JsonParser;
using UnityEngine;

namespace CoffeeCat.FrameWork
{
    public class DataManager : DynamicSingleton<DataManager>
    {
        [ShowInInspector, ReadOnly] public MonsterStatDatas MonsterStats { get; private set; } = null;
        [ShowInInspector, ReadOnly] public MonsterSkillDatas MonsterSkills { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerStatDatas PlayerStats { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerMainSkillDatas PlayerMainSkills { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerSubAttackSkillDatas PlayerSubAttackSkills { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerSubStatSkillDatas PlayerSubStatSkills { get; private set; } = null;
        
        public bool IsDataLoaded { get; private set; } = false;

        protected override void Initialize()
        {
            MonsterStats = new MonsterStatDatas();
            MonsterSkills = new MonsterSkillDatas(); 
            PlayerStats = new PlayerStatDatas();
            PlayerMainSkills = new PlayerMainSkillDatas();
            PlayerSubAttackSkills = new PlayerSubAttackSkillDatas();
            PlayerSubStatSkills = new PlayerSubStatSkillDatas();
            DataLoad();
        }

        public void DataLoad()
        {
            if (IsDataLoaded)
                return;
            JsonToClasses();
            IsDataLoaded = true;
        }

        public void DataReload()
        {
            JsonToClasses();
        }

        /// <summary>
        /// Load Json Data to Data Class
        /// </summary>
        private void JsonToClasses()
        {
            // JsonParser jsonParser = new JsonParser();
            MonsterStats.Initialize(/*jsonParser*/);
            MonsterSkills.Initialize(/*jsonParser*/);
            PlayerStats.Initialize();
            PlayerMainSkills.Initialize();
            PlayerSubAttackSkills.Initialize();
            PlayerSubStatSkills.Initialize();
        }
    }
}

[Serializable]
public class PlayerSkillSelectData
{
    public string Name;
    public string Desc;
    public Sprite Icon;
    public int Index;
    public SkillType Type;

    public PlayerSkillSelectData(string name, string desc, int index, SkillType type)
    {
        Name = name;
        Desc = desc;
        Index = index;
        Type = type;
        /*Icon = icon;*/
    }
}