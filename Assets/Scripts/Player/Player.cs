using System;
using Spine.Unity;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using DG.Tweening;
using UniRx.Triggers;
using UnityEngine.PlayerLoop;
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
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(normalAttackProjectile.ToStringEx(),
                                                                                    true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }

        private void Movement()
        {
            var hor = Input.GetAxisRaw("Horizontal");
            var ver = Input.GetAxisRaw("Vertical");

            rigid.velocity = new Vector2(hor, ver) * moveSpeed;

            if (isPlayerInBattle)
                return;

            if (rigid.velocity != Vector2.zero)
                SwitchingPlayerDirection(rigid.velocity.x < 0 ? true : false);
        }

        private void SwitchingPlayerDirection(bool isSwitching)
        {
            // Default Direction is Right
            // isSwitching : true -> Left, false -> Right
            var lossyScale = tr.lossyScale;
            tr.localScale = isSwitching switch
            {
                true  => new Vector3(-2f, lossyScale.y, lossyScale.z),
                false => new Vector3(2f, lossyScale.y, lossyScale.z)
            };
        }

        private void NormalAttack()
        {
            Observable.Interval(TimeSpan.FromSeconds(attackDelay))
                      .Where(_ => isPlayerInBattle)
                      .Subscribe(_ =>
                      {
                          var targetMonster = FindNearestMonster();
                          if (targetMonster == null) return;

                          var a = (tr.position - targetMonster.position).normalized;
                          if (a != Vector3.zero)
                              SwitchingPlayerDirection(a.x > 0 ? true : false);

                          var direction = (targetMonster.position - projectilePoint.position).normalized;
                          var spawnObj =
                              ObjectPoolManager.Instance.Spawn(normalAttackProjectile.ToStringEx(),
                                                               projectilePoint.position);
                          var projectile = spawnObj.GetComponent<PlayerProjectile>();
                          projectile.SetStat(10f, projectileSpeed, direction);
                          projectile.Fire();
                          
                          hasFiredProjectile = true;
                      }).AddTo(this);

            Transform FindNearestMonster()
            {
                var monsters =
                    Physics2D.OverlapCircleAll(Tr.position, attackRange, 1 << LayerMask.NameToLayer("Monster"));

                if (monsters.Length <= 0)
                    return null;

                var target = monsters[0].transform;
                var shortestDistance = Vector2.Distance(Tr.position, target.position);

                for (int i = 1; i < monsters.Length; i++)
                {
                    var distance = Vector2.Distance(Tr.position, monsters[i].transform.position);

                    if (distance < shortestDistance)
                        target = monsters[i].transform;

                    i++;
                }

                return target;
            }
        }

        private void Hit()
        {
            // 몬스터와 닿았을 때 Hit 판정?
            // 몬스터 Projectile과 충돌
            // 대미지 계산 후 HP 감소
            
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

        public void FinishAttack()
        {
            hasFiredProjectile = false;
        }

        // TODO : Stat 만들고 이거 완성해
        public bool IsDead()
        {
            return false;
        }
    }
}