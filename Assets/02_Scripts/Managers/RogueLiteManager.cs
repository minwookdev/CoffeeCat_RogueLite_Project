/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION: 
using System;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using UnityEngine;

namespace CoffeeCat.FrameWork {
	public class RogueLiteManager : DynamicSingleton<RogueLiteManager> {
		// Properties
		public Player SpawnedPlayer { get; private set; } = null;
		public Vector3 SpawnedPlayerPosition => SpawnedPlayer.Tr.position;

		// Fields
		private const string playerKey = "FlowerMagician";
		
		public InteractableObject Interactable { get; private set; } = null;

		protected override void Initialize() {
			base.Initialize();
			
			InputManager.BindInteractInput(OnInteractAction);
		}

		public void SetPlayerOnEnteredDungeon(Vector2 playerSpawnPosition) {
			SpawnPlayer();
			SetPlayerPosition(playerSpawnPosition);
			ActivePlayer();
		}

		private void SpawnPlayer() {
			if (SpawnedPlayer)
				return;
			
			SpawnedPlayer = ObjectPoolManager.Inst.Spawn<Player>(playerKey, Vector3.zero);
		}

		private void SetPlayerPosition(Vector2 position) {
			if (!SpawnedPlayer) {
				return;
			}
			SpawnedPlayer.Tr.position = position;
		}

		private void ActivePlayer() {
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
			ObjectPoolManager.Inst.Despawn(SpawnedPlayer.gameObject);
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

		private void OnInteractAction() {
			if (!Interactable) {
				return;
			}
			
			var type = Interactable.InteractType;
			switch (type) {
				case InteractableType.None:
					break;
				case InteractableType.Floor:
					CatLog.Log("Floor");
					StageManager.Inst.RequestGenerateNextFloor();
					ReleaseInteractable();
					break;
				case InteractableType.Shop:
					CatLog.Log("Shop");
					break;
				case InteractableType.Reward:
					CatLog.Log("Reward");
					break;
				case InteractableType.Boss:
					CatLog.Log("Boss");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
        
		public void SetInteractable(InteractableObject interactable) {
			Interactable = interactable;
			if (InputManager.IsExist) {
				InputManager.Inst.EnableInteractable(interactable.InteractType);
			}
		}
		
		public void ReleaseInteractable() {
			Interactable = null;
			if (InputManager.IsExist) {
				InputManager.Inst.DisableInteractable();
			}
		}
	}
}
