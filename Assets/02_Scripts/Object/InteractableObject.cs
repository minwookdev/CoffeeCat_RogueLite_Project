using System;
using CoffeeCat.FrameWork;
using UnityEngine;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;

namespace CoffeeCat {
    public class InteractableObject : MonoBehaviour {
        [field: SerializeField] public InteractableType InteractType { get; protected set; } = InteractableType.None;
        [ShowInInspector, ReadOnly] public bool IsEnteredPlayer { get; private set; } = false;
        [SerializeField] private ParticleSystem ps = null;

        protected void Start()
        {
            this.OnTriggerEnter2DAsObservable()
                .TakeUntilDestroy(this)
                .Where(other => other.gameObject.layer == Defines.GetPlayerLayer())
                .Subscribe(playerCollider => {
                    // Ignore Not Triggered Collider
                    if (!playerCollider.isTrigger)
                        return;
                    
                    OnPlayerEnter();
                    IsEnteredPlayer = true;
                    
                    RogueLiteManager.Inst.SetInteractable(this);
                })
                .AddTo(this);
            
            this.OnTriggerStay2DAsObservable()
                .TakeUntilDestroy(this)
                .Where(other => other.gameObject.layer == Defines.GetPlayerLayer())
                .Subscribe(playerCollider => {
                    // Ignore Not Triggered Collider
                    if (!playerCollider.isTrigger)
                        return;
                    
                    var currentInteractable = RogueLiteManager.Inst.Interactable;
                    if (currentInteractable || currentInteractable == this) 
                        return;
                    RogueLiteManager.Inst.SetInteractable(this);
                })
                .AddTo(this);

            this.OnTriggerExit2DAsObservable()
                .TakeUntilDestroy(this)
                .Where(other => other.gameObject.layer == Defines.GetPlayerLayer())
                .Subscribe(playerCollider => {
                    // Ignore Not Triggered Collider
                    if (!playerCollider.isTrigger)
                        return;
                    
                    OnPlayerExit();
                    IsEnteredPlayer = false;
                    
                    var currentInteractable = RogueLiteManager.Inst.Interactable;
                    if (currentInteractable != this) 
                        return;
                    RogueLiteManager.Inst.ReleaseInteractable();
                })
                .AddTo(this);
        }

        private void Update() {
            if (!IsEnteredPlayer) 
                return;
            OnPlayerStay();
        }

        private void OnDisable() {
            IsEnteredPlayer = false;
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
