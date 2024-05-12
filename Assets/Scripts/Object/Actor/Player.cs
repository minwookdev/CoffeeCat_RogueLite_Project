using System;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.ResourceManagement;
using ResourceManager = CoffeeCat.FrameWork.ResourceManager;

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

    public class Player : MonoBehaviour
    {
        [Title("Movement")]
        [SerializeField] private Transform tr = null;

        [SerializeField] private float moveSpeed = 0;

        [Title("Attack")]
        [SerializeField] private Transform projectilePoint = null;

        private float attackDelay = 0.5f;
        private float attackRange = 3.5f;
        private float projectileSpeed = 5.0f;

        private bool isAttack = false;
        private SkeletonAnimation anim = null;
        private Rigidbody2D rigid = null;
        private PlayerState state = PlayerState.Idle;

        private readonly string normalAttackProjectile = "NormalAttack";

        // Properties
        public Transform Tr => tr;
        public PlayerState State => state;

        private void Start()
        {
            anim = GetComponent<SkeletonAnimation>();
            rigid = GetComponent<Rigidbody2D>();

            Initialize();
            UpdateState();
        }

        private void Update()
        {
            Movement();
            
            // test
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var monsters =
                    Physics2D.OverlapCircleAll(Tr.position, attackRange, 1 << LayerMask.NameToLayer("Monster"));

                if (monsters.Length <= 0)
                    return;
                
                var target = monsters[0].transform;
                var shortestDistance = Vector2.Distance(Tr.position, target.position);

                for (int i = 1; i < monsters.Length; i++)
                {
                    var distance = Vector2.Distance(Tr.position, monsters[i].transform.position);
                        
                    if (distance < shortestDistance)
                        target = monsters[i].transform;

                    i++;
                }
                    
                // 방향 정하기
                var direction = (target.position - projectilePoint.position).normalized;
                    
                var projectile = ObjectPoolManager.Instance.Spawn(normalAttackProjectile, projectilePoint.position);
                projectile.transform.DOMove(direction * 10f, projectileSpeed)
                          .SetRelative().SetSpeedBased().SetEase(Ease.OutSine);
            }
        }

        private void Initialize()
        {
            LoadResources();
            // Attack();
            StageManager.Instance.AddListenerRoomEnteringEvent(PlayerEnteredRoom);
            StageManager.Instance.AddListenerClearedRoomEvent(PlayerClearedRoom);
        }

        private void LoadResources()
        {
            // 리소스 로드
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(normalAttackProjectile, true);
            // 리소스 풀에 등록
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
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

        private void Attack()
        {
            this.ObserveEveryValueChanged(_ => isAttack)
                .Skip(TimeSpan.Zero)
                .Where(_ => isAttack)
                .Subscribe(_ =>
                {
                    // TODO : 공격 딜레이
                    
                    var monsters =
                        Physics2D.OverlapCircleAll(Tr.position, attackRange, 1 << LayerMask.NameToLayer("Monster"));

                    if (monsters.Length <= 0)
                        return;
                    
                    var target = monsters[0].transform;
                    var shortestDistance = Vector2.Distance(Tr.position, target.position);

                    for (int i = 1; i < monsters.Length; i++)
                    {
                        var distance = Vector2.Distance(Tr.position, monsters[i].transform.position);
                        
                        if (distance < shortestDistance)
                            target = monsters[i].transform;

                        i++;
                    }
                    
                    var direction = (target.position - projectilePoint.position).normalized;
                    
                    var projectile = ObjectPoolManager.Instance.Spawn(normalAttackProjectile, projectilePoint.position);
                    projectile.transform.DOMove(direction * 10f, projectileSpeed)
                              .SetRelative().SetSpeedBased().SetEase(Ease.Linear);
                });
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(Tr.position, 3.0f);
        }

        private void PlayerEnteredRoom(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.PlayerSpawnRoom:
                    break;
                case RoomType.MonsterSpawnRoom:
                    isAttack = true;
                    break;
                case RoomType.ShopRoom:
                    break;
                case RoomType.BossRoom:
                    break;
                case RoomType.RewardRoom:
                    break;
                case RoomType.EmptyRoom:
                    break;
                case RoomType.ExitRoom:
                    break;
            }
        }

        private void PlayerClearedRoom(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.MonsterSpawnRoom:
                    isAttack = false;
                    break;
                case RoomType.BossRoom:
                    break;
            }
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