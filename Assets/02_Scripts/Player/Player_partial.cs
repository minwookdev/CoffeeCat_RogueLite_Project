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
        private readonly Dictionary<string, PlayerSkillEffect> skillEffects =
            new Dictionary<string, PlayerSkillEffect>();

        private readonly UnityEvent OnUpdateSkillCompleted = new UnityEvent();

        private PlayerSkillSelectData[] SkillSelector(int SelectCount)
        {
            // TODO : 새로 배울 수 있는 스킬의 수가 셀렉트 패널에 표시 할 수 있는 수보다 적을 경우
            // TODO : 가지고 있던 스킬을 업그레이드 시켰을 경우 발사체 등에 skillData가 반영되었는지 확인하기
            // TODO : 스킬 Description 시트에 추가

            List<PlayerSkill> playerAllSkills = new List<PlayerSkill>();
            var activeSkills = DataManager.Instance.PlayerActiveSkills;
            var passiveSkills = DataManager.Instance.PlayerPassiveSkills;

            playerAllSkills.AddRange(activeSkills.DataDictionary.Values);
            playerAllSkills.AddRange(passiveSkills.DataDictionary.Values);

            var skillSelectDataList = new List<PlayerSkillSelectData>();

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

                var pickSkillSelectData = new PlayerSkillSelectData(pickSkill.SkillName, "원래 있는거 업그레이드임",
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
            var randomIndex = Random.Range(0, learnableSkills.Count);

            for (int i = 0; i < SelectCount; i++)
            {
                var pickSkill = learnableSkills[randomIndex];
                var pickSkillSelectData =
                    new PlayerSkillSelectData(pickSkill.SkillName, "새로운거임", pickSkill.Index, (int)pickSkill.SkillType,
                                              false);

                learnableSkills.RemoveAt(randomIndex);
                skillSelectDataList.Add(pickSkillSelectData);
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
            if (data == null)
            {
                CatLog.ELog("Invalid Data !");
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

        // test
        public void EnableSkillSelect()
        {
            UIPresenter.Instance.OpenSkillSelectPanel(SkillSelector(Defines.PLAYER_SKILL_SELECT_COUNT));
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

        private void CoolTimeReduce(float delta)
        {
            // Skill을 업데이트 한 시점에는 항상 ownedSkillsList의 마지막 요소에 업데이트된 Skill이 들어가 있음
            var newSkill = ownedSkillsList.Last();

            if (newSkill is not PlayerActiveSkill activeSkill)
                return;

            activeSkill.SkillCoolTime -= delta;
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

        #endregion
    }
}