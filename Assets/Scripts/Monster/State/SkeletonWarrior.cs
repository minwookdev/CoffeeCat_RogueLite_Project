using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using DG.Tweening;
using UniRx;

namespace CoffeeCat {
	public class SkeletonWarrior : MonsterState {
		[TitleGroup("Movement"), SerializeField] private float sqrDistanceToAttack = 1.5f;

		[TitleGroup("Attack", order: 3), SerializeField] private float attackStateAnimationSpeedRatio = 2f;
		[TitleGroup("Attack", order: 3), SerializeField] private float dashAnimationSpeedRatio = 2.5f;
		[TitleGroup("Attack", order: 3), SerializeField] private float dashAttackStartSeconds = 0.8f;
		[TitleGroup("Attack", order: 3), SerializeField] private float attackCancelledSqrDistance = 10f;
		[TitleGroup("Attack", order: 3), SerializeField] private float dashAttackSpeed = 5f;
		[TitleGroup("Attack", order: 3), SerializeField] private float linearDrag = 3f;
		[TitleGroup("Attack", order: 3), SerializeField] private ParticleSystem dashReadyParticle1 = null;
		[TitleGroup("Attack", order: 3), SerializeField] private ParticleSystem dashReadyParticle2 = null;
		[TitleGroup("Attack", order: 3), SerializeField] private ParticleSystem dashPrticle = null;
		
		//Fields
		private bool isMoveDirectionRight = false;
		private bool isDashAttackStart = false;
		private bool isDashAttacked = false;
		private bool isPreviousMoveDirectionValue = false;
		private float currentAttackSeconds = 0f;
		private Vector2 normalizedMoveDirection = Vector2.zero;
		private readonly int animHash = Animator.StringToHash("AnimState");

		protected override void Initialize() {
			base.Initialize();
			rigidBody.drag = linearDrag;
			deathTimerObservable = Observable.Timer(TimeSpan.FromSeconds(deathAnimDuration))
			                                 .DoOnSubscribe(() => { /*CatLog.Log("DoOnSubscribe");*/ })
			                                 .Skip(0)
			                                 .TakeUntilDisable(this)
			                                 .Publish()
			                                 .RefCount();
		}

		protected override void OnActivated() {
			StateChange(EnumMonsterState.Idle);
		}
		
		#region IDLE

		protected override void OnEnterIdleState() {
			anim.SetInteger(animHash, 0);
		}

		protected override void OnUpdateIdleState() {
			// Check Player Alive And Exist
			if (RogueLiteManager.Instance.IsPlayerNotExistOrDeath()) {
				return;
			}

			// To Tracking State
			StateChange(EnumMonsterState.Tracking);
		}
		
		#endregion
		
		#region TRACKING

		protected override void OnEnterTrackingState() {
			anim.SetInteger(animHash, 1);
		}

		protected override void OnUpdateTrackingState() {
			// Check Play Death Or Not Exist
			if (RogueLiteManager.Instance.IsPlayerNotExistOrDeath()) {
				StateChange(EnumMonsterState.Idle);
				return;
			}

			// Get Direction to Player Position
			bool isArrivalToPlayer = TrackPlayerToCertainDistance(sqrDistanceToAttack, out normalizedMoveDirection);
			
			// Set Flip SpriteRenderer
			isMoveDirectionRight = Math2DHelper.GetDirectionIsRight(normalizedMoveDirection);
			SetFlipX(isMoveDirectionRight);
			
			// Check Can be Attack to Player
			if (!isArrivalToPlayer)
				return;
			StateChange(EnumMonsterState.Attack);
		}

		protected override void OnFixedUpdateTrackingState() {
			SetVelocity(normalizedMoveDirection, moveSpeed);
		}

		protected override void OnExitTrackingState() {
			SetVelocityZero();
		}
		
		#endregion

		#region DEATH

		protected override void OnEnterDeathState() {
			anim.SetInteger(animHash, 2);
			deathTimerObservable?.Subscribe(_ => { Despawn(); })
			                    .AddTo(this);
		}

		protected override void OnUpdateDeathState() {
			base.OnUpdateDeathState();
		}

		protected override void OnExitDeathState() {
			base.OnExitDeathState();
		}
		
		#endregion
		
		#region ATTACK

