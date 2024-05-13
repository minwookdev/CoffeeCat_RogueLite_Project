using System;
using Spine.Unity;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using DG.Tweening;
using ResourceManager = CoffeeCat.FrameWork.ResourceManager;

namespace CoffeeCat
{
    public class Player : MonoBehaviour
    {
        [Title("Movement")]
        [SerializeField] protected Transform tr = null;
        [SerializeField] protected float moveSpeed = 0;

        [Title("Attack")]
        [SerializeField] protected Transform projectilePoint = null;
        [SerializeField] protected PlayerAddressablesKey normalAttackProjectile = PlayerAddressablesKey.NONE;
        [SerializeField] protected float attackDelay = 0.5f;
        [SerializeField] protected float attackRange = 3.5f;
        [SerializeField] protected float projectileSpeed = 5.0f;

        private bool isPlayerInBattle = false;
        private bool hasFiredProjectile = false;
        private Rigidbody2D rigid = null;

        // Properties
        public Transform Tr => tr;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            Movement();
        }

        private void Initialize()
        {
            rigid = GetComponent<Rigidbody2D>();
            
            LoadResources();
            NormalAttack();
            
            StageManager.Instance.AddListenerRoomEnteringEvent(PlayerEnteredRoom);
            StageManager.Instance.AddListenerClearedRoomEvent(PlayerClearedRoom);
        }

        private void LoadResources()
        {
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(normalAttackProjectile.ToStringEx(), true);
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
        }

        private void NormalAttack()
        {
            Observable.Interval(TimeSpan.FromSeconds(attackDelay))
                      .Where(_ => isPlayerInBattle)
                      .Subscribe(_ =>
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
                          
                          var direction = (target.position - projectilePoint.position).normalized;
                          var projectile = ObjectPoolManager.Instance.Spawn(normalAttackProjectile.ToStringEx(), projectilePoint.position);

                          // TODO : 딜레이 주고 느낌 보기
                          hasFiredProjectile = true;
                          projectile.transform.DOMove(direction * 10f, projectileSpeed)
                                    .SetRelative().SetSpeedBased().SetEase(Ease.Linear)
                                    .OnStart(() =>
                                    {
                                        hasFiredProjectile = false;
                                    })
                                    .OnComplete(() =>
                                    {
                                        ObjectPoolManager.Instance.Despawn(projectile);
                                    });
                      }).AddTo(this);
        }

        /*private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(Tr.position, 3.0f);
        }*/

        private void PlayerEnteredRoom(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.PlayerSpawnRoom:
                    break;
                case RoomType.MonsterSpawnRoom:
                    isPlayerInBattle = true;
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
                    isPlayerInBattle = false;
                    break;
                case RoomType.BossRoom:
                    break;
            }
        }

        public bool IsWalking()
        {
            return rigid.velocity.x != 0 || rigid.velocity.y != 0;
        }

        public bool IsAttacking()
        {
            return hasFiredProjectile;
        }

        // TODO : Stat 만들고 이거 완성해
        public bool IsDead()
        {
            return false;
        }
    }
}