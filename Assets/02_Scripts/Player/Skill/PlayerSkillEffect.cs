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
        protected Transform playerTr = null;
        protected Table_PlayerSkills skillData = null;
        public Table_PlayerSkills SkillData => skillData;

        public PlayerSkillEffect(Transform playerTr, Table_PlayerSkills skillData)
        {
            this.playerTr = playerTr;
            this.skillData = skillData;
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(skillData.SkillKey, true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }

        public void UpdateSkillData(Table_PlayerSkills skillData)
        {
            this.skillData = skillData;
        }

        public void Fire(PlayerStatus playerStat)
        {
            SkillEffect(playerStat);
        }

        protected virtual void SkillEffect(PlayerStatus playerStat)
        {
        }

        protected List<MonsterStatus> FindAllMonsters()
        {
            var monsters = Physics2D.OverlapBoxAll(playerTr.position,
                                                   new Vector2(Defines.PLAYER_AREA_SKILL_VECTOR_X,
                                                               Defines.PLAYER_AREA_SKILL_VECTOR_Y), 0f,
                                                   1 << LayerMask.NameToLayer("Monster"));

            if (monsters.Length <= 0) return null;

            return monsters.Select(mon => mon.GetComponent<MonsterStatus>())
                           .ToList();
        }

        protected List<MonsterStatus> FindAroundMonsters(int attackCount)
        {
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc
                (playerTr.position, skillData.SkillRange, monsters, 1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0)
                return null;

            var targets = monsters
                          .Where(collider2D => collider2D)
                          .Select(collider2D => collider2D.GetComponent<MonsterStatus>()) 
                          .ToList();

            return targets;
        }

        protected MonsterStatus FindAroundMonster(int attackCount)
        {
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc(playerTr.position, skillData.SkillRange, monsters,
                                                               1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0) return null;

            return monsters
                   .Where(collider2D => collider2D)
                   .Select(collider2D => collider2D.GetComponent<MonsterStatus>())
                   .FirstOrDefault();
        }
    }
}