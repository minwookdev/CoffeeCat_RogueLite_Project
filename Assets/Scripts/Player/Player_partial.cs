using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UniRx;
using Random = UnityEngine.Random;

namespace CoffeeCat
{
    // Skill
    public partial class Player
    {
        // SkillIndex(SKillKey?)를 key로 가진 SKillEffect 딕셔너리?
        [Title("Skill")]
        [ShowInInspector, ReadOnly] private List<Table_PlayerSkills> ownedSkillsList = new List<Table_PlayerSkills>();

        private void ApplySkillEffect(Table_PlayerSkills skillData)
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
                default:
                    break;
            }
        }

        private void ActivateSkill(PlayerSkillEffect skillEffect)
        {
            this.ObserveEveryValueChanged(_ => isPlayerInBattle)
                .Where(_ => isPlayerInBattle)
                .Where(_ => !isDead)
                .Skip(TimeSpan.Zero)
                .Subscribe(_ => { skillEffect.Fire(status); });
        }

        private bool CanUpgradeSkill()
        {
            // 보유스킬이 1개 이상일 경우
            if (ownedSkillsList.Count >= 1)
            {
                foreach (var ownedSkill in ownedSkillsList)
                {
                    // 최대등급이 아닌 스킬이 1개라도 있을 경우
                    if (ownedSkill.Grade < Defines.PLAYER_SKILL_MAX_GRADE)
                        return true;
                }

                return false;
            }

            return false;
        }

        // TODO
        private PlayerSkillSelectData[] SkillSelector(int SelectCount)
        {
            var playerSkills = StageManager.Instance.PlayerSkills.Values;
            var skillSelectDatas = new PlayerSkillSelectData[SelectCount];
            var skillSelectDataIndex = 0;
            var newSkillCount = SelectCount;

            if (CanUpgradeSkill())
            {
                var randomOwnedSkill = ownedSkillsList[Random.Range(0, ownedSkillsList.Count)];

                // 업그레이드 가능한 스킬일 때까지 다시 뽑기
                while (randomOwnedSkill.Grade == Defines.PLAYER_SKILL_MAX_GRADE)
                {
                    randomOwnedSkill = ownedSkillsList[Random.Range(0, ownedSkillsList.Count)];
                }

                var pickSkill = StageManager.Instance.PlayerSkills[randomOwnedSkill.Index + 1];
                var pickSkillSelectData =
                    new PlayerSkillSelectData(pickSkill.SkillName, "원래 있는거 업그레이드임", pickSkill.Index, true);
                skillSelectDatas[skillSelectDataIndex] = pickSkillSelectData;
                skillSelectDataIndex++;

                // 스킬 리스트에서 보유중인 스킬 제거
                for (int i = 0; i < ownedSkillsList.Count; i++)
                {
                    playerSkills = playerSkills.Where(skill => skill.SkillKey != ownedSkillsList[i].SkillKey).ToList();
                }

                newSkillCount -= 1;
            }

            var newSkillList = playerSkills.Where(skill => skill.Grade == 1).ToList();

            for (int i = 0; i < newSkillCount; i++)
            {
                var pickSkill = newSkillList[Random.Range(0, newSkillList.Count)];
                newSkillList.Remove(pickSkill);
                var pickSkillSelectData =
                    new PlayerSkillSelectData(pickSkill.SkillName, "새로운거임", pickSkill.Index, false);
                skillSelectDatas[skillSelectDataIndex] = pickSkillSelectData;
                skillSelectDataIndex++;
            }

            return skillSelectDatas;
        }

        public void UpdateSkill(PlayerSkillSelectData data)
        {
            if (data == null)
            {
                CatLog.ELog("Invalid Data !");
                return;
            }

            if (data.isOwned)
            {
                // 보유중인 스킬 Upgrade
                var getSkill = StageManager.Instance.PlayerSkills[data.Index];
                // SkillEffect.UpdateSkillData(getSkill); - 이펙트 업데이트
            }
            else
            {
                var getSkill = StageManager.Instance.PlayerSkills[data.Index];
                ApplySkillEffect(getSkill);
                ownedSkillsList.Add(getSkill);
            }
        }

        // test
        private void EnableSkillSelect()
        {
            UIPresenter.Instance.OpenSkillSelectPanel(SkillSelector(Defines.PLAYER_SKILL_SELECT_COUNT));
        }

        #region Apliy Skill Effect

        private void Explosion(Table_PlayerSkills skillData)
        {
            var skillEffect = new PlayerSkill_Explosion(tr, skillData);
            ActivateSkill(skillEffect);
        }

        private void Beam(Table_PlayerSkills skillData)
        {
            var skillEffect = new PlayerSkill_Beam(tr, skillData);
            ActivateSkill(skillEffect);
        }

        private void Bubble(Table_PlayerSkills skillData)
        {
            var skillEffect = new PlayerSkill_Bubble(tr, skillData);
            ActivateSkill(skillEffect);
        }

        #endregion
    }
}