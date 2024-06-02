using System;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerSkillDatas
    {
    }

    [Serializable]
    public class PlayerSkill
    {
        public int Index = default;
        public string SkillName = string.Empty;
        public int Grade = default;
        public SkillType SkillType = default;
    }
    
    public enum SkillType
    {
        Active = 0,
        Passive = 1,
    }
}