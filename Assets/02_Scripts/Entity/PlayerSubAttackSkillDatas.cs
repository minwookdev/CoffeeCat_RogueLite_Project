using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils.SerializedDictionaries;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerSubAttackSkillDatas
    {
        [ShowInInspector, ReadOnly] public IntPlayerSubAttackSkillDictionary DataDictionary { get; private set; } = null;
        
        public void Initialize()
        {
            var textAsset = Resources.Load<TextAsset>("Entity/Json/PlayerSubAttackSkill");
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonUtility.FromJson<PlayerSubAttackSkill[]>(decText);

            DataDictionary = new IntPlayerSubAttackSkillDictionary();
            for (int i = 0; i < datas.Length; i++)
            {
                DataDictionary.Add(datas[i].Index, datas[i]);
            }
        }
    }

    [Serializable]
    public class PlayerSubAttackSkill
    {
        public int Index = default;
        public string SkillName = default;
        public int SkillLevel = default;
        public float TriggerChance = default;
        public float Damage = default;
        public float Duration = default;
        public float Description = default;
    }
}