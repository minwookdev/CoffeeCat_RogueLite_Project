using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils.SerializedDictionaries;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable] public class IntIntDictionary : UnitySerializedDictionary<int, float> { }
    [Serializable, CreateAssetMenu(menuName = "CoffeeCat/Scriptable Object/PlayerLevelData")]
    public class PlayerLevelData : ScriptableObject
    {
        [SerializeField] private IntIntDictionary levelToExp = default;

        private int currentLevel = 1;
        private float currentExp = 0;

        public void Initialize()
        {
            currentLevel = 1;
            currentExp = 0;
        }
        
        private float GetExpToNextLevel()
        {
            return levelToExp[currentLevel];
        }
        
        public void AddExp(float exp)
        {
            currentExp += exp;
        }
        
        public bool isReadyLevelUp()
        {
            if (currentExp >= GetExpToNextLevel())
                return true;

            return false;
        }

        public void LevelUp()
        {
            currentExp -= GetExpToNextLevel();
            currentLevel++;
        }

        public int GetCurrentLevel() => currentLevel;
        public float GetCurrentExp() => currentExp;
    }
}