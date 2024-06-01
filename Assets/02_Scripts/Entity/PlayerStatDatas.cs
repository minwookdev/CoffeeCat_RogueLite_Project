using System;
using CoffeeCat.Utils.SerializedDictionaries;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerStatDatas
    {
        [ShowInInspector, ReadOnly] public StringPlayerStatDictionary DataDictionary { get; private set; } = null;

        public void Initialize()
        {
            var textAsset = Resources.Load<TextAsset>("Entity/Json/PlayerStat");
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonConvert.DeserializeObject<PlayerStat[]>(decText);
            
            DataDictionary = new StringPlayerStatDictionary();
            for (int i = 0; i < datas.Length; i++)
            {
                DataDictionary.Add(datas[i].Name, datas[i]);
            }
        }
    }
    
    [Serializable]
    public class PlayerStat
    {
        public int Index = default;
        public string Name = string.Empty;
        public float MaxHp = default;
        public float CurrentHp = default;
        public float Defense = default;
        public float MoveSpeed = default;
        public float InvincibleTime = default;
        public float AttackPower = default;
        public float CriticalChance = default;
        public float CriticalResistance = default;
        public float CriticalMultiplier = default;
        public float DamageDeviation = default;
        public float Penetration = default;

        public void Initialize()
        {
            CurrentHp = MaxHp;
        }
        
        public void StatEnhancement(PlayerStatEnhanceData enhanceData)
        {
            MaxHp += enhanceData.MaxHp;
            // Defence += enhanceData.Defence;
            MoveSpeed += enhanceData.MoveSpeed;
            AttackPower += enhanceData.AttackPower;
        }
    }
}