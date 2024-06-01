using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using DG.Tweening;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerNormalProjectile : PlayerProjectile
    {
        private const float maxDistance = 20f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out MonsterStatus monsterStat))
                return;

            var damageData = DamageData.GetData(projectileDamageData, monsterStat.CurrentStat);
            monsterStat.OnDamaged(damageData, true, tr.position, 3f);

            ObjectPoolManager.Instance.Despawn(gameObject);
        }

        protected override void SetDamageData(PlayerStat playerStat, float skillBaseDamage = 0f,
                                              float skillCoefficient = 1f)
        {
            projectileDamageData = new ProjectileDamageData(playerStat);
        }

        private void ProjectilePath(float speed, Vector3 startPos, Vector3 direction)
        {
            tr.DORewind();
            tr.DOMove(direction * maxDistance, speed)
              .SetRelative().SetSpeedBased().SetEase(Ease.Linear).SetDelay(0.05f).From(startPos)
              .OnComplete(() =>
              {
                  if (gameObject.activeSelf)
                      ObjectPoolManager.Instance.Despawn(gameObject);
              });
        }

        public void Fire(PlayerStat playerStat, Vector3 startPos, Vector3 direction)
        {
            SetDamageData(playerStat);
            // ProjectilePath(playerStat.ProjectileSpeed, startPos, direction);
        }
    }
}