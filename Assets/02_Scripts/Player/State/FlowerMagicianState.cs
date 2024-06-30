using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoffeeCat.Utils;
using Spine;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;

namespace CoffeeCat
{
    public class FlowerMagicianState : PlayerState
    {
        private TrackEntry currentTrack = null;

        protected override void Start()
        {
            base.Start();
            player = GetComponent<FlowerMagician>();
        }

        #region IDLE

        protected override void Enter_IdleState()
        {
            currentTrack = anim.AnimationState.SetAnimation(0, animIdle, true);
        }

        protected override void Update_IdleState()
        {
            if (player.IsDead())
                ChangeState(EnumPlayerState.Dead);
            if (player.IsDamaged())
                ChangeState(EnumPlayerState.Hit);
            if (player.IsAttacking())
                ChangeState(EnumPlayerState.Attack);
            if (player.IsWalking())
                ChangeState(EnumPlayerState.Walk);
        }

        protected override void Exit_IdleState()
        {
        }

        #endregion

        #region WALK

        protected override void Enter_WalkState()
        {
            currentTrack = anim.AnimationState.SetAnimation(0, animWalk, true);
        }

        protected override void Update_WalkState()
        {
            if (player.IsDead())
                ChangeState(EnumPlayerState.Dead);
            if (player.IsDamaged())
                ChangeState(EnumPlayerState.Hit);
            if (player.IsAttacking())
                ChangeState(EnumPlayerState.Attack);
            if (!player.IsWalking())
                ChangeState(EnumPlayerState.Idle);
        }

        protected override void Exit_WalkState()
        {
        }

        #endregion

        #region ATTACK

        protected override void Enter_AttackState()
        {
            currentTrack = anim.AnimationState.SetAnimation(1, animAttack, false);
            currentTrack.TimeScale = 1.5f;
        }

        protected override void Update_AttackState()
        {
            if (player.IsDead())
                ChangeState(EnumPlayerState.Dead);
            if (player.IsDamaged())
                ChangeState(EnumPlayerState.Hit);
            if (currentTrack.IsComplete)
                ChangeState(player.IsWalking() ? EnumPlayerState.Walk : EnumPlayerState.Idle);
        }

        protected override void Exit_AttackState()
        {
            anim.AnimationState.ClearTrack(1);
            player.FinishAttackAnimation();
        }

        #endregion

        #region HIT

        protected override void Enter_HitState()
        {
            currentTrack = anim.AnimationState.SetAnimation(1, animHit, false);
            currentTrack.TimeScale = 1.3f;

            OnPlayerInvincivle();
        }

        protected override void Update_HitState()
        {
            if (player.IsDead())
                ChangeState(EnumPlayerState.Dead);

            if (currentTrack.IsComplete)
                ChangeState(player.IsWalking() ? EnumPlayerState.Walk : EnumPlayerState.Idle);

            // 스턴이 필요할까?
            // 다른 연출은 필요없을까?
        }

        protected override void Exit_HitState()
        {
            anim.AnimationState.ClearTrack(currentTrack.TrackIndex);
            player.FinishHitAnimation();
        }

        #endregion

        #region DEAD

        protected override void Enter_DeadState()
        {
            currentTrack = anim.AnimationState.SetAnimation(0, animDead, false);
            currentTrack.TimeScale = 0.7f;
        }

        protected override void Update_DeadState()
        {
            if (currentTrack.IsComplete)
                ChangeState(EnumPlayerState.None);
        }

        protected override void Exit_DeadState()
        {
            anim.AnimationState.ClearTracks();
        }

        #endregion
    }
}