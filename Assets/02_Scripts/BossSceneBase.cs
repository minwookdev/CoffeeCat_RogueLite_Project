using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class BossSceneBase : SceneSingleton<BossSceneBase> {
        [Title("Spawn Point Transforms")]
        [field: SerializeField] public Transform PlayerSpawnTr { get; private set; } = null;
        [field: SerializeField] public Transform ReturnPortalSpawnTr { get; private set; } = null;
        [field: SerializeField] public Transform BossSpawnTr { get; private set; } = null;

        private void Start() {
            // Positioning Portal
            var interactable = ObjectPoolManager.Inst.Spawn<InteractableTown>(InteractableType.Town.ToKey(), ReturnPortalSpawnTr.position);
            if (interactable) {
                interactable.PlayParticle();
            }
            
            // Positioning Player
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            if (!player) 
                return;
            player.Tr.position = PlayerSpawnTr.position;
            player.gameObject.SetActive(true);
        }
    }
}