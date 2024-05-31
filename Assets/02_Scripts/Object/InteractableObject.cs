using UnityEngine;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractableObject : MonoBehaviour {
        [SerializeField] private ParticleSystem ps = null;
        
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.layer == Defines.GetPlayerLayer()) {
                
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.layer == Defines.GetPlayerLayer()) {
                
            }
        }

        public virtual void PlayParticle() {
            ps.Play();
        }
        
        public virtual void StopParticle() {
            ps.Stop();
        }

        protected virtual void OnPlayerEnter() {
            
        }

        protected virtual void OnPlayerExit() {
            
        }
    }
}
