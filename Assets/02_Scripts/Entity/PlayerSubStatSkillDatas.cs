using System;
using CoffeeCat.Utils.SerializedDictionaries;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerSubStatSkillDatas
    {
        [ShowInInspector, ReadOnly] public IntPlayerSubStatSkillDictionary DataDictionary { get; private set; } = null;
        
        public void Initialize()
        {
            var textAsset = Resources.Load<TextAsset>("Entity/Json/PlayerSubStatSkill");
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonConvert.DeserializeObject<PlayerSubStatSkill[]>(decText);

            DataDictionary = new IntPlayerSubStatSkillDictionary();
            for (int i = 0; i < datas.Length; i++)
            {
                DataDictionary.Add(datas[i].Index, datas[i]);
            }
        }
    }

    [Serializable]
    public class PlayerSubStatSkill
    {
        public int Index = default;
        public string SkillName = default;
        public int SkillLevel = default;
        public float Delta = default;
        public string Description = default;
    }
}