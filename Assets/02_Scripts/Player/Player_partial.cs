using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Rito;
using Sirenix.OdinInspector;
using UniRx;
using Random = UnityEngine.Random;

namespace CoffeeCat
{
    public partial class Player_Dungeon
    {
        [Title("Skill")]
        [ShowInInspector] private List<PlayerSkillSet> skillSets = new List<PlayerSkillSet>();

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
                             mainSkill.Description, mainSkill.Index, SkillType.Main);
                    mainSkills.Remove(mainSkill);
                }
            }

            // 레벨이 최대치인 메인 스킬이 있다면 제외
            foreach (var skillSet in skillSets)
            {
                if (skillSet.IsMaxLevelMainSkill())
                {
                    mainSkills = mainSkills.Where(skill => skill.SkillName != skillSet.MainSkillData.SkillName)
                                           .ToList();
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
                        pickableSubAttackSkills.AddRange
                            (subAttackSkills.Where(skill => skill.SkillName == skillSet.SubAttackSkill.SkillName));
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
                                                           (skill => skill.SkillName ==
                                                                     skillSet.SubStatSkill_1.SkillName));
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
                                                                          SkillType.SubAttack);
                                pickableSubAttackSkills.Remove(subAttackSkill);
                            }

                            break;
                        case SkillType.SubStat:
                            var subStatSkill = pickableSubStatSkills.OrderBy(_ => Random.value).FirstOrDefault();
                            if (subStatSkill == null) CatLog.WLog("Picked SubStat Skill is null");
                            else
                            {
                                selectData[i] = new PlayerSkillSelectData(subStatSkill.SkillName,
                                                                          subStatSkill.Description,
                                                                          subStatSkill.Index, SkillType.SubStat);
                                pickableSubStatSkills.Remove(subStatSkill);
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

        private void GetNewMainSkill(PlayerMainSkill skill)
        {
            var skillSet = new PlayerSkillSet(skill, skillSets.Count);
            ActivateSkill(skillSet);
            skillSets.Add(skillSet);
        }

        private void GetOwnedMainSkill(int skillSetIndex, PlayerMainSkill skill)
        {
            var skillSet = skillSets[skillSetIndex];
            skillSet.UpdateMainSkill(skill);
            ActivateSkill(skillSet);
        }

        public void SelectedSkill(PlayerSkillSelectData data)
        {
            // 메인 스킬일 때 : 기존의 스킬인지 새로운 스킬인지 검사
            // 기존 스킬이라면 레벨업, 새로운 스킬이라면 추가
            // 서브 스킬일 때 : 슬롯 선택 UI 띄워서 선택하게 하기 > skillsetIndex 받기

            switch (data.Type)
            {
                case SkillType.Main:
                    UpdateMainSkill();
                    break;
                default:
                    DungeonUIPresenter.Inst.SkillsPanelOpenForSkillSelect(data);
                    break;
            }

            void UpdateMainSkill()
            {
                var ownedSkillSetIndex = -1;
                foreach (var skillSet in skillSets)
                {
                    if (data.Name == skillSet.MainSkillData.SkillName)
                        ownedSkillSetIndex = skillSet.SkillSetIndex;
                }

                if (ownedSkillSetIndex != -1)
                {
                    var skillDataIndex = skillSets[ownedSkillSetIndex].MainSkillData.Index + 1;
                    var newSkill = DataManager.Inst.PlayerMainSkills.DataDictionary[skillDataIndex];
                    GetOwnedMainSkill(ownedSkillSetIndex, newSkill);
                }
                else
                {
                    var newSkill = DataManager.Inst.PlayerMainSkills.DataDictionary[data.Index];
                    GetNewMainSkill(newSkill);
                }

                DungeonUIPresenter.Inst.RefreshPlayerSkillsPanel(skillSets);
                StageManager.Inst.InvokeEventSkillSelectCompleted();
            }
        }

        public bool CheckSelectableSlot(int SkillSetIndex, PlayerSkillSelectData data)
        {
            switch (data.Type)
            {
                case SkillType.SubAttack:
                    if (skillSets[SkillSetIndex].IsEmptySubAttackSkill())
                    {
                        return true;
                    }
                    else
                    {
                        if (skillSets[SkillSetIndex].SubAttackSkill.SkillName == data.Name)
                        {
                            if (skillSets[SkillSetIndex].IsMaxLevelSubAttackSkill())
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                case SkillType.SubStat:
                    if (skillSets[SkillSetIndex].IsEmptySubStatSkill())
                    {
                        return true;
                    }
                    else
                    {
                        if (skillSets[SkillSetIndex].SubStatSkill_1.SkillName == data.Name)
                        {
                            if (skillSets[SkillSetIndex].IsMaxLevelSubStatSkill())
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public void UpdateSubSkill(int SkillSetIndex, PlayerSkillSelectData data)
        {
            switch (data.Type)
            {
                case SkillType.SubAttack:
                    if (skillSets[SkillSetIndex].IsEmptySubAttackSkill())
                    {
                        var newSkill = DataManager.Inst.PlayerSubAttackSkills.DataDictionary[data.Index];
                        skillSets[SkillSetIndex].UpdateSubAttackSkill(newSkill);
                    }
                    else
                    {
                        var newSkillIndex = skillSets[SkillSetIndex].SubAttackSkill.Index + 1;
                        var newSkill = DataManager.Inst.PlayerSubAttackSkills.DataDictionary[newSkillIndex];
                        skillSets[SkillSetIndex].UpdateSubAttackSkill(newSkill);
                    }

                    break;
                case SkillType.SubStat:
                    if (skillSets[SkillSetIndex].IsEmptySubStatSkill())
                    {
                        var newSkill = DataManager.Inst.PlayerSubStatSkills.DataDictionary[data.Index];
                        skillSets[SkillSetIndex].UpdateSubStatSkill(newSkill, 1);
                    }
                    else
                    {
                        var newSkillIndex = skillSets[SkillSetIndex].SubStatSkill_1.Index + 1;
                        var newSkill = DataManager.Inst.PlayerSubStatSkills.DataDictionary[newSkillIndex];
                        skillSets[SkillSetIndex].UpdateSubStatSkill(newSkill, 1);
                    }

                    break;
            }

            StageManager.Inst.InvokeEventSkillSelectCompleted();
            DungeonUIPresenter.Inst.RefreshPlayerSkillsPanel(skillSets);
        }

        public void EnableSkillSelect()
        {
            DungeonUIPresenter.Inst.OpenSkillSelectPanel(SkillSelector());
        }
    }
}