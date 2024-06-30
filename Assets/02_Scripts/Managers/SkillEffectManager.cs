using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils;
using UnityEngine;

namespace CoffeeCat.FrameWork
{
    public enum SkillType
    {
        Main,
        SubAttack,
        SubStat
    }
    
    public class SkillEffectManager : DynamicSingleton<SkillEffectManager>
    {
        #region MainSkillEffect

        public PlayerSkillEffect InstantiateMainSkillEffect(PlayerMainSkill skillData)
        {
            PlayerSkillEffect skillEffect = skillData.SkillName switch
            {
                "NormalAttack" => NormalAttack(skillData),
                "Explosion"    => Explosion(skillData),
                "Beam"         => Beam(skillData),
                "Bubble"       => Bubble(skillData),
                _              => null
            };

            return skillEffect;
        }

        private PlayerSkillEffect NormalAttack(PlayerMainSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            var skillEffect = new PlayerSkillEffect_NormalAttack(player.Tr, skillData.SkillName, player);
            return skillEffect;
        }
        
        private PlayerSkillEffect Explosion(PlayerMainSkill skillData)
        {
            var playerTr = RogueLiteManager.Inst.SpawnedPlayer.transform;
            var skillEffect = new PlayerSkillEffect_Explosion(playerTr, skillData.SkillName);
            return skillEffect;
        }

        private PlayerSkillEffect Beam(PlayerMainSkill skillData)
        {
            var playerTr = RogueLiteManager.Inst.SpawnedPlayer.transform;
            var skillEffect = new PlayerSkillEffect_Beam(playerTr, skillData.SkillName);
            return skillEffect;
        }

        private PlayerSkillEffect Bubble(PlayerMainSkill skillData)
        {
            var playerTr = RogueLiteManager.Inst.SpawnedPlayer.transform;
            var skillEffect = new PlayerSkillEffect_Bubble(playerTr, skillData.SkillName);
            return skillEffect;
        }
        #endregion
        
        #region SubAttackSkillEffect

        public void SubAttackSkillEffect(PlayerSubAttackSkill subAttackSkill)
        {
            switch (subAttackSkill.SkillName)
            {
                case "Poison":
                    Poison(subAttackSkill);
                    break;
                case "Bleed":
                    Bleed(subAttackSkill);
                    break;
                case "LifeDrain":
                    LifeDrain(subAttackSkill);
                    break;
                case "KnockBack":
                    KnockBack(subAttackSkill);
                    break;
                case "InstantKill":
                    InstantKill(subAttackSkill);
                    break;
                case "Stun":
                    Stun(subAttackSkill);
                    break;
            }
        }
        
        public void Poison(PlayerSubAttackSkill subAttackSkill)
        {
            
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
                    mainSkill.SkillCoolTime *= subStatSkill.Delta;
                    break;
            }
        }

        #endregion
        
    }
}