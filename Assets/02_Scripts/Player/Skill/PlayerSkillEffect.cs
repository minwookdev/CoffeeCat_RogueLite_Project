using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect
    {
        protected Transform playerTr = null;
        protected PlayerSkill playerSkillData = null;
        
        protected PlayerSkillEffect() { }
        protected PlayerSkillEffect(Transform playerTr, PlayerSkill playerSkillData)
        {
            this.playerTr = playerTr;
            this.playerSkillData = playerSkillData;
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(playerSkillData.SkillName, true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }

        public virtual void UpdateSkillData(PlayerSkill updateSkillData)
        {
            playerSkillData = updateSkillData;
        }

        public void ActivateSkillEffect(PlayerStat playerStat)
        {
            SkillEffect(playerStat);
        }

        protected virtual void SkillEffect(PlayerStat playerStat)
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

        protected List<MonsterStatus> FindAroundMonsters(int attackCount, float skillRange)
        {
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc
                (playerTr.position, skillRange, monsters, 1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0)
                return null;

            var targets = monsters
                          .Where(collider2D => collider2D)
                          .Select(collider2D => collider2D.GetComponent<MonsterStatus>()) 
                          .ToList();

            return targets;
        }

        protected MonsterStatus FindAroundMonster(int attackCount, float skillRange)
        {
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc(playerTr.position, skillRange, monsters,
                                                               1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0) return null;

            return monsters
                   .Where(collider2D => collider2D)
                   .Select(collider2D => collider2D.GetComponent<MonsterStatus>())
                   .FirstOrDefault();
        }
    }
}