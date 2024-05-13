using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeCat
{
    public class FlowerMagicianState : PlayerState
    {
        // TODO : 주황색 밑줄이 너무 많아ㅜ 어케 좀 해바
        
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
            // AddAnimation
            anim.AnimationState.SetAnimation(1, animAttack, false).TimeScale = 1.5f;
        }

        protected override void Update_AttackState()
        {
            anim.AnimationState.Complete += delegate
            {
                if (player.IsWalking())
                    ChangeState(EnumPlayerState.Walk);
                else
                    ChangeState(EnumPlayerState.Idle);
            };
        }

        protected override void Exit_AttackState()
        {
            anim.AnimationState.ClearTrack(1);
            // anim.ClearState();
        }

        #endregion

        #region HIT

        protected override void Enter_HitState()
        {
        }

        protected override void Update_HitState()
        {
        }

        protected override void Exit_HitState()
        {
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