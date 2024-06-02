using System;
using System.Collections.Generic;
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

        [ShowInInspector]
        private readonly Dictionary<string, PlayerSkillEffect> skillEffects = new Dictionary<string, PlayerSkillEffect>();
        private readonly UnityEvent OnUpdateSkillCompleted = new UnityEvent();

        private PlayerSkillSelectData[] SkillSelector(int SelectCount)
        {
            // TODO : Seed
            // TODO : 노션에 전달 할 기획 메모 작성 : 스킬 Select는 배틀룸 밖에서만 해야함 (InBattleRoom 변수가 변화하는 순간 업데이트 된 스킬들이 반영됨)

            var skillSelectDataList = new List<PlayerSkillSelectData>();

            List<PlayerSkill> playerAllSkills = new List<PlayerSkill>();
            var activeSkills = DataManager.Instance.PlayerActiveSkills;
            var passiveSkills = DataManager.Instance.PlayerPassiveSkills;

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

                SelectCount--;
            }

            // 현재 보유중인 스킬은 새로 배울 스킬에서 제외
            foreach (var ownedSkill in ownedSkillsList)
            {
                playerAllSkills = playerAllSkills.Where(skill => skill.SkillName != ownedSkill.SkillName).ToList();
            }

            // 보유 스킬 중 최고등급을 달성한 스킬을 리스트에서 제외
            var MaxGradeSkills = ownedSkillsList.Where(skill => skill.Grade == Defines.PLAYER_SKILL_MAX_GRADE).ToList();
            foreach (var maxGradeSkill in MaxGradeSkills)
            {
                playerAllSkills = playerAllSkills.Where(skill => skill.SkillName != maxGradeSkill.SkillName).ToList();
            }

            // 배우지 않은 스킬 중 레벨이 1인 스킬들을 가져옴
            var learnableSkills = playerAllSkills.Where(skill => skill.Grade == 1).ToList();

            // 배울 수 있는 스킬이 충분하지 않을 경우
            var isNotEnoughSkill = learnableSkills.Count < SelectCount;
            if (isNotEnoughSkill) SelectCount = learnableSkills.Count;

            for (int i = 0; i < SelectCount; i++)
            {
                var randomIndex = Random.Range(0, learnableSkills.Count);
                var pickSkill = learnableSkills[randomIndex];
                var pickSkillSelectData =
                    new PlayerSkillSelectData(pickSkill.SkillName, pickSkill.Description, pickSkill.Index, (int)pickSkill.SkillType,
                                              false);

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
                    SelectCount--;
                }
            }

            return skillSelectDataList.ToArray();
        }

        private void ApplySkillEffect(PlayerSkill skillData)
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
                .Where(_ => isPlayerInBattle && !isDead)
                .Skip(TimeSpan.Zero)
                .Subscribe(_ => { skillEffect.Fire(stat); });
        }

        public void UpdateSkill(PlayerSkillSelectData data)
        {
            if (data.Index == -1)
            {
                return;
            }

            PlayerSkill getSkill = null;
            getSkill = data.Type == 0
                ? DataManager.Instance.PlayerActiveSkills.DataDictionary[data.Index]
                : DataManager.Instance.PlayerPassiveSkills.DataDictionary[data.Index];

            if (data.IsOwned)
            {
                var skillEffect = skillEffects[getSkill.SkillName];
                skillEffect.UpdateSkillData(getSkill);
                ownedSkillsList.Remove(ownedSkillsList.Find(skill => skill.SkillName == getSkill.SkillName));
                ownedSkillsList.Add(getSkill);
            }
            else
            {
                ApplySkillEffect(getSkill);
                ownedSkillsList.Add(getSkill);
            }

            OnUpdateSkillCompleted.Invoke();
        }

        public void GetCoolTimeReduce(float delta)
        {
            // 보유 중인 액티브 스킬 중 쿨타임이 있는 스킬들을 찾아서 쿨타임을 감소
            var ownedActiveSkills = ownedSkillsList.Where(skill => skill.SkillType == SkillType.Active).ToList();
            foreach (var ownedActiveSkill in ownedActiveSkills)
            {
                if (ownedActiveSkill is not PlayerActiveSkill activeSkill)
                    continue;

                activeSkill.SkillCoolTime -= delta;
            }

            // 이후 새로 스킬을 배울 때마다 쿨타임을 감소시키기 위해 이벤트 추가
            OnUpdateSkillCompleted.AddListener(() => CoolTimeReduce(delta));
        }
        
        public void EnableSkillSelect()
        {
            UIPresenter.Instance.OpenSkillSelectPanel(SkillSelector(Defines.PLAYER_SKILL_SELECT_COUNT));
        }

        #region Skill Effect

        private void Explosion(PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Explosion(tr, skillData);
            skillEffects.Add(skillData.SkillName, skillEffect);
            ActivateSkill(skillEffect);
        }

        private void Beam(PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Beam(tr, skillData);
            skillEffects.Add(skillData.SkillName, skillEffect);
            ActivateSkill(skillEffect);
        }

        private void Bubble(PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Bubble(tr, skillData);
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
            skillEffect.Fire(stat);
        }
        
        private void CoolTimeReduce(float delta)
        {
            // Skill을 업데이트 한 시점에는 항상 ownedSkillsList의 마지막 요소에 업데이트된 Skill이 들어가 있음
            var newSkill = ownedSkillsList.Last();

            if (newSkill is not PlayerActiveSkill activeSkill)
                return;

            // 최소 쿨타임 0.1초
            if (activeSkill.SkillCoolTime - delta < 0)
                activeSkill.SkillCoolTime = 0.1f;
            else
                activeSkill.SkillCoolTime -= delta;
        }

        #endregion
    }
}