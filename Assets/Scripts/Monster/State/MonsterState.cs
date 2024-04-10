using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;

namespace CoffeeCat {
    public class MonsterState : MonoBehaviour {
        public enum EnumMonsterState {
            None,
            Idle,
            Tracking,
            Patrol,
            Attack,
            TakeDamage,
            Death,
        }
        
        [TitleGroup("State", order: 0), ShowInInspector, ReadOnly] 
        public EnumMonsterState State { get; private set; } = EnumMonsterState.None;
        
        [TitleGroup("Models", order: 1), SerializeField] 
        protected SpriteRenderer sprite = null;
        [TitleGroup("Models", order: 1), SerializeField] 
        protected SpriteRenderer[] sprites = null;
        // Default Sprite Renderer Direction is Right 

        [TitleGroup("Movement", order: 2), SerializeField] 
        protected float moveSpeed = 8f;

        // Fields
        protected Transform tr = null;
        protected Animator anim = null;
        protected MonsterStat stat = null;
        protected Collider2D bodyCollider = null;
        protected Rigidbody2D rigidBody = null;
        private bool isDefaultSpriteFlipX = false;
        private float originAnimationSpeed = 0f;

        protected virtual void Initialize() {
            tr = GetComponent<Transform>();
            anim = GetComponent<Animator>();
            bodyCollider = GetComponent<Collider2D>();
            rigidBody = GetComponent<Rigidbody2D>();
            originAnimationSpeed = anim.speed;
        }

        protected virtual void OnActivated() {
            // Awake    -> Initialize 사용
            // OnEnable -> Activated 사용
            // awake 사용을 피하고 virtual, overriding을 사용하기 위해 함수를 따로 두었음
        }

        protected void Start() {
            SubscribeOnEnableObservable();
        }

        protected void Update() {
            StateUpdate();
        }

        protected void FixedUpdate() {
            StateFixedUpdate();
        }

        private void SubscribeOnEnableObservable() {
            this.OnEnableAsObservable()
                .Skip(TimeSpan.Zero)
                .TakeUntilDestroy(this)
                .DoOnSubscribe(() => {
                    this.Initialize();
                    this.OnActivated();
                })
                .Subscribe(_ => { this.OnActivated(); })
                .AddTo(this);
        }

        private void StateUpdate() {
            switch (State) {
                case EnumMonsterState.None:       break;
                case EnumMonsterState.Idle:       OnUpdateIdleState(); break;
                case EnumMonsterState.Tracking:   OnUpdateTrackingState(); break;
                case EnumMonsterState.Patrol:     break;
                case EnumMonsterState.Attack:     OnUpdateAttackState(); break;
                case EnumMonsterState.Death:      OnUpdateDeathState(); break;
                case EnumMonsterState.TakeDamage: OnUpdateTakeDamageState(); break;
                default:                          break;
            }
        }

