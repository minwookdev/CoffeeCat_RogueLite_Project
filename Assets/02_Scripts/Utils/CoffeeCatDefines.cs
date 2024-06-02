using System;
using UnityEngine;

namespace CoffeeCat.Utils.Defines
{
    public static class Defines
    {
        public const int PLAYER_SKILL_SELECT_COUNT = 3;
        public const int PLAYER_SKILL_MAX_GRADE = 3;
        public const float PLAYER_AREA_SKILL_VECTOR_X = 15;
        public const float PLAYER_AREA_SKILL_VECTOR_Y = 8;
        public const string PLAYER_ENHANCE_DATA_KEY = "PlayerEnhanceData";
        
        // Encrypt/Decrypt
        public static readonly string ENC_KEY = "m71a12x28";

        public static LayerMask GetPlayerLayer() {
            return LayerMask.NameToLayer("Player");
        }
    }

    public enum SceneName
    {
        NONE,
        LoadingScene,
        DungeonScene,
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
        InteractableSign,
    }

    public enum PlayerAddressablesKey
    {
        NONE,
        NormalAttack_01 = 1,
        FlowerMagician,
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

    public enum InteractableType
    {
        None,
        Floor,
        Shop,
        Reward
    }
}