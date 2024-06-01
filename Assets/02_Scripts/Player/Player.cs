using System;
using System.Linq;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;
using ResourceManager = CoffeeCat.FrameWork.ResourceManager;

namespace CoffeeCat
{
    public partial class Player : MonoBehaviour
    {
        [Title("Status")]
        [SerializeField] protected PlayerStatusKey playerName = PlayerStatusKey.NONE;
        [ShowInInspector, ReadOnly] protected PlayerStat stat;

        [Title("Attack")]
        [SerializeField] protected PlayerAddressablesKey normalAttackProjectile = PlayerAddressablesKey.NONE;

        [Title("Transform")]
        [SerializeField] protected Transform tr = null;

        [SerializeField] protected Transform projectilePoint = null;

        private Rigidbody2D rigid = null;
        [ShowInInspector]private bool isPlayerInBattle = false;
        private bool hasFiredProjectile = false;
        private bool isPlayerDamaged = false;
        private bool isInvincible = false;
        private bool isDead = false;

        public Transform Tr => tr;
        public PlayerStat Stat => stat;

        private void Start()
        {
            rigid = GetComponent<Rigidbody2D>();
            LoadResources();
            SetStatus();
            // NormalAttack();
            CheckInvincibleTime();

            StageManager.Instance.AddListenerRoomFirstEnteringEvent(PlayerEnteredRoom);
            StageManager.Instance.AddListenerClearedRoomEvent(PlayerClearedRoom);
        }

        private void Update()
        {
            Movement();

            // test
            if (Input.GetKeyDown(KeyCode.O))
            {
                // EnableSkillSelect();
            }
        }

        private void LoadResources()
        {
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(normalAttackProjectile.ToStringEx(),
                                                                                    true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
        }

        private void SetStatus()
        {
            stat = DataManager.Instance.PlayerStats.DataDictionary[0];
            stat.Initialize();
        }

        private void Movement()
        {
            if (isDead)
                return;

            var hor = Input.GetAxisRaw("Horizontal");
            var ver = Input.GetAxisRaw("Vertical");

            rigid.velocity = new Vector2(hor, ver) * stat.MoveSpeed;

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

        /*private void NormalAttack()
        {
            Observable.Interval(TimeSpan.FromSeconds(stat.AttackDelay))
                      .Where(_ => !isDead)
                      .Where(_ => isPlayerInBattle)
                      .Subscribe(_ =>
                      {
                          var targetMonster = FindNearestMonster();

                          if (targetMonster == null) return;

                          var targetMonsterStatus = targetMonster.GetComponent<MonsterStatus>();
                          var targetDirection = (targetMonsterStatus.GetCenterPosition() - projectilePoint.position).normalized;
                          SwitchingPlayerDirection(targetDirection.x < 0 ? true : false);

                          var spawnObj =
                              ObjectPoolManager.Instance.Spawn(normalAttackProjectile.ToStringEx(),
                                                               projectilePoint.position);
                          var projectile = spawnObj.GetComponent<PlayerNormalProjectile>();
                          projectile.Fire(status, projectilePoint.position, targetDirection);

                          hasFiredProjectile = true;
                      }).AddTo(this);

            Transform FindNearestMonster()
            {
                var monsters =
                    Physics2D.OverlapCircleAll(Tr.position, status.AttackRange, 1 << LayerMask.NameToLayer("Monster"));

                if (monsters.Length <= 0)
                    return null;

                var target = monsters
                             .Select(monster => monster.transform)
                             .Where(monster => monster.GetComponent<MonsterState>().State != MonsterState.EnumMonsterState.Death)
                             .OrderBy(monster => Vector2.Distance
                                          (projectilePoint.position, monster.position))
                             .FirstOrDefault();

                return target;
            }
        }*/
        
        private void CheckInvincibleTime()
        {
            this.ObserveEveryValueChanged(_ => isPlayerDamaged)
                .Skip(TimeSpan.Zero)
                .Where(_ => isPlayerDamaged)
                .Select(_ => isInvincible)
                .Subscribe(_ =>
                {
                    isInvincible = true;
                    Observable.Timer(TimeSpan.FromSeconds(stat.InvincibleTime))
                              .Subscribe(__ => isInvincible = false);
                }).AddTo(this);
        }

        private void OnDamaged(DamageData damageData)
        {
            if (isInvincible)
                return;

            isPlayerDamaged = true;
            stat.CurrentHp -= damageData.CalculatedDamage;

            if (stat.CurrentHp <= 0)
            {
                stat.CurrentHp = 0;
                OnDead();
            }
        }

        private void OnDead()
        {
            isDead = true;
            rigid.velocity = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // TODO : 몬스터 대시 스킬 발동 시 충돌

            // Monster와 충돌
            if (other.gameObject.TryGetComponent(out MonsterStatus monsterStat))
            {
                var damageData = DamageData.GetData(monsterStat.CurrentStat, stat);
                /*CatLog.Log($"Player_OnTriggerEnter2D_{monsterStat.name} : {damageData.CalculatedDamage.ToString()}");*/
                OnDamaged(damageData);
            }
        }

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

        #region Public Methods

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

        public bool IsDead()
        {
            return isDead;
        }

        #endregion
    }
}