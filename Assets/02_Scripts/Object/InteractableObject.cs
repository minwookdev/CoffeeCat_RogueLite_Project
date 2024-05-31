using UnityEngine;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;

namespace CoffeeCat {
    public class InteractableObject : MonoBehaviour {
        [SerializeField] private ParticleSystem ps = null;
        [ShowInInspector, ReadOnly] public bool IsEnteredPlayer { get; private set; } = false;

        protected void Start()
        {
            Collider2D lastCollider = null;
            
            this.OnTriggerStay2DAsObservable()
                .Where(other => other.gameObject.layer == Defines.GetPlayerLayer())
                .Subscribe(playerCollider =>
                {
                    if (IsEnteredPlayer)
                        return;
                    IsEnteredPlayer = true;
                    lastCollider = playerCollider;
                    OnPlayerEnter();
                });

            this.OnTriggerExit2DAsObservable()
                .Where(other => other.gameObject.layer == Defines.GetPlayerLayer())
                .Subscribe(playerCollider =>
                {
                    if (!IsEnteredPlayer || lastCollider != playerCollider)
                        return;
                    IsEnteredPlayer = false;
                    lastCollider = null;
                    OnPlayerExit();
                });
        }

        public virtual void PlayParticle() {
            ps.Play();
        }
        
        public virtual void StopParticle() {
            ps.Stop();
        }

        protected virtual void OnPlayerEnter()
        {
            CatLog.Log("Enter Player To Interactable.");
        }

        protected virtual void OnPlayerExit()
        {
            CatLog.Log("Exit Player From Interactable.");
        }
    }
}
