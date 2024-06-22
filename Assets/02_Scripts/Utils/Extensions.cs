using System;
using UnityEngine;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat
{
    public static class Extensions
    {
        public static string ToKey(this InteractableType type)
        {
            return type switch
            {
                InteractableType.Floor  => "portal_floor",
                InteractableType.Shop   => "portal_reward",
                InteractableType.Reward => "portal_shop",
                InteractableType.None   => throw new ArgumentOutOfRangeException(nameof(type), type, null),
                _                       => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static string ToKey(this AddressablesKey key)
        {
            return key switch
            {
                AddressablesKey.NONE                             => "",
                AddressablesKey.Effect_hit_1                     => "Effect_hit_1",
                AddressablesKey.Effect_hit_2                     => "Effect_hit_2",
                AddressablesKey.Effect_hit_3                     => "Effect_hit_3",
                AddressablesKey.Skeleton_Mage_Projectile_Default => "monster_attack_fireball",
                AddressablesKey.Skeleton_Mage_Projectile_Skill   => "monster_skill_expsphere",
                AddressablesKey.Monster_Skeleton                 => "Skeleton",
                AddressablesKey.Monster_Skeleton_Warrior         => "Skeleton_Warrior",
                AddressablesKey.Monster_Skeleton_Mage            => "Skeleton_Mage",
                AddressablesKey.GroupSpawnPositions              => "GroupSpawnPositions",
                AddressablesKey.InteractableSign                 => "InteractableIcon",
                AddressablesKey.InputCanvas                      => "Canvas_Input",
                _                                                => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }

        public static string ToKey(this PlayerAddressablesKey key)
        {
            return key switch
            {
                PlayerAddressablesKey.NONE            => "",
                PlayerAddressablesKey.FlowerMagician  => "FlowerMagician",
                _                                     => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }
    }
}