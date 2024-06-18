using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using UnityEngine;
using DG.Tweening;

namespace CoffeeCat
{
    public class PlayerSkillEffect
    {
        protected Transform playerTr = null;
        protected PlayerSkill playerSkillData = null;
        protected IDisposable updateDisposable = null;
        protected bool completedLoadResource = false;

        // 새로운 스킬 선택
        protected PlayerSkillEffect() { }
        protected PlayerSkillEffect(Transform playerTr, PlayerSkill playerSkillData)
        {
            this.playerTr = playerTr;
            this.playerSkillData = playerSkillData;

            StageManager.Inst.AddListenerClearedRoomEvent(roomData =>
            {
                if (roomData.RoomType == RoomType.MonsterSpawnRoom) OnDispose();
            });

            SafeLoader.Request(playerSkillData.SkillName, onCompleted: completed =>
            {
                completedLoadResource = completed;
                if (!completed)
                    CatLog.WLog("PlayerSkillEffect : Load Resource Failed");
            });
        }

        // 스킬 효과
        protected virtual void SkillEffect(PlayerStat playerStat) { }

        // 보유한 스킬 선택 (등급 업)
        public virtual void UpdateSkillData(PlayerSkill updateSkillData) => playerSkillData = updateSkillData;

        // 스킬 효과 활성화
        public void ActivateSkillEffect(PlayerStat playerStat) => SkillEffect(playerStat);

        // 스킬 효과 비활성화
        public void OnDispose() => updateDisposable?.Dispose();

        #region FindMonster

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

        protected void DisplayDamageRange()
        {
            var damageRangeObj = ObjectPoolManager.Inst.Spawn("DamageRange", playerTr.position);
            damageRangeObj.transform.localScale = new Vector3(Defines.PLAYER_AREA_SKILL_VECTOR_X,
                                                              Defines.PLAYER_AREA_SKILL_VECTOR_Y, 1f);
            
            var sprite = damageRangeObj.GetComponent<SpriteRenderer>();
            sprite.DORewind();
            sprite.DOFade(0.15f, 0.5f).SetEase(Ease.Linear)
                  .OnComplete(() =>
                  {
                      sprite.DOFade(0f, 0.5f).SetEase(Ease.Linear);
                      ObjectPoolManager.Inst.Despawn(damageRangeObj, 0.25f);
                  });
        }

        #endregion
    }
}