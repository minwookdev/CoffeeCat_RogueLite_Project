using System;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using ResourceManager = CoffeeCat.FrameWork.ResourceManager;

namespace CoffeeCat
{
    public class Player : MonoBehaviour
    {
        [Title("Status")]
        [ShowInInspector] private PlayerStatus status;

        [Title("Attack")]
        [SerializeField] protected PlayerAddressablesKey normalAttackProjectile = PlayerAddressablesKey.NONE;

        [Title("Transform")]
        [SerializeField] protected Transform tr = null;

        [SerializeField] protected Transform projectilePoint = null;

        private Rigidbody2D rigid = null;
        private bool isPlayerInBattle = false;
        private bool hasFiredProjectile = false;
        private bool isPlayerDamaged = false;

        public Transform Tr => tr;
        public PlayerStatus Status => status;

        private void Start()
        {
            rigid = GetComponent<Rigidbody2D>();
            Initialize();
        }

        private float time = 0;
        private bool isInvincible = false;

        private void Update()
        {
            Movement();

            // test
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (isInvincible)
                    return;

                isPlayerDamaged = true;
                isInvincible = true;
            }

            if (isInvincible)
            {
                time += Time.deltaTime;
                if (time >= status.InvincibleTime)
                {
                    isInvincible = false;
                    time = 0;
                }
            }
        }

        public void FinishHitAnimation()
        {
            isPlayerDamaged = false;
        }

        private void Initialize()
        {
            SetStatus();
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

        private void SetStatus()
        {
            status = new PlayerStatus(StageManager.Instance.LoadPlayerStatus(0));
        }

        private void Movement()
        {
            var hor = Input.GetAxisRaw("Horizontal");
            var ver = Input.GetAxisRaw("Vertical");

            rigid.velocity = new Vector2(hor, ver) * status.MoveSpeed;

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
            Observable.Interval(TimeSpan.FromSeconds(status.AttackDelay))
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
                          projectile.AttackData = ProjectileDamageData.GetData(status);
                          projectile.Fire(direction, status.ProjectileSpeed);

                          hasFiredProjectile = true;
                      }).AddTo(this);

            Transform FindNearestMonster()
            {
                var monsters =
                    Physics2D.OverlapCircleAll(Tr.position, status.AttackRange, 1 << LayerMask.NameToLayer("Monster"));

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

        private void Hit(DamageData damageData)
        {
            if (isPlayerDamaged)
                return;

            isPlayerDamaged = true;

            if (status.CurrentHp <= 0)
            {
                status.CurrentHp = 0;
                // Dead state로 변경
            }

            isPlayerDamaged = true;
            status.CurrentHp -= damageData.CalculatedDamage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 충돌 판정
            // 대미지를 입히는 충돌체 : 몬스터 / 몬스터 스킬, 몬스터 Projectile
            // 몬스터 : 현재 스킬 발동중이 아니라면
            // 몬스터 스킬 : 현재 스킬 발동중이라면 (대쉬 등)

            // Monster와 충돌
            if (other.gameObject.TryGetComponent(out MonsterStatus monsterStat))
            {
                // status.OnDamaged();
            }
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

        public bool IsDamaged()
        {
            return isPlayerDamaged;
        }

        // TODO : Stat 만들고 이거 완성해
        public bool IsDead()
        {
            return false;
        }
    }
}