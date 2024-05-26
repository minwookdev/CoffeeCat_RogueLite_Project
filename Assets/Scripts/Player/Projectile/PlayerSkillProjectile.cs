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
        public enum SkillType
        {
            NONE,
            SingleTargetAttack,
            AreaAttack,
        }
        
        [SerializeField] private SkillType skillType = SkillType.AreaAttack;
        private Transform monsterTr = null;
        private bool isAttacked = false;

        private void OnEnable()
        {
            DespawnProjectile();
        }

        private void Update()
        {
            switch (skillType)
            {
                case SkillType.AreaAttack:
                    break;
                case SkillType.SingleTargetAttack:
                    if (monsterTr)
                        tr.position = monsterTr.position;
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isAttacked)
                return;
            
            if (other.TryGetComponent(out MonsterStatus monster))
            {
                switch (skillType)
                {
                    case SkillType.AreaAttack:
                        AreaAttack(monster);
                        break;
                    case SkillType.SingleTargetAttack:
                        SingleTargetAttack(monster);
                        break;
                }
            }
        }

        private void AreaAttack(MonsterStatus monster)
        {
            DamageData damageData = DamageData.GetData(projectileDamageData, monster.CurrentStat);
            monster.OnDamaged(damageData, true);
        }
        
        private void SingleTargetAttack(MonsterStatus monster)
        {
            isAttacked = true;
            monsterTr = monster.transform;
            DamageData damageData = DamageData.GetData(projectileDamageData, monster.CurrentStat);
            monster.OnDamaged(damageData, true);
        }

        private void DespawnProjectile()
        {
            var particleDuration = GetComponent<ParticleSystem>().main.duration;

            Observable.Timer(TimeSpan.FromSeconds(particleDuration))
                      .Where(_ => gameObject.activeSelf)
                      .Subscribe(_ =>
                      {
                          isAttacked = false;
                          ObjectPoolManager.Instance.Despawn(gameObject);
                      });
        }
        
        public void SetDamageData(PlayerStatus playerStatus,  float skillBaseDamage = 0f, float skillCoefficient = 1f)
        {
            projectileDamageData = new ProjectileDamageData(playerStatus, skillBaseDamage, skillCoefficient);
        }
    }
}