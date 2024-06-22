using System;
using UnityEngine;

namespace CoffeeCat.Utils.Defines
{
    public static class Defines {
        public const int SPAWN_MONSTER_MAX_COUNT = 30;
        public const int PLAYER_SKILL_SELECT_COUNT = 3;
        public const int PLAYER_SKILL_MAX_GRADE = 3;
        public const float PLAYER_AREA_SKILL_VECTOR_X = 10;
        public const float PLAYER_AREA_SKILL_VECTOR_Y = 7;
        public const string PLAYER_ENHANCE_DATA_KEY = "PlayerEnhanceData";
        public const string TAG_UICAM = "UICamera";
        
        // Encrypt/Decrypt
        public static readonly string ENC_KEY = "m71a12x28";

        public static LayerMask GetPlayerLayer() {
            return LayerMask.NameToLayer("Player");
        }
    }
    
    #region Enumerations

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
        InputCanvas,
    }

    public enum PlayerAddressablesKey
    {
        NONE,
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
        Reward,
        Boss,
    }
    
    #endregion
}