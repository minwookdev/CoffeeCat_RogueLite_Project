using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Rito;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public partial class Player
    {
        [Title("Skill")]
        [ShowInInspector]
        private readonly Dictionary<string, PlayerSkillEffect> skillEffects =
            new Dictionary<string, PlayerSkillEffect>();

        [ShowInInspector] private List<PlayerSkillSet> skillSets = new List<PlayerSkillSet>();

        /*
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
        */

        private PlayerSkillSelectData[] SkillSelector()
        {
            var skillTypePicker = new WeightedRandomPicker<SkillType>();
            skillTypePicker.Add
                (
                 (SkillType.Main, 5),
                 (SkillType.SubAttack, 10),
                 (SkillType.SubStat, 20)
                );

            var selectData = new PlayerSkillSelectData[Defines.PLAYER_SKILL_SELECT_COUNT];
            var mainSkills =
                DataManager.Inst.PlayerMainSkills.DataDictionary.Values.Where(skill => skill.SkillLevel == 1).ToList();
            var subAttackSkills =
                DataManager.Inst.PlayerSubAttackSkills.DataDictionary
                           .Values.Where(skill => skill.SkillLevel == 1).Where(skill => skill.Index != 0).ToList();
            var subStatSkills =
                DataManager.Inst.PlayerSubStatSkills.DataDictionary
                           .Values.Where(skill => skill.SkillLevel == 1).Where(skill => skill.Index != 0).ToList();

            // Main 스킬이 기본공격밖에 없다면 Main 1개 확정 선택
            if (skillSets.Count == 1)
            {
                // 기본 공격 제외 메인 스킬 중 선택
                var mainSkill = mainSkills.Where(skill => skill.SkillName != "NormalAttack")
                                          .OrderBy(_ => Random.value).FirstOrDefault();

                if (mainSkill == null) CatLog.WLog("Picked Main Skill is null");
                else
                {
                    selectData[Random.Range(0, selectData.Length)] = new PlayerSkillSelectData(mainSkill.SkillName,
                             mainSkill.Description, mainSkill.Index, (int)SkillType.Main);
                    mainSkills.Remove(mainSkill);
                }
            }

            // 레벨이 최대치인 메인 스킬이 있다면 제외
            foreach (var skillSet in skillSets)
            {
                if (skillSet.IsMaxLevelMainSkill())
                {
                    mainSkills = mainSkills.Where(skill => skill.SkillName != skillSet.GetMainSkill()).ToList();
                }
            }

            // 모든 메인 스킬이 최대 레벨인 경우 Main 제외
            if (mainSkills.Count == 0)
            {
                skillTypePicker.Remove(SkillType.Main);
            }

            // 비어있는 SubAttack 스킬 슬롯이 있는지 검사
            var existEmptySlot = false;
            foreach (var skillSet in skillSets)
            {
                if (skillSet.IsEmptySubAttackSkill())
                    existEmptySlot = true;
            }

            // subAttackSkill 슬롯 중 빈 슬롯이 없을 경우
            List<PlayerSubAttackSkill> pickableSubAttackSkills = new List<PlayerSubAttackSkill>();
            if (!existEmptySlot)
            {
                foreach (var skillSet in skillSets)
                {
                    if (!skillSet.IsMaxLevelSubAttackSkill())
                        // 스킬 레벨이 최대치가 아니라면 목록에 추가
                        pickableSubAttackSkills.AddRange(subAttackSkills.Where
                                                             (skill => skill.SkillName == skillSet.GetSubAttackSkill()));
                }
            }
            else
            {
                // 빈 슬롯이 있는 경우
                pickableSubAttackSkills.AddRange(subAttackSkills);
            }

            // 빈 슬롯이 없고, 최대레벨이 아닌 스킬조차 없는 경우 SubAttack 제외
            if (pickableSubAttackSkills.Count == 0)
            {
                skillTypePicker.Remove(SkillType.SubAttack);
            }

            // 비어있는 SubStat 스킬 슬롯이 있는지 검사
            existEmptySlot = false;
            foreach (var skillSet in skillSets)
            {
                if (skillSet.IsEmptySubStatSkill())
                    existEmptySlot = true;
            }

            // subStatSkill 슬롯 중 빈 슬롯이 없을 경우
            List<PlayerSubStatSkill> pickableSubStatSkills = new List<PlayerSubStatSkill>();
            if (!existEmptySlot)
            {
                foreach (var skillSet in skillSets)
                {
                    if (skillSet.IsMaxLevelSubStatSkill())
                        // 스킬 레벨이 최대치가 아니라면 목록에 추가
                        pickableSubStatSkills.AddRange(subStatSkills.Where
                                                           (skill => skill.SkillName == skillSet.GetSubStatSkill_1()));
                }
            }
            else
            {
                // 빈 슬롯이 있는 경우
                pickableSubStatSkills.AddRange(subStatSkills);
            }

            // 빈 슬롯이 없고, 최대레벨이 아닌 스킬조차 없는 경우 SubStat 제외
            if (pickableSubStatSkills.Count == 0)
            {
                skillTypePicker.Remove(SkillType.SubStat);
            }

            for (int i = 0; i < selectData.Length; i++)
            {
                if (selectData[i] == null)
                {
                    var pickedType = skillTypePicker.GetRandomPick();
                    switch (pickedType)
                    {
                        case SkillType.Main:
                            var mainSkill = mainSkills.OrderBy(_ => Random.value).FirstOrDefault();
                            if (mainSkill == null) CatLog.WLog("Picked Main Skill is null");
                            else
                            {
                                selectData[i] = new PlayerSkillSelectData(mainSkill.SkillName, mainSkill.Description,
                                                                          mainSkill.Index, (int)SkillType.Main);
                                mainSkills.Remove(mainSkill);
                            }

                            break;
                        case SkillType.SubAttack:
                            var subAttackSkill = pickableSubAttackSkills.OrderBy(_ => Random.value).FirstOrDefault();
                            if (subAttackSkill == null) CatLog.WLog("Picked SubAttack Skill is null");
                            else
                            {
                                selectData[i] = new PlayerSkillSelectData(subAttackSkill.SkillName,
                                                                          subAttackSkill.Description,
                                                                          subAttackSkill.Index,
                                                                          (int)SkillType.SubAttack);
                                subAttackSkills.Remove(subAttackSkill);
                            }

                            break;
                        case SkillType.SubStat:
                            var subStatSkill = pickableSubStatSkills.OrderBy(_ => Random.value).FirstOrDefault();
                            if (subStatSkill == null) CatLog.WLog("Picked SubStat Skill is null");
                            else
                            {
                                selectData[i] = new PlayerSkillSelectData(subStatSkill.SkillName,
                                                                          subStatSkill.Description,
                                                                          subStatSkill.Index, (int)SkillType.SubStat);
                                subStatSkills.Remove(subStatSkill);
                            }

                            break;
                    }
                }
            }

            return selectData;
        }

        private void ActivateSkill(PlayerSkillSet skillSet)
        {
            this.ObserveEveryValueChanged(_ => isPlayerInBattle)
                .Where(_ => isPlayerInBattle)
                .Subscribe(_ => { skillSet.MainSkillEffect(stat); });
        }

        public void UpdateSkill(PlayerSkillSelectData data)
        {
            // 메인 스킬일 때 : 기존의 스킬인지 새로운 스킬인지 검사
            // 기존 스킬이라면 레벨업, 새로운 스킬이라면 추가
            // 서브 스킬일 때 : 슬롯 선택 UI 띄워서 선택하게 하기 > skillsetIndex 받기
            

            /*
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
            */
        }

        public void EnableSkillSelect()
        {
            UIPresenter.Inst.OpenSkillSelectPanel(SkillSelector());
        }
    }
}