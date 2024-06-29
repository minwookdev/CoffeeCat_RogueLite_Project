using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;

namespace CoffeeCat {
    public class BossSceneBase : SceneSingleton<BossSceneBase> {
        [Title("Spawn Point Transforms")]
        [field: SerializeField] public Transform PlayerSpawnTr { get; private set; } = null;
        [field: SerializeField] public Transform ReturnPortalSpawnTr { get; private set; } = null;
        [field: SerializeField] public Transform BossSpawnTr { get; private set; } = null;
    }
}