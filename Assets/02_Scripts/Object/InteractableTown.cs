using CoffeeCat.Utils;
using UnityEngine;

namespace CoffeeCat {
    public class InteractableTown : InteractableObject {
        protected override void OnPlayerStay() {
            if (!Input.GetKeyDown(KeyCode.Q)) 
                return;
            CatLog.Log("Town");
        }
    }
}