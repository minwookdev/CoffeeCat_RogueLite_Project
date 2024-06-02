using System;
using System.Linq;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
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
        // TODO : 영구강화 된 스탯 / 던전 일시 강화 스탯

        [Title("Stat")]
        [ShowInInspector, ReadOnly] protected PlayerStat stat;

        [ShowInInspector, ReadOnly] private PlayerActiveSkill normalAttackData = null;
        [SerializeField] protected PlayerAddressablesKey playerName = PlayerAddressablesKey.NONE;
        [SerializeField] protected PlayerAddressablesKey normalAttackProjectile = PlayerAddressablesKey.NONE;

        [Title("Transform")]
        [SerializeField] protected Transform tr = null;

        [SerializeField] protected Transform projectilePoint = null;

        private Rigidbody2D rigid = null;
        private bool isPlayerInBattle = false;
        private bool hasFiredProjectile = false;
        private bool isPlayerDamaged = false;
        private bool isInvincible = false;
        private bool isDead = false;

        // 임시
        private int maxExp = 50;
        private int currentExp = 0;

        public Transform Tr => tr;
        public PlayerStat Stat => stat;

        private void Start()
        {
            rigid = GetComponent<Rigidbody2D>();
            normalAttackData = DataManager.Instance.PlayerActiveSkills.DataDictionary[(int)normalAttackProjectile];
            LoadResources();
            SetStat();
            NormalAttack();
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
                UpdateStat();
            }
        }

        private void LoadResources()
        {
            var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(normalAttackProjectile.ToStringEx(),
                                                                                    true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));
            
            // LevelUp Effect : 임시
            obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>("LevelUp", true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj, initSpawnCount: 1));
        }

        private void SetStat()
        {
            stat = DataManager.Instance.PlayerStats.DataDictionary[playerName.ToStringEx()];
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

        private void NormalAttack()
        {
            float currentCoolTime = normalAttackData.SkillCoolTime;

            Observable.EveryUpdate()
                      .Where(_ => isPlayerInBattle && !isDead)
                      .Select(_ => currentCoolTime += Time.deltaTime)
                      .Where(_ => currentCoolTime >= normalAttackData.SkillCoolTime)
                      .Subscribe(_ =>
                      {
                          var targetMonster = FindNearestMonster();

                          if (targetMonster == null) return;

                          var targetMonsterStatus = targetMonster.GetComponent<MonsterStatus>();
                          var targetDirection = (targetMonsterStatus.GetCenterPosition() - projectilePoint.position)
                              .normalized;
                          SwitchingPlayerDirection(targetDirection.x < 0 ? true : false);

                          var spawnObj =
                              ObjectPoolManager.Instance.Spawn(normalAttackProjectile.ToStringEx(),
                                                               projectilePoint.position);
                          var projectile = spawnObj.GetComponent<PlayerNormalProjectile>();
                          projectile.Fire(stat, normalAttackData.ProjectileSpeed, projectilePoint.position,
                                          targetDirection);

                          currentCoolTime = 0;
                          hasFiredProjectile = true;
                      }).AddTo(this);

            Transform FindNearestMonster()
            {
                var monsters =
                    Physics2D.OverlapCircleAll(Tr.position, normalAttackData.SkillRange,
                                               1 << LayerMask.NameToLayer("Monster"));

                if (monsters.Length <= 0)
                    return null;

                var target = monsters
                             .Select(monster => monster.transform)
                             .Where(monster => monster.GetComponent<MonsterState>().State !=
                                               MonsterState.EnumMonsterState.Death)
                             .OrderBy(monster => Vector2.Distance
                                          (projectilePoint.position, monster.position))
                             .FirstOrDefault();

                return target;
            }
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
                    Observable.Timer(TimeSpan.FromSeconds(stat.InvincibleTime))
                              .Subscribe(__ => isInvincible = false);
                }).AddTo(this);
        }

        private void OnDead()
        {
            isDead = true;
            rigid.velocity = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isInvincible)
                return;

            // Monster와 충돌
            if (other.gameObject.TryGetComponent(out MonsterStatus monsterStat))
            {
                var damageData = DamageData.GetData(monsterStat.CurrentStat, stat);
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
                    GetExp();
                    break;
                case RoomType.BossRoom:
                    break;
            }
        }

        private void GetExp()
        {
            var exp = StageManager.Instance.CurrentRoomMonsterKilledCount * 5;
            currentExp += exp;

            if (currentExp >= maxExp)
            {
                maxExp += 50;

                var levelUpEffect = ObjectPoolManager.Instance.Spawn("LevelUp", tr);
                levelUpEffect.transform.localPosition = Vector3.zero;
                Observable.Timer(TimeSpan.FromSeconds(2.5f))
                          .Subscribe(_ => { EnableSkillSelect(); }).AddTo(this);
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

        public void UpdateStat()
        {
            // UI 에서 해줄 것
            /// 사용자의 입력을 받아서 enhanceData 객체 생성
            /// var enhance = new PlayerEnhanceData();
            /// enhance.MaxHp = 100;
            /// 만들어진 객체를 enhanceData.SaveEnhanceData()로 저장
            /// enhance.SaveEnhanceData();
            /// 사용자가 (스탯 포인트 등을 가지고) 강화를 확정하면 UpdateStat() 호출

            var enhanceData = PlayerEnhanceData.GetEnhanceData();
            stat.StatEnhancement(enhanceData);
        }

        public void OnDamaged(DamageData damageData)
        {
            var calculatedDamage = damageData.CalculatedDamage;
            stat.CurrentHp -= calculatedDamage;
            DamageTextManager.Instance.OnFloatingText(calculatedDamage, tr.position, false);
            isPlayerDamaged = true;

            if (stat.CurrentHp <= 0)
            {
                stat.CurrentHp = 0;
                OnDead();
            }
        }

        public void UpgradeNormalAttack()
        {
            var index = normalAttackData.Index + 1;
            normalAttackData = DataManager.Instance.PlayerActiveSkills.DataDictionary[index];
        }

        #endregion
    }
}