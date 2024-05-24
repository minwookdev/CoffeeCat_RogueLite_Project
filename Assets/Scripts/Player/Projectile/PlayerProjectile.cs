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
        protected Transform tr = null;

        public ProjectileDamageData AttackData { get; set; } = null;

        protected virtual void Awake()
        {
            tr = GetComponent<Transform>();
        }

        public void Fire(Vector3 direction, float speed, Vector3 startPos)
        {
            ProjectilePath(direction, speed, startPos);
        }

        protected virtual void ProjectilePath(Vector3 direction, float speed, Vector3 startPos) { }
    }
}