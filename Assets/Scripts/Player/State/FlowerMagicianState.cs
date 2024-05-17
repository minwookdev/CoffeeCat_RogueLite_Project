using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeCat
{
    public class FlowerMagicianState : PlayerState
    {
        protected override void Start()
        {
            base.Start();
            player = GetComponent<Player_FlowerMagician>();
        }
        
        #region IDLE

        protected override void Enter_IdleState()
        {
            anim.AnimationState.SetAnimation(0, animIdle, true);
        }

        protected override void Update_IdleState()
        {
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
            anim.AnimationState.SetAnimation(0, animWalk, true);
        }

        protected override void Update_WalkState()
        {
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
            anim.AnimationState.SetAnimation(1, animAttack, false).TimeScale = 1.5f;
            anim.AnimationState.Complete += delegate
            {
                ChangeState(player.IsWalking() ? EnumPlayerState.Walk : EnumPlayerState.Idle);
            };
        }

        protected override void Update_AttackState()
        {
            // 공격과 스턴 사이 그 어딘가,,
            
        }

        protected override void Exit_AttackState()
        {
            anim.AnimationState.ClearTrack(1);
            player.FinishAttack();
        }

        #endregion

        #region HIT

        protected override void Enter_HitState()
        {
            anim.AnimationState.SetAnimation(1, animHit, false).TimeScale = 1.3f;
            anim.AnimationState.Complete += delegate
            {
                ChangeState(player.IsWalking() ? EnumPlayerState.Walk : EnumPlayerState.Idle);
            };
        }

        protected override void Update_HitState()
        {
        }

        protected override void Exit_HitState()
        {
            anim.AnimationState.ClearTrack(1);
            player.FinishHitAnimation();
        }

        #endregion

        #region DEAD

        protected override void Enter_DeadState()
        {
        }

        protected override void Update_DeadState()
        {
        }

        protected override void Exit_DeadState()
        {
        }

        #endregion
    }
}