using UnityEngine;

namespace CoffeeCat 
{
    public class InteractableFloor : InteractableObject 
    {
        protected override void OnPlayerStay() {
            if (Input.GetKeyDown(KeyCode.Q)) {
                
            }
        }
    }
}
