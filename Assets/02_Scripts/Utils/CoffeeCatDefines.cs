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

        public static LayerMask GetPlayerLayer() {
            return LayerMask.NameToLayer("Player");
        }
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
        Explosion_1 = 1,
        Explosion_2,
        Explosion_3,
        Beam_1,
        Beam_2,
        Beam_3,
        Bubble_1,
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

    public enum InteractableType
    {
        None,
        Floor,
        Shop,
        Reward
    }
}