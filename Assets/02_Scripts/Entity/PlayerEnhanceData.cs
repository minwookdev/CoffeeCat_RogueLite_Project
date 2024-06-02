using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils.Defines;
using Newtonsoft.Json;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerEnhanceData
    {
        // Json화
        // DataManager에서 읽어오기
        // StageManager에서 Player Spawn시 적용
        
        public float MaxHp = default;
        public float Defence = default;
        public float MoveSpeed = default;
        public float AttackPower = default;

        public void UpdateEnhanceData()
        {
            var jsonData = PlayerPrefs.GetString(Defines.PLAYER_ENHANCE_DATA_KEY);
            jsonData = Cryptor.Decrypt(jsonData);
            
            var enhanceData = JsonConvert.DeserializeObject<PlayerEnhanceData>(jsonData);
            
            // Player Stat에 강화 데이터 적용
        }

        // 강화 했을 때
        public void SaveEnhanceData()
        {
            var result = JsonConvert.SerializeObject(this);
            result = Cryptor.Encrypt(result);
            PlayerPrefs.SetString(Defines.PLAYER_ENHANCE_DATA_KEY, result);
        }
    }
}