using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
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
		[TitleGroup("Attack", order: 3), SerializeField] private Effector dashEffector = null;
		
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
			base.OnEnterDeathState();
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
			dashEffector.Play(EffectPlayOptions.Custom(isSetFlipFromDirection: true, isSetPivotFromDirection: true, 
			                                           direction: GetParticleFixedDirection(isMoveDirectionRight)));
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
				
				// Update flip / pivot when Changed Move Direction
				// Get Particle System Fixed Direction
				var particleFixedDirection = GetParticleFixedDirection(isMoveDirectionRight);
				
				// Set Sprite/ParticleSystems Flip and Pivot
				SetFlipX(isMoveDirectionRight);
				dashEffector.SetAllParticlesPivot(particleFixedDirection);
				dashEffector.SetAllParticlesFlip(particleFixedDirection);
					
				isPreviousMoveDirectionValue = isMoveDirectionRight;
				return;
			}

			// Dash Attack Start
			isDashAttackStart = true;

			// Check Velocity after AddForced
			if (!isDashAttacked) {
				return;
			}

			var velocity = rigidBody.velocity.sqrMagnitude;
			if (velocity > 2f) {
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
			dashEffector.Stop(false);
			RestoreAnimationSpeed();
		}
		
		#endregion
		
		#region Particle Method

		private Effector.ParticleFixedDirection GetParticleFixedDirection(bool isDirectionRight) {
			return (isDirectionRight) ? Effector.ParticleFixedDirection.Right : Effector.ParticleFixedDirection.Left;
		}
		
		#endregion
	}
}
