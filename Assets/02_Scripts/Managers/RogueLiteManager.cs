/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION: 
using CoffeeCat.Utils;
using UnityEngine;

namespace CoffeeCat.FrameWork {
	public class RogueLiteManager : GenericSingleton<RogueLiteManager> {
		// Properties
		public Player SpawnedPlayer { get; private set; } = null;
		public Vector3 SpawnedPlayerPosition => SpawnedPlayer.Tr.position;

		// Fields
		private const string playerKey = "01_FlowerMagician";

		protected override void Initialize() {
			base.Initialize();
		}

		public void SetPlayerOnEnteredDungeon(Vector2 playerSpawnPosition) {
			SpawnPlayer();
			SetPlayerPosition(playerSpawnPosition);
			ActivePlayer();
		}

		public void SpawnPlayer() {
			if (SpawnedPlayer)
				return;
			
			if (!ObjectPoolManager.Instance.IsExistInPoolDictionary(playerKey)) {
				var origin = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(playerKey, true);
				ObjectPoolManager.Instance.AddToPool(PoolInformation.New(origin, true, 1));
			}

			SpawnedPlayer = ObjectPoolManager.Instance.Spawn<Player>(playerKey, Vector3.zero);
		}
		
		public void SetPlayerPosition(Vector2 position) {
			if (!SpawnedPlayer) {
				return;
			}
			SpawnedPlayer.Tr.position = position;
		}

		public void ActivePlayer() {
			if (!SpawnedPlayer)
				return;
			SpawnedPlayer.gameObject.SetActive(true);
		}

		public void DisablePlayer() {
			if (!SpawnedPlayer)
				return;
			SpawnedPlayer.gameObject.SetActive(false);
		}

		public void DespawnPlayer() {
			if (!SpawnedPlayer) {
				return;
			}
			ObjectPoolManager.Instance.Despawn(SpawnedPlayer.gameObject);
		}

		public void DisableInput() {
			
		}
		
		public void EnableInput() {
			
		}

		public void TimeScaleZero() {
			Time.timeScale = 0f;
		}

		public void RestoreTimeScale() {
			Time.timeScale = 1f;
		}

		public bool IsPlayerExistAndAlive() => SpawnedPlayer && !SpawnedPlayer.IsDead();

		public bool IsPlayerNotExistOrDeath() => !SpawnedPlayer || SpawnedPlayer.IsDead();
	}
}
