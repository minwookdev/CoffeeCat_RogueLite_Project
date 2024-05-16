using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using DG.Tweening;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerProjectile : MonoBehaviour
    {
        private Transform tr = null;
        
        // TODO : 공격 타입 추가
        // TODO : 몬스터 충돌 시 Despawn
        
        private float damage = 0;
        private float speed = 0;
        private Vector3 direction = Vector3.zero;
        
        public float Damage => damage;
        
        private void Awake()
        {
            tr = GetComponent<Transform>();
        }
        
        public void Fire()
        {
            tr.DOMove(direction * 10f, speed)
                      .SetRelative().SetSpeedBased().SetEase(Ease.Linear).SetDelay(0.1f)
                      .OnComplete(() =>
                      {
                          ObjectPoolManager.Instance.Despawn(gameObject);
                      });
        }
        
        public void SetStat(float playerAttack, float projectileSpeed, Vector3 fireDirection)
        {
            damage = playerAttack;
            speed = projectileSpeed;
            direction = fireDirection;
        }

        // Attack Monster Sample
        /*private void OnCollisionEnter2D(Collision2D other) 
        {
            if (!other.gameObject.TryGetComponent(out MonsterStatus status)) 
                return;
            var testAttackData = new AttackData() { CalculatedDamage = 10 }; // Do Not Use Like This !!
            status.OnDamaged(testAttackData, other.GetContact(0).point);     // Use Contact Point
        }*/

        /*private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.TryGetComponent(out MonsterStatus status)) 
                return;
            var testAttackData = new AttackData() { CalculatedDamage = damage }; // Do Not Use Like This !!
            status.OnDamaged(testAttackData, tr.position);                       // Use Projectile Position When Collision
        }*/
    }
}