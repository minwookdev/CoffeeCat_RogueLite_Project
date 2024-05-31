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
                .Skip(0)
                .TakeUntilDestroy(this)
                .Where(other => other.gameObject.layer == Defines.GetPlayerLayer())
                .Subscribe(playerCollider =>
                {
                    if (IsEnteredPlayer) {
                        OnPlayerStay();
                        return;
                    }
                    IsEnteredPlayer = true;
                    lastCollider = playerCollider;
                    OnPlayerEnter();
                })
                .AddTo(this);

            this.OnTriggerExit2DAsObservable()
                .Skip(0)
                .TakeUntilDestroy(this)
                .Where(other => other.gameObject.layer == Defines.GetPlayerLayer())
                .Subscribe(playerCollider =>
                {
                    if (!IsEnteredPlayer || lastCollider != playerCollider)
                        return;
                    IsEnteredPlayer = false;
                    lastCollider = null;
                    OnPlayerExit();
                })
                .AddTo(this);
        }

        public virtual void PlayParticle() {
            ps.Play();
        }
        
        public virtual void StopParticle() {
            ps.Stop();
        }

        protected virtual void OnPlayerEnter() {
            
        }

        protected virtual void OnPlayerStay() {
            
        }

        protected virtual void OnPlayerExit() {
            
        }
    }
}
