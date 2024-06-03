using CoffeeCat.FrameWork;
using UnityEngine;

namespace CoffeeCat 
{
    public class InteractableFloor : InteractableObject 
    {
        protected override void OnPlayerStay() {
            UIPresenter.Instance.EnableNextFloorButton();
            
            if (!Input.GetKeyDown(KeyCode.Q)) 
                return;
            DisposeInteractableSign();
            StageManager.Instance.RequestGenerateNextFloor();
        }

        protected override void OnPlayerExit() {
            UIPresenter.Instance.DisableNextFloor();
        }
    }
}
