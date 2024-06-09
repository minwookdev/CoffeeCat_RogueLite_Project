using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Events;
using ResourceManager = CoffeeCat.FrameWork.ResourceManager;

namespace CoffeeCat
{
    public partial class Player : MonoBehaviour
    {
        [Title("Status")]
        [SerializeField] protected PlayerAddressablesKey playerName = PlayerAddressablesKey.NONE;

        [SerializeField] protected PlayerAddressablesKey normalAttackProjectile = PlayerAddressablesKey.NONE;
        [ShowInInspector, ReadOnly] protected PlayerStat stat;

        [Title("Transform")]
        [SerializeField] protected Transform tr = null;

        [SerializeField] protected Transform projectileTr = null;

        private UnityEvent OnPlayerDead = new UnityEvent();
        private PlayerActiveSkill normalAttackData = null;
        private Rigidbody2D rigid = null;
        private bool isPlayerInBattle = false;
        private bool hasFiredProjectile = false;
        private bool isPlayerDamaged = false;
        private bool isInvincible = false;
        private bool isDead = false;

        // 임시
        private int maxExp = 50;
        private int currentExp = 0;

        // Property
        public Transform Tr => tr;
        public PlayerStat Stat => stat;

        private void Start()
        {
            rigid = GetComponent<Rigidbody2D>();
            normalAttackData = DataManager.Instance.PlayerActiveSkills.DataDictionary[(int)normalAttackProjectile];

            // LoadResources();
            // Movement();
            SetStat();
            NormalAttack();
            CheckInvincibleTime();

            StageManager.Instance.AddListenerRoomFirstEnteringEvent(PlayerEnteredRoom);
            StageManager.Instance.AddListenerClearedRoomEvent(PlayerClearedRoom);
        }

        private void Update()
        {
            // test
            if (Input.GetKeyDown(KeyCode.O))
            {
                EnableSkillSelect();
                // UpdateStat();
            }
        }

        private void LoadResources()
        {
            // TODO: Use AddressablesAsyncLoad Method !
            
            /*var obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(normalAttackProjectile.ToStringEx(), true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj));*/

            // LevelUp Effect : 임시
            /*obj = ResourceManager.Instance.AddressablesSyncLoad<GameObject>("LevelUp", true);
            ObjectPoolManager.Instance.AddToPool(PoolInformation.New(obj, initSpawnCount: 1));*/
        }

        private void SetStat()
        {
            stat = DataManager.Instance.PlayerStats.DataDictionary[playerName.ToStringEx()];
            stat.SetCurrentHp();
            UIPresenter.Instance.UpdatePlayerHPSlider(stat.CurrentHp, stat.MaxHp);
        }

        private void Movement()
        {
#if UNITY_STANDALONE   
            this.UpdateAsObservable()
                .Skip(TimeSpan.Zero)
                .Where(_ => !isDead)
                .Subscribe(_ =>
                {
                    var hor = Input.GetAxisRaw("Horizontal");
                    var ver = Input.GetAxisRaw("Vertical");

                    rigid.velocity = new Vector2(hor, ver) * stat.MoveSpeed;

                    if (isPlayerInBattle) return;

                    if (hor != 0 || ver != 0)
                        SwitchingPlayerDirection(rigid.velocity.x < 0 ? true : false);
                    
                }).AddTo(this);
#endif
        }

        public void Move(Vector2 direction) 
        {
#if UNITY_ANDROID
            if (isDead)
                return;
            
            rigid.velocity = new Vector2(direction.x, direction.y) * stat.MoveSpeed;
            
            if (isPlayerInBattle) 
                return;

            if (direction.x != 0f || direction.y != 0f) 
            {
                SwitchingPlayerDirection(rigid.velocity.x < 0);    
            }
#endif
        }

        public void ClearMove() {
            rigid.velocity = Vector2.zero;
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
            var currentCoolTime = normalAttackData.SkillCoolTime;

            this.UpdateAsObservable()
                .Skip(TimeSpan.Zero)
                .Where(_ => isPlayerInBattle && !isDead)
                .Select(_ => currentCoolTime += Time.deltaTime)
                .Where(_ => currentCoolTime >= normalAttackData.SkillCoolTime)
                .Subscribe(_ =>
                {
                    var targetMonster = FindNearestMonster();

                    if (targetMonster == null) return;

                    // Battle 중에는 Player의 방향을 기본공격의 타겟 방향으로 전환
                    var targetMonsterStatus = targetMonster.GetComponent<MonsterStatus>();
                    var targetDirection = targetMonsterStatus.GetCenterTr().position - projectileTr.position;
                    targetDirection = targetDirection.normalized;
                    SwitchingPlayerDirection(targetDirection.x < 0 ? true : false);

                    // 기본 공격 Projectile 스폰 및 발사
                    var spawnObj =
                        ObjectPoolManager.Instance.Spawn(normalAttackProjectile.ToStringEx(), projectileTr.position);
                    var projectile = spawnObj.GetComponent<PlayerNormalProjectile>();
                    projectile.Fire(stat, normalAttackData, projectileTr.position, targetDirection);

                    // 쿨타임 초기화 및 발사 여부
                    currentCoolTime = 0;
                    hasFiredProjectile = true;
                }).AddTo(this);

            Transform FindNearestMonster()
            {
                Collider2D[] result = new Collider2D[Defines.SPAWN_MONSTER_MAX_COUNT];
                var count = Physics2D.OverlapCircleNonAlloc
                    (Tr.position, normalAttackData.SkillRange, result, 1 << LayerMask.NameToLayer("Monster"));

                if (count <= 0) return null;

                var target = result.Where(Collider2D => Collider2D != null)
                                   .Select(Collider2D => Collider2D.GetComponent<MonsterStatus>())
                                   .Where(monster => monster.IsAlive)
                                   .OrderBy(monster => Vector2.Distance(projectileTr.position,
                                                                        monster.GetCenterTr().position))
                                   .FirstOrDefault();

                return target == null ? null : target.transform;
            }
        }

        private void CheckInvincibleTime()
        {
            this.ObserveEveryValueChanged(_ => isPlayerDamaged)
                .Skip(TimeSpan.Zero)
                .Where(_ => isPlayerDamaged)
                .Subscribe(_ =>
                {
                    isInvincible = true;
                    Observable.Timer(TimeSpan.FromSeconds(stat.InvincibleTime))
                              .Subscribe(__ => isInvincible = false);
                }).AddTo(this);
        }

        private void OnDead()
        {
            rigid.velocity = Vector2.zero;
            OnPlayerDead.Invoke();
            isDead = true;
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

        public void UpgradeNormalAttack()
        {
            var index = normalAttackData.Index + 1;
            normalAttackData = DataManager.Instance.PlayerActiveSkills.DataDictionary[index];
        }

        public void OnDamaged(DamageData damageData)
        {
            var calculatedDamage = damageData.CalculatedDamage;
            stat.CurrentHp -= calculatedDamage;
            DamageTextManager.Instance.OnFloatingText(calculatedDamage, tr.position, true);
            isPlayerDamaged = true;

            if (stat.CurrentHp <= 0)
            {
                stat.CurrentHp = 0;
                OnDead();
            }
            
            UIPresenter.Instance.UpdatePlayerHPSlider(stat.CurrentHp, stat.MaxHp);
        }

        public void AddListenerPlayerDeadEvent(UnityAction action)
        {
            OnPlayerDead.AddListener(action);
        }

        #endregion
    }
}