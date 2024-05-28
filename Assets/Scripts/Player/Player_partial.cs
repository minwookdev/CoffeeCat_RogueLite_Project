using System;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UniRx;

namespace CoffeeCat
{
    // Skill
    public partial class Player
    {
        [Title("Skill")]
        [ShowInInspector] private List<string> ownedSkillsList = new List<string>();

        private PlayerSkillSelectData[] skillSelectDatas = null;

        private void GenerateSkillEffect(Table_PlayerSkills skillData)
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

        // TODO
        private void SetSelectSkillData()
        {
            skillSelectDatas = new PlayerSkillSelectData[Defines.PLAYER_SKILL_SELECT_COUNT];
            var explosion = StageManager.Instance.PlayerSkills[(int)PlayerSkillsKey.Explosion_1];
            var beam = StageManager.Instance.PlayerSkills[(int)PlayerSkillsKey.Beam_1];
            var bubble = StageManager.Instance.PlayerSkills[(int)PlayerSkillsKey.Bubble_1];

            var skill = new PlayerSkillSelectData(explosion.SkillName, "다중 몬스터 단일 공격", explosion.Index);
            var skill2 = new PlayerSkillSelectData(beam.SkillName, "하나만 조짐", beam.Index);
            var skill3 = new PlayerSkillSelectData(bubble.SkillName, "전체 공격", bubble.Index);

            skillSelectDatas[0] = skill;
            skillSelectDatas[1] = skill2;
            skillSelectDatas[2] = skill3;
        }

        public void UpdateSkill(int index)
        {
            // -1 index is Invalid
            if (index == -1)
            {
                return;
            }

            // 새로운 스킬 Get
            var getSkill = StageManager.Instance.PlayerSkills[index];
            ownedSkillsList.Add(getSkill.SkillName);
            GenerateSkillEffect(getSkill);

            // TODO : 선택한 스킬이 원래 가지고 있던 스킬의 다음 등급일 경우
        }

        // test
        private void EnableSkillSelect()
        {
            SetSelectSkillData();
            UIPresenter.Instance.OpenSkillSelectPanel(skillSelectDatas);
        }

        #region Gernerate Skill Effect

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