        private void StateFixedUpdate() {
            switch (State) {
                case EnumMonsterState.None:     break;
                case EnumMonsterState.Idle:     break;
                case EnumMonsterState.Tracking: OnFixedUpdateTrackingState(); break;
                case EnumMonsterState.Patrol:   break;
                case EnumMonsterState.Attack:   OnFixedUpdateAttackState(); break;
                case EnumMonsterState.Death:    break;
                case EnumMonsterState.TakeDamage: OnFixedUpdateTakeDamageState(); break;
                default: CatLog.ELog("NotImplementedThisState."); return;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected void StateChange(EnumMonsterState targetState, float delaySeconds = 0f) {
            if (delaySeconds <= 0) {
                Execute();
                return;
            }

            Observable.Timer(TimeSpan.FromSeconds(delaySeconds))
                      .Skip(TimeSpan.Zero)
                      .TakeUntilDisable(this)
                      .Subscribe(_ => { Execute(); })
                      .AddTo(this);

            void Execute() {
                switch (State) {
                    case EnumMonsterState.None:       break;
                    case EnumMonsterState.Patrol:     break;
                    case EnumMonsterState.Idle:       OnExitIdleState(); break;
                    case EnumMonsterState.Tracking:   OnExitTrackingState(); break;
                    case EnumMonsterState.Attack:     OnExitAttackState(); break;
                    case EnumMonsterState.Death:      OnExitDeathState(); break;
                    case EnumMonsterState.TakeDamage: OnExitTakeDamageState(); break;
                    default:                          break;
                }

                this.State = targetState;

                switch (State) {
                    case EnumMonsterState.None:       break;
                    case EnumMonsterState.Patrol:     break;
                    case EnumMonsterState.Idle:       OnEnterIdleState(); break;
                    case EnumMonsterState.Tracking:   OnEnterTrackingState(); break;
                    case EnumMonsterState.Attack:     OnEnterAttackState(); break;
                    case EnumMonsterState.Death:      OnEnterDeathState(); break;
                    case EnumMonsterState.TakeDamage: OnEnterTakeDamageState(); break;
                    default:                          break;
                }
            }
        }

        #region IDLE

        protected virtual void OnEnterIdleState() { }

        protected virtual void OnUpdateIdleState() { }

        protected virtual void OnExitIdleState() { }

        #endregion

        #region TRACKING

        protected virtual void OnEnterTrackingState() { }

        protected virtual void OnUpdateTrackingState() { }

        protected virtual void OnFixedUpdateTrackingState() { }

        protected virtual void OnExitTrackingState() { }

        #endregion

        #region DEATH

        protected virtual void OnEnterDeathState() { }

        protected virtual void OnUpdateDeathState() { }

        protected virtual void OnExitDeathState() { }

        #endregion

        #region ATTACK

        protected virtual void OnEnterAttackState() { }

        protected virtual void OnUpdateAttackState() { }

        protected virtual void OnFixedUpdateAttackState() { }

        protected virtual void OnExitAttackState() { }

        #endregion

        #region TAKE DAMAGE

        protected virtual void OnEnterTakeDamageState() { }

        protected virtual void OnUpdateTakeDamageState() { }

        protected virtual void OnFixedUpdateTakeDamageState() { }

        protected virtual void OnExitTakeDamageState() { }

        #endregion

        #region OTHER METHODS
        
        public void SetStat(MonsterStat stat) => this.stat = stat;

        public void SetDisplay(bool isDisplay) {
            foreach (var sprite in sprites) {
                sprite.enabled = isDisplay;
            }

            if (bodyCollider) {
                bodyCollider.enabled = isDisplay;
            }
        }

        protected void SetFlipX(bool isRight) => sprite.flipX = isDefaultSpriteFlipX ? isRight : !isRight;

        protected void IncreaseAnimationSpeed(float ratio) => anim.speed = originAnimationSpeed * ratio;

        protected void RestoreAnimationSpeed() => anim.speed = originAnimationSpeed;

        public virtual void OnTakeDamage() { }
        
        /// <summary>
        /// Returns the direction and arrival status of the monster from the current monster's location to the player.
        /// </summary>
        /// <param name="sqrDistance">SqrMagnitude Distance</param>
        /// <param name="normalizedDirection">Direction to Player</param>
        /// <returns>is Arrival to Player</returns>
        protected bool TrackPlayerToCertainDistance(float sqrDistance, out Vector2 normalizedDirection) {
            // Get Direction to Player Position
            normalizedDirection = Math2DHelper.GetDirection(tr.position, RogueLiteManager.Instance.SpawnedPlayerPosition);
            
            // Get Distance
            var sqrDistanceToPlayer = normalizedDirection.sqrMagnitude;
            
            // Trans to Normalized
            normalizedDirection.Normalize();
            
            // Return Result
            return sqrDistanceToPlayer <= sqrDistance;
        }
        
        protected void SetVelocity(Vector2 direction, float speed) => rigidBody.velocity = direction * speed;

        protected void SetVelocityZero() => rigidBody.velocity = Vector2.zero;

        #endregion
    }
}
