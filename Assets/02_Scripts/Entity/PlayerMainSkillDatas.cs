using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils.SerializedDictionaries;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerMainSkillDatas
    {
        [ShowInInspector, ReadOnly] public IntPlayerMainSkillDictionary DataDictionary { get; private set; } = null;

        public void Initialize()
        {
            var textAsset = Resources.Load<TextAsset>("Entity/Json/PlayerMainSkill");
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonUtility.FromJson<PlayerMainSkill[]>(decText);

            DataDictionary = new IntPlayerMainSkillDictionary();
            for (int i = 0; i < datas.Length; i++)
            {
                DataDictionary.Add(datas[i].Index, datas[i]);
            }
        }
    }

    [Serializable]
    public class PlayerMainSkill
    {
        public int Index = default;
        public string SkillName = default;
        public int SkillLevel = default;
        public float SkillCoolTime = default;
        public float SkillBaseDamage = default;
        public float SkillCoefficient = default;
        public float SkillRange = default;
        public int AttackCount = default;
        public float ProjectileSpeed = default;
        public string Description = default;
    }
}