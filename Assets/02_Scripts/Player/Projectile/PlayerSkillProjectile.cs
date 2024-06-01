using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillProjectile : PlayerProjectile
    {
        private void OnEnable()
        {
            DespawnProjectile();
        }

        private void UpdatePosition(Transform monsterTr)
        {
            Observable.EveryUpdate()
                      .Skip(TimeSpan.Zero)
                      .TakeUntilDisable(gameObject)
                      .Subscribe(_ => { tr.position = monsterTr.position; });
        }

        public void AreaAttack(PlayerStatus playerStatus, List<MonsterStatus> monsters, float skillBaseDamage = 0f, float skillCoefficient = 1f)
        {
            SetDamageData(playerStatus, skillBaseDamage, skillCoefficient);
            
            foreach (var monster in monsters)
            {
                if (!monster.IsAlive) continue;
                
                DamageData damageData = DamageData.GetData(projectileDamageData, monster.CurrentStat);
                monster.OnDamaged(damageData, true, tr.position, knockBackForce);
            }
        }

        public void SingleTargetAttack(PlayerStatus playerStatus, MonsterStatus monster, float skillBaseDamage = 0f, float skillCoefficient = 1f)
        {
            SetDamageData(playerStatus, skillBaseDamage, skillCoefficient);
            UpdatePosition(monster.transform);
            
            DamageData damageData = DamageData.GetData(projectileDamageData, monster.CurrentStat);
            monster.OnDamaged(damageData, true, tr.position, knockBackForce);
        }

        public void DespawnProjectile()
        {
            var particleDuration = GetComponent<ParticleSystem>().main.duration;

            Observable.Timer(TimeSpan.FromSeconds(particleDuration))
                      .TakeUntilDisable(gameObject)
                      .Where(_ => gameObject.activeSelf)
                      .Subscribe(_ => { ObjectPoolManager.Instance.Despawn(gameObject); });
        }

        protected override void SetDamageData(PlayerStatus playerStatus, float skillBaseDamage = 0f,
                                              float skillCoefficient = 1f)
        {
            projectileDamageData = new ProjectileDamageData(playerStatus, skillBaseDamage, skillCoefficient);
        }
    }
}