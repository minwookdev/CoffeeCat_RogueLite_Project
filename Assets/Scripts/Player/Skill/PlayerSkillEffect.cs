using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using UnityEditor;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect
    {
        protected Table_PlayerSkills skillData = null;
        public Table_PlayerSkills SkillData => skillData;

        public PlayerSkillEffect(Table_PlayerSkills skillData)
        {
            this.skillData = skillData;
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(skillData.SkillKey, true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }

        public void Fire(Transform playerTr, PlayerStatus playerStat)
        {
            SkillEffect(playerTr, playerStat);
        }

        protected virtual void SkillEffect(Transform playerTr, PlayerStatus playerStat)
        {
        }

        protected List<Transform> FindAroundMonsters(Transform playerTr, int attackCount)
        {
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc
                (playerTr.position, skillData.SkillRange, monsters, 1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0)
                return null;

            var targets = new List<Transform>();
            
            for (int i = 0; i < monsterCount; i++)
            {
                targets.Add(monsters[i].transform);
            }

            return targets;
        }

        protected Transform FindAroundMonster(Transform playerTr, int attackCount)
        {
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc(playerTr.position, skillData.SkillRange, monsters,
                                            1 << LayerMask.NameToLayer("Monster"));

            return monsterCount <= 0 ? null : monsters[0].transform;
        }
    }
}