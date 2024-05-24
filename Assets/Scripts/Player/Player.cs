using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
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
        [SerializeField] protected PlayerStatusKey playerName = PlayerStatusKey.NONE;
        [ShowInInspector, ReadOnly] protected PlayerStatus status;

        [Title("Attack")]
        [SerializeField] protected PlayerAddressablesKey normalAttackProjectile = PlayerAddressablesKey.NONE;

        [Title("Transform")]
        [SerializeField] protected Transform tr = null;

        [SerializeField] protected Transform projectilePoint = null;

        private PlayerSkillsKey skill = PlayerSkillsKey.NONE;
        private Rigidbody2D rigid = null;
        private bool isPlayerInBattle = false;
        private bool hasFiredProjectile = false;
        private bool isPlayerDamaged = false;
        private bool isInvincible = false;
        private bool isDead = false;

        public Transform Tr => tr;
        public PlayerStatus Status => status;

        public PlayerSkillsKey Skill
        {
            set => skill = value;
        }

        private void Start()
        {
            rigid = GetComponent<Rigidbody2D>();
            Initialize();
        }

        private void Update()
        {
            Movement();

            // test
            if (Input.GetKeyDown(KeyCode.O))
            {
                ObjectPoolManager.Instance.Spawn("testSkill", tr.position);
            }
        }

        private void Initialize()
        {
            SetStatus();
            LoadResources();
            NormalAttack();

            // test
            CheckInvincibleTime();
            GetSkilled();

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
            status = new PlayerStatus(DataManager.Instance.playerStatus[(int)playerName]);
        }

        private void Movement()
        {
            if (isDead)
                return;

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
                      .Where(_ => !isDead)
                      .Where(_ => isPlayerInBattle)
                      .Subscribe(_ =>
                      {
                          var targetMonster = FindNearestMonster();
                          if (targetMonster == null) return;

                          var a = (tr.position - targetMonster.position).normalized;
                          if (a != Vector3.zero)
                              SwitchingPlayerDirection(a.x > 0 ? true : false);

                          if (targetMonster.TryGetComponent(out MonsterState state)) {
                              targetMonster = state.CenterPointTr;
                          }
                          var direction = (targetMonster.position - projectilePoint.position).normalized;
                          var spawnObj =
                              ObjectPoolManager.Instance.Spawn(normalAttackProjectile.ToStringEx(),
                                                               projectilePoint.position);
                          spawnObj.TryGetComponent(out PlayerNormalProjectile projectile);
                          projectile.AttackData = new ProjectileDamageData(status);
                          projectile.Fire(direction, status.ProjectileSpeed, projectilePoint.position);

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

        private void GetSkilled()
        {
            this.ObserveEveryValueChanged(_ => skill)
                .Where(_ => skill != null)
                .Skip(TimeSpan.Zero)
                .Subscribe(_ =>
                {
                    // 스킬 슬롯 설정
                    SkillAttack();
                });
        }

        private void SkillAttack()
        {
            if (isDead || !isPlayerInBattle)
                return;
            
            
        }

        private void OnDamaged(DamageData damageData)
        {
            if (isInvincible)
                return;

            isPlayerDamaged = true;
            status.CurrentHp -= damageData.CalculatedDamage;

            if (status.CurrentHp <= 0)
            {
                status.CurrentHp = 0;
                OnDead();
            }
        }

        private void OnDead()
        {
            isDead = true;
            rigid.velocity = Vector2.zero;
        }

        private void CheckInvincibleTime()
        {
            this.ObserveEveryValueChanged(_ => isPlayerDamaged)
                .Skip(TimeSpan.Zero)
                .Where(_ => isPlayerDamaged)
                .Select(_ => isInvincible)
                .Subscribe(_ =>
                {
                    isInvincible = true;
                    Observable.Timer(TimeSpan.FromSeconds(status.InvincibleTime))
                              .Subscribe(__ => isInvincible = false);
                }).AddTo(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // TODO : 몬스터 대시 스킬 발동 시 충돌

            // Monster와 충돌
            if (other.gameObject.TryGetComponent(out MonsterStatus monsterStat))
            {
                var damageData = DamageData.GetData(monsterStat.CurrentStat, status);
                /*CatLog.Log($"Player_OnTriggerEnter2D_{monsterStat.name} : {damageData.CalculatedDamage.ToString()}");*/
                OnDamaged(damageData);
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

        public void FinishAttackAnimation()
        {
            hasFiredProjectile = false;
        }

        public bool IsDamaged()
        {
            return isPlayerDamaged;
        }

        public void FinishHitAnimation()
        {
            isPlayerDamaged = false;
        }

        // TODO : Stat 만들고 이거 완성해
        public bool IsDead()
        {
            return isDead;
        }

        public void UpdateSkill(int index) {
            // -1 index is Invalid
            if (index == -1) {
                return;
            }
        }
    }
}