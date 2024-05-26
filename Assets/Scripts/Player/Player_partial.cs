using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    // Skill
    public partial class Player
    {
        [Title("Skill")]
        [ShowInInspector] private List<string> ownedSkillsList = new List<string>();

        private PlayerSkillSelectData[] skillSelectDatas = null;
        private List<PlayerSkillEffect> skillEffects = null;

        private void GenerateSkillEffect(Table_PlayerSkills skillData)
        {
            switch (skillData.SkillName)
            {
                case "Explosion":
                    var skillEffect = new PlayerSkill_Explosion(skillData);
                    skillEffects.Add(skillEffect);
                    ActivateSkill(skillEffect);
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
                .Subscribe(_ => { skillEffect.Fire(tr, status); });
        }

        private void SetSelectSkillData()
        {
            // TODO
            skillSelectDatas = new PlayerSkillSelectData[Defines.PLAYER_SKILL_SELECT_COUNT];
            var skillData = StageManager.Instance.PlayerSkills[1];

            var skill = new PlayerSkillSelectData(skillData.SkillName, "이걸 선택해", skillData.Index);
            var dummySkill = new PlayerSkillSelectData("화난 더미", "난 인형 안에 사는 유령이다!", -1);
            var dummySkill2 = new PlayerSkillSelectData("냥냥 펀치", "가벼워서 아프지는 않지만 기분이 나쁘다.", -1);
            
            skillSelectDatas[0] = skill;
            skillSelectDatas[1] = dummySkill;
            skillSelectDatas[2] = dummySkill2;
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
            skillEffects = new List<PlayerSkillEffect>();
            SetSelectSkillData();
            UIPresenter.Instance.OpenSkillSelectPanel(skillSelectDatas);
        }
    }
}