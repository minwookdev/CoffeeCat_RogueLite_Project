using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;

namespace CoffeeCat
{
    [SuppressMessage("ReSharper", "HeapView.ClosureAllocation")]
    [SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
    public class PlayerSkillProjectile : PlayerProjectile
    {
        private void OnEnable()
        {
            DespawnProjectile();
        }

        private void UpdatePosition(Transform monsterCenterTr)
        {
            this.UpdateAsObservable()
                .TakeUntilDisable(this)
                .Subscribe(_ => { tr.position = monsterCenterTr.position; });
        }

        private void DespawnProjectile()
        {
            var particleDuration = GetComponent<ParticleSystem>().main.duration;

            Observable.Timer(TimeSpan.FromSeconds(particleDuration))
                      .TakeUntilDisable(gameObject)
                      .Where(_ => gameObject.activeSelf)
                      .Subscribe(_ => { ObjectPoolManager.Inst.Despawn(gameObject); });
        }

        protected override void SetDamageData(PlayerStat playerStat, float skillBaseDamage = 0f,
                                              float skillCoefficient = 1f)
        {
            projectileDamageData = new ProjectileDamageData(playerStat, skillBaseDamage, skillCoefficient);
        }

        #region public methods

        public void AreaAttack(PlayerStat playerStat, List<MonsterStatus> monsters, float skillBaseDamage = 0f,
                               float skillCoefficient = 1f)
        {
            SetDamageData(playerStat, skillBaseDamage, skillCoefficient);

            foreach (var monster in monsters)
            {
                if (!monster.IsAlive) continue;

                DamageData damageData = DamageData.GetData(projectileDamageData, monster.CurrentStat);
                monster.OnDamaged(damageData, true);
            }
        }

        public void SingleTargetAttack(PlayerStat playerStat, MonsterStatus monster, float skillBaseDamage = 0f,
                                       float skillCoefficient = 1f)
        {
            SetDamageData(playerStat, skillBaseDamage, skillCoefficient);
            UpdatePosition(monster.GetCenterTr());

            DamageData damageData = DamageData.GetData(projectileDamageData, monster.CurrentStat);
            monster.OnDamaged(damageData, true);
        }

        #endregion
    }
}