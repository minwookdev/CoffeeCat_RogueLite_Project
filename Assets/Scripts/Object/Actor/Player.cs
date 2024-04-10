using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;

namespace CoffeeCat
{
    public enum PlayerState
    {
        Idle = 0,
        Walk,
        Attack,
        Hit,
        Dead
    }

    public class Player : MonoBehaviour {
        [Title("Movement")]
        [SerializeField] private Transform tr = null;
        [SerializeField] private float moveSpeed = 0;
        
        private SkeletonAnimation anim = null;
        private Rigidbody2D rigid = null;
        private PlayerState state = PlayerState.Idle;
        
        // Properties
        public Transform Tr => tr;
        public PlayerState State => state;

        private void Start()
        {
            anim = GetComponent<SkeletonAnimation>();
            rigid = GetComponent<Rigidbody2D>();

            UpdateState();
        }

        private void Update()
        {
            Movement();
        }

        private void Movement()
        {
            var hor = Input.GetAxisRaw("Horizontal");
            var ver = Input.GetAxisRaw("Vertical");

            rigid.velocity = new Vector3(hor, ver) * moveSpeed;

            if (rigid.velocity.x > 0)
                transform.localScale = new Vector3(2f, transform.lossyScale.y, transform.lossyScale.z);
            else if (rigid.velocity.x < 0)
                transform.localScale = new Vector3(-2f, transform.lossyScale.y, transform.lossyScale.z);

            if (rigid.velocity.x != 0 || rigid.velocity.y != 0)
                state = PlayerState.Walk;
            else if (rigid.velocity.x == 0 && rigid.velocity.y == 0)
                state = PlayerState.Idle;
        }

        private void UpdateState()
        {
            anim.state.SetAnimation(0, "Idle_1", true);

            this.ObserveEveryValueChanged(_ => state)
                .Skip(System.TimeSpan.Zero)
                .Subscribe(state =>
                {
                    switch (state)
                    {
                        case PlayerState.Idle:
                            anim.state.SetAnimation(0, "Idle_2", true);
                            break;
                        case PlayerState.Walk:
                            anim.state.SetAnimation(0, "Walk_NoHand", true);
                            break;
                        case PlayerState.Attack:
                            break;
                    }
                });
        }
    }
}
