using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils.SerializedDictionaries;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerActiveSkillDatas : PlayerSkillDatas
    {
        [ShowInInspector, ReadOnly] public IntPlayerActiveSkillDictionary DataDictionary { get; private set; } = null;
        
        public void Initialize()
        {
            var textAsset = Resources.Load<TextAsset>("Entity/Json/PlayerActiveSkill");
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonConvert.DeserializeObject<PlayerActiveSkill[]>(decText);

            DataDictionary = new IntPlayerActiveSkillDictionary();
            for (int i = 0; i < datas.Length; i++)
            {
                DataDictionary.Add(datas[i].Index, datas[i]);
            }
        }
    }

    [Serializable]
    public class PlayerActiveSkill : PlayerSkill
    {
        public float SkillCoolTime = default;
        public float SkillBaseDamage = default;
        public float SkillCoefficient = default;
        public float SkillRange = default;
        public int AttackCount = default;
        public float ProjectileSpeed = default;
        public float Duration = default;
    }
}