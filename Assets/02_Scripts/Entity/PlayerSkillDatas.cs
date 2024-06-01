using System;
using CoffeeCat.Utils.SerializedDictionaries;
using Sirenix.OdinInspector;

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
    }
}