using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerState : MonoBehaviour
    {
        public enum EnumPlayerState
        {
            None,
            Idle,
            Walk,
            Attack,
            Hit,
            Dead
        }
        
        [SerializeField] public EnumPlayerState State = EnumPlayerState.None;

        protected Transform tr = null;
        protected SkeletonAnimation anim = null;
        protected Player player = null;

        protected readonly string animIdle = "Idle_2";
        protected readonly string animWalk = "Walk_NoHand";
        protected readonly string animAttack = "Attack_2";
        protected readonly string animHit = "Hit";
        protected readonly string animDead = "Die_2";

        protected virtual void Start()
        {
            tr = GetComponent<Transform>();
            anim = GetComponent<SkeletonAnimation>();
            
            ChangeState(EnumPlayerState.Idle);
        }

        protected virtual void Update()
        {
            UpdateState();
        }

        private void UpdateState()
        {
            switch (State)
            {
                case EnumPlayerState.None:
                    break;
                case EnumPlayerState.Idle:
                    Update_IdleState();
                    break;
                case EnumPlayerState.Walk:
                    Update_WalkState();
                    break;
                case EnumPlayerState.Attack:
                    Update_AttackState();
                    break;
                case EnumPlayerState.Hit:
                    Update_HitState();
                    break;
                case EnumPlayerState.Dead:
                    Update_DeadState();
                    break;
                default:
                    break;
            }
        }

        protected void ChangeState(EnumPlayerState targetState)
        {
            switch (State)
            {
                case EnumPlayerState.None:
                    break;
                case EnumPlayerState.Idle:
                    Exit_IdleState();
                    break;
                case EnumPlayerState.Walk:
                    Exit_WalkState();
                    break;
                case EnumPlayerState.Attack:
                    Exit_AttackState();
                    break;
                case EnumPlayerState.Hit:
                    Exit_HitState();
                    break;
                case EnumPlayerState.Dead:
                    Exit_DeadState();
                    break;
                default:
                    break;
            }

            State = targetState;

            switch (State)
            {
                case EnumPlayerState.None:
                    break;
                case EnumPlayerState.Idle:
                    Enter_IdleState();
                    break;
                case EnumPlayerState.Walk:
                    Enter_WalkState();
                    break;
                case EnumPlayerState.Attack:
                    Enter_AttackState();
                    break;
                case EnumPlayerState.Hit:
                    Enter_HitState();
                    break;
                case EnumPlayerState.Dead:
                    Enter_DeadState();
                    break;
                default:
                    break;
            }
        }

        #region IDLE

        protected virtual void Enter_IdleState()
        {
        }

        protected virtual void Update_IdleState()
        {
        }

        protected virtual void Exit_IdleState()
        {
        }

        #endregion

        #region WALK

        protected virtual void Enter_WalkState()
        {
        }

        protected virtual void Update_WalkState()
        {
        }

        protected virtual void Exit_WalkState()
        {
        }

        #endregion

        #region ATTACK

        protected virtual void Enter_AttackState()
        {
        }

        protected virtual void Update_AttackState()
        {
        }

        protected virtual void Exit_AttackState()
        {
        }

        #endregion

        #region HIT

        protected virtual void Enter_HitState()
        {
        }

        protected virtual void Update_HitState()
        {
        }

        protected virtual void Exit_HitState()
        {
        }

        #endregion

        #region DEAD

        protected virtual void Enter_DeadState()
        {
        }

        protected virtual void Update_DeadState()
        {
        }

        protected virtual void Exit_DeadState()
        {
        }

        #endregion
    }
}