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
    public partial class Player
    {
        private readonly List<Table_PlayerActiveSkills> ownedSkillsList = new List<Table_PlayerActiveSkills>();
        private readonly Dictionary<string, PlayerSkillEffect> skillEffects = new Dictionary<string, PlayerSkillEffect>();

        private PlayerSkillSelectData[] SkillSelector(int SelectCount)
        {
            // TODO : 새로 배울 수 있는 스킬의 수가 셀렉트 패널에 표시 할 수 있는 수보다 적을 경우
            
            var playerSkills = DataManager.Instance.PlayerActiveSkills.Values;
            var skillSelectDataList = new List<PlayerSkillSelectData>();

            var upgradeableSkills = ownedSkillsList.Where(skill => skill.Grade < Defines.PLAYER_SKILL_MAX_GRADE).ToList();

            if (upgradeableSkills.Any())
            {
                var randomOwnedSkill = upgradeableSkills[Random.Range(0, upgradeableSkills.Count)];
                var pickSkill = DataManager.Instance.PlayerActiveSkills[randomOwnedSkill.Index + 1];
                var pickSkillSelectData = new PlayerSkillSelectData(pickSkill.SkillName, "원래 있는거 업그레이드임", pickSkill.Index, true);
                skillSelectDataList.Add(pickSkillSelectData);

                // Remove owned skills from the list
                playerSkills = playerSkills.Except(ownedSkillsList).ToList();

                SelectCount--;
            }

            var newSkillList = playerSkills.Where(skill => skill.Grade == 1).ToList();
            newSkillList = newSkillList.OrderBy(x => Random.value).ToList();

            for (int i = 0; i < SelectCount; i++)
            {
                var pickSkill = newSkillList[i];
                var pickSkillSelectData =
                    new PlayerSkillSelectData(pickSkill.SkillName, "새로운거임", pickSkill.Index, false);
                skillSelectDataList.Add(pickSkillSelectData);
            }

            return skillSelectDataList.ToArray();
        }

        private void ApplySkillEffect(Table_PlayerActiveSkills skillData)
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
                    // Passive(skillData);
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

        public void UpdateSkill(PlayerSkillSelectData data)
        {
            if (data == null)
            {
                CatLog.ELog("Invalid Data !");
                return;
            }
            
            if (data.IsOwned)
            {
                var getSkill = DataManager.Instance.PlayerActiveSkills[data.Index];
                var skillEffect = skillEffects[getSkill.SkillKey];
                skillEffect.UpdateSkillData(getSkill);
            }
            else
            {
                var getSkill = DataManager.Instance.PlayerActiveSkills[data.Index];
                ApplySkillEffect(getSkill);
                ownedSkillsList.Add(getSkill);
            }
        }

        // test
        public void EnableSkillSelect()
        {
            UIPresenter.Instance.OpenSkillSelectPanel(SkillSelector(Defines.PLAYER_SKILL_SELECT_COUNT));
        }

        #region Skill Effect

        private void Explosion(Table_PlayerActiveSkills skillData)
        {
            var skillEffect = new PlayerSkillEffect_Explosion(tr, skillData);
            skillEffects.Add(skillData.SkillName, skillEffect);
            ActivateSkill(skillEffect);
        }

        private void Beam(Table_PlayerActiveSkills skillData)
        {
            var skillEffect = new PlayerSkillEffect_Beam(tr, skillData);
            skillEffects.Add(skillData.SkillName, skillEffect);
            ActivateSkill(skillEffect);
        }

        private void Bubble(Table_PlayerActiveSkills skillData)
        {
            var skillEffect = new PlayerSkillEffect_Bubble(tr, skillData);
            skillEffects.Add(skillData.SkillName, skillEffect);
            ActivateSkill(skillEffect);
        }

        #endregion
    }
}