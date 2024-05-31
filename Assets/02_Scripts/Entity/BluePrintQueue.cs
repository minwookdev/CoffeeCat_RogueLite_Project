using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RandomDungeonWithBluePrint
{
    [CreateAssetMenu(menuName = "CoffeeCat/Scriptable Object/BlurPrintQueue")]
    public class BluePrintQueue : ScriptableObject
    {
        [Title("BluePrint Queue Options", TitleAlignment = TitleAlignments.Centered)]
        [field: SerializeField] public FieldBluePrint[] NormalMapBluePrints { get; private set; } = null;
        [field: SerializeField] public FieldBluePrint[] HiddenMapBluePrints { get; private set; } = null;
        [field: SerializeField] public string EventMapSceneKey { get; private set; } = "";
        [field: SerializeField] public string BossMapSceneKey { get; private set; } = "";
        [field: SerializeField] public bool IsGrantSkillOnStart { get; private set; } = true;
    }
}