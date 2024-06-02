using System;
using System.Collections;
using System.Collections.Generic;
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

        public void Initialize()
        {
            // Json 파일 읽어오기
        }

        public void SaveJson()
        {
            // TODO : Path - 세이브 파일 경로
            
            // Json 파일로 저장
            
            // 암호화
            
        }
    }
}