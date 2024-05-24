using System;

namespace CoffeeCat.Utils.Defines
{
    public static class Defines 
    {
        public const int PLAYER_SKILL_SELECT_COUNT = 3;
        
        public static string ToStringEx(this AddressablesKey key)
        {
            return key switch
            {
                AddressablesKey.NONE => "",
                AddressablesKey.Effect_hit_1 => "Effect_hit_1",
                AddressablesKey.Effect_hit_2 => "Effect_hit_2",
                AddressablesKey.Effect_hit_3 => "Effect_hit_3",
                AddressablesKey.Skeleton_Mage_Projectile_Default => "monster_attack_fireball",
                AddressablesKey.Skeleton_Mage_Projectile_Skill => "monster_skill_expsphere",
                AddressablesKey.Monster_Skeleton => "Skeleton",
                AddressablesKey.Monster_Skeleton_Warrior => "Skeleton_Warrior",
                AddressablesKey.Monster_Skeleton_Mage => "Skeleton_Mage",
                AddressablesKey.GroupSpawnPositions => "GroupSpawnPositions",
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }

        public static string ToStringEx(this PlayerAddressablesKey key)
        {
            return key switch
            {
                PlayerAddressablesKey.NONE => "",
                PlayerAddressablesKey.PlayerAttack_01_Pink => "PlayerAttack_01_Pink",
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }
    }

    public static class Constants
    {
    }

    public enum SceneName
    {
        NONE,
        LoadingScene,
        MonsterSampleScene,
    }

    public enum AddressablesKey
    {
        NONE,
        Effect_hit_1,
        Effect_hit_2,
        Effect_hit_3,
        Monster_Skeleton,
        Monster_Skeleton_Warrior,
        Monster_Skeleton_Mage,
        Skeleton_Mage_Projectile_Default,
        Skeleton_Mage_Projectile_Skill,
        GroupSpawnPositions,
    }

    public enum PlayerAddressablesKey
    {
        NONE,
        PlayerAttack_01_Pink,
    }

    public enum PlayerStatusKey
    {
        NONE,
        FlowerMagician = 1,
    }

    public enum PlayerSkillsKey
    {
        NONE,
        Explosion = 1,
        Explosion_2,
        Explosion_3,
    }

    public enum SkillType
    {
        NONE,
        Explosion,
    }

    public enum Layer
    {
        NONE,
        PLAYER,
    }

    public enum Tag
    {
        NONE,
    }

    public enum ProjectileKey
    {
        NONE,
        monster_attack_fireball,
    }

    public enum ProjectileSkillKey
    {
        NONE,
        monster_skill_expsphere,
    }
}