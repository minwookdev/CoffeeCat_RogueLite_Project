using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils.Defines;
using UnityEngine;

namespace CoffeeCat
{
    public static class SkillEffectConverter
    {
        public static PlayerSkillEffect ConvertSkillEffect(PlayerSkillEffect skillEffect)
        {
            switch (skillEffect.SkillData.SkillName)
            {
                case "Explosion":
                    return skillEffect as PlayerSkill_Explosion;
                default:
                    return null;
            }
        }
    }
}