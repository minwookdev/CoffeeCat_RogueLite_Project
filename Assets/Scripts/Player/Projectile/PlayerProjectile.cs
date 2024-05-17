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
    // TODO : 공격타입?
    public class PlayerProjectile : MonoBehaviour
    {
        private Transform tr = null;

        public ProjectileDamageData AttackData { get; set; } = null;

        private void Awake()
        {
            tr = GetComponent<Transform>();
        }

        public void Fire(Vector3 direction, float speed, Vector3 startPos)
        {
            tr.DORewind();
            tr.DOMove(direction * 10f, speed)
              .SetRelative().SetSpeedBased().SetEase(Ease.Linear).SetDelay(0.1f).From(startPos)
              .OnComplete(() => ObjectPoolManager.Instance.Despawn(gameObject));
        }

        // Attack Monster Sample
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out MonsterStatus monsterStat))
                return;

            var damageData =
                DamageData.GetDamageData(AttackData, monsterStat.CurrentStat); // Do Not Use Like This !!
            monsterStat.OnDamaged(damageData, true);                           // Use Contact Point

            ObjectPoolManager.Instance.Despawn(gameObject);
        }

        /*private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.TryGetComponent(out MonsterStatus status))
                return;
            var testAttackData = new AttackData() { CalculatedDamage = damage }; // Do Not Use Like This !!
            status.OnDamaged(testAttackData, tr.position);                       // Use Projectile Position When Collision
        }*/
    }
}