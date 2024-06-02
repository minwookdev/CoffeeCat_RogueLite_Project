using System;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.Utils.JsonParser;
using UnityEngine;

namespace CoffeeCat.FrameWork
{
    public class DataManager : GenericSingleton<DataManager>
    {
        [ShowInInspector, ReadOnly] public MonsterStatDatas MonsterStats { get; private set; } = null;
        [ShowInInspector, ReadOnly] public MonsterSkillDatas MonsterSkills { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerStatDatas PlayerStats { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerActiveSkillDatas PlayerActiveSkills { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerPassiveSkillDatas PlayerPassiveSkills { get; private set; } = null;
        
        public bool IsDataLoaded { get; private set; } = false;

        protected override void Initialize()
        {
            MonsterStats = new MonsterStatDatas();
            MonsterSkills = new MonsterSkillDatas();
            PlayerStats = new PlayerStatDatas();
            PlayerActiveSkills = new PlayerActiveSkillDatas();
            PlayerPassiveSkills = new PlayerPassiveSkillDatas();
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
            PlayerActiveSkills.Initialize();
            PlayerPassiveSkills.Initialize();
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
    public int Type; // 0: Passive, 1: Active
    public bool IsOwned;

    public PlayerSkillSelectData(string name, string desc, int index, int type, bool isOwned)
    {
        Name = name;
        Desc = desc;
        Index = index;
        Type = type;
        IsOwned = isOwned;
        /*Icon = icon;*/
    }
}