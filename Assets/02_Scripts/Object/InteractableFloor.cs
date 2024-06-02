using UnityEngine;

namespace CoffeeCat 
{
    public class InteractableFloor : InteractableObject 
    {
        protected override void OnPlayerStay() {
            if (!Input.GetKeyDown(KeyCode.Q)) 
                return;
            DisposeInteractableSign();
            StageManager.Instance.RequestGenerateNextFloor();
        }
    }
}
