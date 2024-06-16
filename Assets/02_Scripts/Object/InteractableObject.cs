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
        [SerializeField] protected bool isShowInteractableSign = true;
        [SerializeField] private ParticleSystem ps = null;
        [SerializeField, ReadOnly] private Transform interactableSignTr = null;
        [ShowInInspector, ReadOnly] public bool IsEnteredPlayer { get; private set; } = false;
        private Transform playerTr = null;
        private readonly Vector3 interactableSignOffset = new(0, 1.25f, 0);

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
                        return;
                    }

                    lastCollider = playerCollider;
                    if (isShowInteractableSign) {
                        SpawnInteractableSign(playerCollider.gameObject);
                    }
                    OnPlayerEnter();
                    
                    IsEnteredPlayer = true;
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
                    DisposeInteractableSign();
                })
                .AddTo(this);
        }

        private void Update() {
            if (!IsEnteredPlayer) 
                return;
            UpdateInteractableSign();
            OnPlayerStay();
        }

        private void OnDisable() {
            IsEnteredPlayer = false;
            DisposeInteractableSign();
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
        
        private Vector3 GetInteractableSignPosition(Vector3 playerPos) {
            return playerPos + interactableSignOffset;
        }

        private void SpawnInteractableSign(GameObject collisionGameObject) {
            if (!playerTr) {
                playerTr = collisionGameObject.GetComponent<Transform>();
            }

            if (interactableSignTr) {
                return;
            }
            var signPosition = GetInteractableSignPosition(playerTr.position);
            interactableSignTr = ObjectPoolManager.Inst.Spawn<Transform>(AddressablesKey.InteractableSign.ToStringEx(), signPosition);
        }

        private void UpdateInteractableSign() {
            if (!interactableSignTr) 
                return;
            var signPosition = GetInteractableSignPosition(playerTr.position);
            interactableSignTr.position = signPosition;
        }

        protected void DisposeInteractableSign() {
            if (!interactableSignTr) {
                return;
            }

            ObjectPoolManager.Inst.Despawn(interactableSignTr.gameObject);
            interactableSignTr = null;
        }
    }
}
