using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils;
using UnityEngine;

namespace CoffeeCat.FrameWork
{
    public class SkillEffectManager : DynamicSingleton<SkillEffectManager>
    {
        #region MainSkillEffect

        private void NormalAttack(PlayerMainSkill skillData)
        {
            
        }
        
        private void Explosion(PlayerSkill skillData)
        {
            var playerTr = RogueLiteManager.Inst.SpawnedPlayer.transform;
            var skillEffect = new PlayerSkillEffect_Explosion(playerTr, skillData);
            StageManager.Inst.OnPlayerKilled.AddListener(skillEffect.OnDispose);
        }

        private void Beam(PlayerSkill skillData)
        {
            var playerTr = RogueLiteManager.Inst.SpawnedPlayer.transform;
            var skillEffect = new PlayerSkillEffect_Beam(playerTr, skillData);
            StageManager.Inst.OnPlayerKilled.AddListener(skillEffect.OnDispose);
        }

        private void Bubble(PlayerSkill skillData)
        {
            var playerTr = RogueLiteManager.Inst.SpawnedPlayer.transform;
            var skillEffect = new PlayerSkillEffect_Bubble(playerTr, skillData);
            StageManager.Inst.OnPlayerKilled.AddListener(skillEffect.OnDispose);
        }
        #endregion
        
        #region SubAttackSkillEffect

        public void Poison(PlayerSubAttackSkill subAttackSkill)
        {
            if (subAttackSkill.SkillName != "Poison")
                CatLog.WLog("Skill Name is not Poison");

            // Monster Hp Decrease
            // Sprite Color Change
        }
        
        public void Bleed(PlayerSubAttackSkill subAttackSkill)
        {
            if (subAttackSkill.SkillName != "Bleed")
                CatLog.WLog("Skill Name is not Bleed");

            // Monster Hp Decrease
            // Sprite Color Change
        }
        
        public void LifeDrain(PlayerSubAttackSkill subAttackSkill)
        {
            if (subAttackSkill.SkillName != "LifeDrain")
                CatLog.WLog("Skill Name is not LifeDrain");

            // Monster Hp Decrease
            // Player Hp Increase
        }
        
        public void KnockBack(PlayerSubAttackSkill subAttackSkill)
        {
            if (subAttackSkill.SkillName != "KnockBack")
                CatLog.WLog("Skill Name is not KnockBack");

            // Monster KnockBack
        }
        
        public void InstantKill(PlayerSubAttackSkill subAttackSkill)
        {
            if (subAttackSkill.SkillName != "InstantKill")
                CatLog.WLog("Skill Name is not InstantKill");

            // Monster InstantKill
        }
        
        public void Stun(PlayerSubAttackSkill subAttackSkill)
        {
            if (subAttackSkill.SkillName != "Stun")
                CatLog.WLog("Skill Name is not Stun");

            // Monster Stun
        }

        #endregion

        #region SubStatSkillEffect

        public void UpdateSubStatSkillEffect(PlayerMainSkill mainSkill, PlayerSubStatSkill subStatSkill)
        {
            switch (subStatSkill.SkillName)
            {
                case "SpeedUp":
                    // ???
                    break;
                case "DamageIncrease":
                    mainSkill.SkillBaseDamage += subStatSkill.Delta;
                    break;
                case "CoolTimeReduce":
                    mainSkill.SkillCoolTime -= subStatSkill.Delta;
                    break;
            }
        }

        #endregion
        
    }
}