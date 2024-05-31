using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using DG.Tweening;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerProjectile : MonoBehaviour
    {
        [SerializeField] protected float knockBackForce = 0f; 
        protected Transform tr = null;
        protected ProjectileDamageData projectileDamageData = null;

        protected virtual void Awake()
        {
            tr = GetComponent<Transform>();
        }
        
        protected virtual void SetDamageData(PlayerStatus playerStatus, float skillBaseDamage = 0f, float skillCoefficient = 1f)
        {
            projectileDamageData = new ProjectileDamageData(playerStatus, skillBaseDamage, skillCoefficient);
        }
    }
}