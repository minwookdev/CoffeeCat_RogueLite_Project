using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;

namespace RandomDungeonWithBluePrint
{
    [CreateAssetMenu(menuName = "CoffeeCat/Scriptable Object/BlurPrintQueue")]
    public class BluePrintQueue : ScriptableObject
    {
        [Title("BluePrint Queue Options", TitleAlignment = TitleAlignments.Centered)]
        [field: SerializeField] public FieldBluePrint[] NormalMapBluePrints { get; private set; } = null;
        [field: SerializeField] public FieldBluePrint[] HiddenMapBluePrints { get; private set; } = null;
        [field: SerializeField] public SceneName EventMapSceneKey { get; private set; } = SceneName.NONE;
        [field: SerializeField] public SceneName BossMapSceneKey { get; private set; } = SceneName.NONE;
        [field: SerializeField] public bool IsGrantSkillOnStart { get; private set; } = true;
    }
}