		protected override void OnEnterAttackState() {
			anim.SetInteger(animHash, 1);
			IncreaseAnimationSpeed(attackStateAnimationSpeedRatio);
			
			// TODO: OnFlip Event Listened Observable.OnValueChange?
			
			// Get Direction And Apply Dash Dust Particle With Play
			isMoveDirectionRight = Math2DHelper.GetDirectionIsRight(normalizedMoveDirection);
			PlayDashReadyParticle(isMoveDirectionRight);
			isPreviousMoveDirectionValue = isMoveDirectionRight;
		}

		protected override void OnUpdateAttackState() {
			if (RogueLiteManager.Instance.IsPlayerNotExistOrDeath()) {
				StateChange(EnumMonsterState.Idle);
				return;
			}

			currentAttackSeconds += Time.deltaTime;
			// Ready to Attack Time
			if (currentAttackSeconds <= dashAttackStartSeconds) {
				// Check Distance to Player and Return Tracking State if the Distance too far
				var isCancelledAttack = TrackPlayerToCertainDistance(attackCancelledSqrDistance, out normalizedMoveDirection);
				if (!isCancelledAttack) {
					StateChange(EnumMonsterState.Tracking);
				}

				// Get Move Direction
				isMoveDirectionRight = Math2DHelper.GetDirectionIsRight(normalizedMoveDirection);
				if (isPreviousMoveDirectionValue == isMoveDirectionRight) 
					return;
				
				// Set Sprite/ParticleSystems Flip and Pivot
				SetFlipX(isMoveDirectionRight);
				PlayDashReadyParticle(isMoveDirectionRight);
					
				isPreviousMoveDirectionValue = isMoveDirectionRight;
				return;
			}

			// Ready to Dash Attack
			if (!isDashAttackStart) {
				isDashAttackStart = true;
				PlayDashParticle();
			}
			
			// Check Velocity after AddForced 
			if (!isDashAttacked) {
				return;
			}

			var velocity = rigidBody.velocity.sqrMagnitude;
			if (velocity >= 12f) {
				return;
			}
			
			// Change State if Lowed Velocity
			StateChange(EnumMonsterState.Tracking);
		}

		protected override void OnFixedUpdateAttackState() {
			if (!isDashAttackStart || isDashAttacked) {
				return;
			}

			IncreaseAnimationSpeed(dashAnimationSpeedRatio); // need RigidBody LinearDrag Setting...
			rigidBody.AddForce(normalizedMoveDirection * dashAttackSpeed, ForceMode2D.Impulse);
			isDashAttacked = true;
		}

		protected override void OnExitAttackState() {
			currentAttackSeconds = 0f;
			isDashAttackStart = false;
			isDashAttacked = false;
			RestoreAnimationSpeed();
			StopDashParticle();
		}
		
		#endregion
		
		#region Particle Method

		private Effector.ParticleFixedDirection GetParticleFixedDirection(bool isDirectionRight) {
			return (isDirectionRight) ? Effector.ParticleFixedDirection.Right : Effector.ParticleFixedDirection.Left;
		}

		private void PlayDashReadyParticle(bool isDirectionRight) {
			dashPrticle.Stop();
			dashReadyParticle1.Stop();
			// dashReadyParticle2.Stop();
			var module1Pos = isDirectionRight ? new Vector3(0f, -0.65f, 0f) : new Vector3(0f, 0.65f, 0f);
			var module2Pos = isDirectionRight ? new Vector3(0f, -0.25f, 0f) : new Vector3(0f, 0.25f, 0f);
			var rotation = isDirectionRight ? new Vector3(0f, -90f, 0f) : new Vector3(0f, 90f, 0f);
			var module1 = dashReadyParticle1.shape;
			var module2 = dashReadyParticle2.shape;
			module1.rotation = rotation;
			module2.rotation = rotation;
			module1.position = module1Pos;
			module2.position = module2Pos;
			
			dashReadyParticle1.Play();
			// dashReadyParticle2.Play();
		}

		private void PlayDashParticle() {
			dashReadyParticle1.Stop();
			// dashReadyParticle2.Stop();
			dashPrticle.Play();
		}
		
		private void StopDashParticle() {
			dashReadyParticle1.Stop();
			// dashReadyParticle2.Stop();
			dashPrticle.Stop();
		}
		
		#endregion
	}
}
