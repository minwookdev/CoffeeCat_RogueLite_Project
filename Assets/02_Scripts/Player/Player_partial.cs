using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace CoffeeCat
{
    public partial class Player
    {
        [Title("Skill")]
        [ShowInInspector] private readonly List<PlayerSkill> ownedSkillsList = new List<PlayerSkill>();
        [ShowInInspector] private readonly Dictionary<string, PlayerSkillEffect> skillEffects = new Dictionary<string, PlayerSkillEffect>();

        private readonly UnityEvent OnUpdateSkillCompleted = new UnityEvent();

        private PlayerSkillSelectData[] SkillSelector()
        {
            var selectCount = Defines.PLAYER_SKILL_SELECT_COUNT;
            var skillSelectDataList = new List<PlayerSkillSelectData>();

            List<PlayerSkill> playerAllSkills = new List<PlayerSkill>();
            var activeSkills = DataManager.Inst.PlayerActiveSkills;
            var passiveSkills = DataManager.Inst.PlayerPassiveSkills;

            playerAllSkills.AddRange(activeSkills.DataDictionary.Values);
            playerAllSkills.AddRange(passiveSkills.DataDictionary.Values);
            playerAllSkills = playerAllSkills.Where(skill => skill.SkillName != "NormalAttack").ToList();

            // 보유중인 스킬 중 업그레이드 가능한 스킬이 있는지 확인
            var upgradeableSkills =
                ownedSkillsList.Where(skill => skill.Grade < Defines.PLAYER_SKILL_MAX_GRADE).ToList();
            if (upgradeableSkills.Any())
            {
                // 보유중인 스킬 중 랜덤으로 선택
                var randomOwnedSkill = upgradeableSkills[Random.Range(0, upgradeableSkills.Count)];

                PlayerSkill pickSkill = null;
                pickSkill = randomOwnedSkill.SkillType == SkillType.Active
                    ? activeSkills.DataDictionary[randomOwnedSkill.Index + 1]
                    : passiveSkills.DataDictionary[randomOwnedSkill.Index + 1];

                var pickSkillSelectData = new PlayerSkillSelectData(pickSkill.SkillName, pickSkill.Description,
                                                                    pickSkill.Index, (int)pickSkill.SkillType, true);
                skillSelectDataList.Add(pickSkillSelectData);

                selectCount--;
            }

            // 현재 보유중인 스킬은 새로 배울 스킬에서 제외
            foreach (var ownedSkill in ownedSkillsList)
                playerAllSkills = playerAllSkills.Where(skill => skill.SkillName != ownedSkill.SkillName).ToList();

            // 배우지 않은 스킬 중 등급이 1인 스킬들을 가져옴
            var learnableSkills = playerAllSkills.Where(skill => skill.Grade == 1).ToList();

            // 배울 수 있는 스킬이 충분하지 않을 경우
            var isNotEnoughSkill = learnableSkills.Count < selectCount;
            if (isNotEnoughSkill) selectCount = learnableSkills.Count;

            for (int i = 0; i < selectCount; i++)
            {
                var randomIndex = Random.Range(0, learnableSkills.Count);
                var pickSkill = learnableSkills[randomIndex];
                var pickSkillSelectData = new PlayerSkillSelectData(pickSkill.SkillName, pickSkill.Description,
                                                                    pickSkill.Index, (int)pickSkill.SkillType, false);

                learnableSkills.RemoveAt(randomIndex);
                skillSelectDataList.Add(pickSkillSelectData);
            }

            // 배울 수 있는 스킬이 부족했다면 데이터를 채우기 위해 빈 데이터 추가
            if (isNotEnoughSkill)
            {
                var overCount = Defines.PLAYER_SKILL_SELECT_COUNT - skillSelectDataList.Count;
                for (int i = 0; i < overCount; i++)
                {
                    var emptyData = new PlayerSkillSelectData("", "배울 수 있는 스킬이 없어 !", -1, -1, false);
                    skillSelectDataList.Add(emptyData);
                    selectCount--;
                }
            }

            return skillSelectDataList.ToArray();
        }

        private void InstantiateSkillEffect(PlayerSkill skillData)
        {
            switch (skillData.SkillName)
            {
                case "Explosion":
                    Explosion(skillData);
                    break;
                case "Beam":
                    Beam(skillData);
                    break;
                case "Bubble":
                    Bubble(skillData);
                    break;
                case "NormalAttackUp":
                    NormalAttackUp(skillData);
                    break;
                default:
                    PassiveSkill(skillData);
                    break;
            }
        }

        private void ActivateSkill(PlayerSkillEffect skillEffect)
        {
            this.ObserveEveryValueChanged(_ => isPlayerInBattle)
                .Where(_ => isPlayerInBattle)
                .Subscribe(_ => { skillEffect.ActivateSkillEffect(stat); });
        }

        public void UpdateSkill(PlayerSkillSelectData data)
        {
            if (data.Index == -1) return;

            PlayerSkill getSkill = null;
            getSkill = data.Type == 0
                ? DataManager.Inst.PlayerActiveSkills.DataDictionary[data.Index]
                : DataManager.Inst.PlayerPassiveSkills.DataDictionary[data.Index];

            if (data.IsOwned)
            {
                var prevSkill = ownedSkillsList.Find
                    (skill => skill.SkillName == getSkill.SkillName);
                ownedSkillsList.Remove(prevSkill);
                ownedSkillsList.Add(getSkill);

                var skillEffect = skillEffects[getSkill.SkillName];
                skillEffect.UpdateSkillData(getSkill);
            }
            else
            {
                InstantiateSkillEffect(getSkill);
                ownedSkillsList.Add(getSkill);
            }

            OnUpdateSkillCompleted.Invoke();
        }

        public void GetCoolTimeReduce(float delta)
        {
            // 보유 중인 액티브 스킬 중 쿨타임이 있는 스킬들을 찾아서 쿨타임을 감소
            var ownedActiveSkills = 
                ownedSkillsList.Where(skill => skill.SkillType == SkillType.Active).ToList();
            foreach (var ownedActiveSkill in ownedActiveSkills)
            {
                if (ownedActiveSkill is not PlayerActiveSkill activeSkill) continue;
                activeSkill.SkillCoolTime *= delta;
            }

            // 이후 새로 스킬을 배울 때마다 쿨타임을 감소시키기 위해 이벤트 추가
            OnUpdateSkillCompleted.AddListener(() => CoolTimeReduce(delta));
        }

        public void EnableSkillSelect()
        {
            UIPresenter.Inst.OpenSkillSelectPanel(SkillSelector());
        }

        #region Skill Effect

        private void Explosion(PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Explosion(tr, skillData); // 스킬 효과 객체 생성
            skillEffects.Add(skillData.SkillName, skillEffect);               // 스킬 효과 딕셔너리에 추가 (Key : 스킬 이름)
            OnPlayerDead.AddListener(skillEffect.OnDispose);                  // 플레이어가 죽으면 스킬 효과 비활성화
            ActivateSkill(skillEffect);                                       // 스킬 효과 활성화
        }

        private void Beam(PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Beam(tr, skillData);
            OnPlayerDead.AddListener(skillEffect.OnDispose);
            skillEffects.Add(skillData.SkillName, skillEffect);
            ActivateSkill(skillEffect);
        }

        private void Bubble(PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Bubble(tr, skillData);
            OnPlayerDead.AddListener(skillEffect.OnDispose);
            skillEffects.Add(skillData.SkillName, skillEffect);
            ActivateSkill(skillEffect);
        }

        private void NormalAttackUp(PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_NormalAttackUp(tr, skillData);
            skillEffects.Add(skillData.SkillName, skillEffect);
            ActivateSkill(skillEffect);
        }

        private void PassiveSkill(PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Passive(tr, skillData);
            skillEffects.Add(skillData.SkillName, skillEffect);
            skillEffect.ActivateSkillEffect(stat);
        }

        // Skill을 업데이트 한 시점의 ownedSkillsList의 마지막 아이템은 언제나 업데이트된 Skill
        private void CoolTimeReduce(float delta)
        {
            var newSkill = ownedSkillsList.Last();

            if (newSkill is not PlayerActiveSkill activeSkill)
                return;

            // 최소 쿨타임 0.1초
            if (activeSkill.SkillCoolTime - delta < 0)
                activeSkill.SkillCoolTime = 0.1f;
            else
                activeSkill.SkillCoolTime *= delta;
        }

        #endregion
    }
}