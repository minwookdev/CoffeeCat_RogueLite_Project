using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class TownSceneManager : SceneSingleton<TownSceneManager> {
        [field: SerializeField] public Transform PlayerSpawnTr { get; private set; } = null;
        [field: SerializeField] public Transform DungeonEntranceTr { get; private set; } = null;
        [field: SerializeField] public Transform StatEnhancementTr { get; private set; } = null;
        
        protected override void Initialize()
        {
            base.Initialize();
            DataManager.Inst.Create();
            InputManager.Inst.Create();
        }

        private void Start() {
            // Spawn New Player
            ResourceManager.Inst.AddressablesAsyncLoad<GameObject>("Player_Town", false, (player) => {
                var playerObj = Instantiate(player, PlayerSpawnTr.position, Quaternion.identity);
            });
        }
    }
}
