using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class TownSceneManager : SceneSingleton<TownSceneManager> {
        [field: SerializeField] public Transform PlayerSpawnTr { get; private set; } = null;
        [field: SerializeField] public Transform DungeonEntranceTr { get; private set; } = null;
        [field: SerializeField] public Transform StatEnhancementTr { get; private set; } = null;

        private void Start() {
            // Spawn New Player
            ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(PlayerAddressablesKey.FlowerMagician.ToKey(), false, (player) => {
                var playerObj = Instantiate(player, PlayerSpawnTr.position, Quaternion.identity);
            });
        }
    }
}
