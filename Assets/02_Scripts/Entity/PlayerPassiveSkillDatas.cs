using System;
using CoffeeCat.Utils.SerializedDictionaries;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerPassiveSkillDatas : PlayerSkillDatas
    {
        [ShowInInspector, ReadOnly] public IntPlayerPassiveSkillDictionary DataDictionary { get; private set; } = null;
        
        public void Initialize()
        {
            var textAsset = Resources.Load<TextAsset>("Entity/Json/PlayerPassiveSkill");
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonConvert.DeserializeObject<PlayerPassiveSkill[]>(decText);

            DataDictionary = new IntPlayerPassiveSkillDictionary();
            for (int i = 0; i < datas.Length; i++)
            {
                DataDictionary.Add(datas[i].Index, datas[i]);
            }
        }
    }

    [Serializable]
    public class PlayerPassiveSkill : PlayerSkill
    {
        public float Delta = default;
    }
}