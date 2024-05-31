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

        [ShowInInspector, ReadOnly] public TSet_PlayerStatus PlayerStatus { get; private set; } = null;
        [ShowInInspector, ReadOnly] public TSet_PlayerActiveSkills PlayerActiveSkills { get; private set; } = null;
        [ShowInInspector, ReadOnly] public TSet_PlayerPassiveSkills PlayerPassiceSkills { get; private set; } = null;

        private const string tablePath = "StaticData/Output/TableAssets";
        
        public bool IsDataLoaded { get; private set; } = false;

        protected override void Initialize()
        {
            MonsterStats = new MonsterStatDatas();
            MonsterSkills = new MonsterSkillDatas();
        }

        public void DataLoad()
        {
            if (IsDataLoaded)
                return;
            JsonToClasses();
            LoadTableSet();
            IsDataLoaded = true;
        }

        public void DataReload()
        {
            JsonToClasses();
            LoadTableSet();
        }

        /// <summary>
        /// Load Json Data to Data Class
        /// </summary>
        private void JsonToClasses()
        {
            JsonParser jsonParser = new JsonParser();
            MonsterStats.Initialize(jsonParser);
            MonsterSkills.Initialize(jsonParser);
        }

        private void LoadTableSet()
        {
            PlayerStatus = Resources.Load<TSet_PlayerStatus>($"{tablePath}/{nameof(TSet_PlayerStatus)}");
            PlayerActiveSkills =
                Resources.Load<TSet_PlayerActiveSkills>($"{tablePath}/{nameof(TSet_PlayerActiveSkills)}");
            PlayerPassiceSkills =
                Resources.Load<TSet_PlayerPassiveSkills>($"{tablePath}/{nameof(TSet_PlayerPassiveSkills)}");
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

    public PlayerSkillSelectData(string name, string desc, int index, bool isOwned)
    {
        Name = name;
        Desc = desc;
        Index = index;
        IsOwned = isOwned;
        /*Type = type;*/
        /*Icon = icon;*/
    }
}