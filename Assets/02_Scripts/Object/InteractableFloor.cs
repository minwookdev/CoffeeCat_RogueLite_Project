using CoffeeCat.FrameWork;
using UnityEngine;

namespace CoffeeCat 
{
    public class InteractableFloor : InteractableObject 
    {
        protected override void OnPlayerStay() {
#if UNITY_ANDROID
            UIPresenter.Inst.EnableNextFloorButton();
#elif UNITY_STANDALONE
            if (!Input.GetKeyDown(KeyCode.Q)) 
                return;
            DisposeInteractableSign();
            StageManager.Instance.RequestGenerateNextFloor();
#endif
        }

        protected override void OnPlayerExit() {
#if UNITY_ANDROID
            UIPresenter.Inst.DisableNextFloorButton();
#endif
        }
    }
}
