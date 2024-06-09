/// DATE: 2023. 07. 23 
/// CODER: MIN WOOK KIM
/// IMPLEMENTATION: 각각의 Monster Object와 Manager간의 소통을 위한 클래스
using System;
using System.Collections.Generic;
using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Datas;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using UniRx;
using UniRx.Triggers;

namespace CoffeeCat {
    public class MonsterManager : GenericSingleton<MonsterManager> {
        // MonsterManager를 통해 요청된 작업과 상태를 저장. 추후 다른 작업의 처리를 위해 Dictionary를 통해 관리
        private readonly Dictionary<string, Provided> providedDictionary = new Dictionary<string, Provided>();
        private readonly Queue<Action> requestQueue = new Queue<Action>();
        public bool IsProcessing = false;

        protected override void Initialize() {
            base.Initialize();
            SceneManager.Instance.OnSceneChangeBeforeEvent += OnSceneChangeBefore;

            // Request Queue Processing Observable
            this.UpdateAsObservable()
                .Skip(TimeSpan.Zero)
                .TakeUntilDisable(this)
                .Where(_ => IsProcessing && requestQueue.Count > 0)
                .Select(_ => requestQueue.Dequeue())
                .Subscribe(request => {
                    // ~per 1 frame
                    request.Invoke();
                })
                .AddTo(this);
        }

        public void StartProcess() => IsProcessing = true;

        /// <summary>
        /// 깊은 복사된 몬스터 스텟 데이터 불러옴 (DataManager를 통하기 때문에 Awake호출 금지 !)
        /// </summary>
        /// <param name="key">Monster GameObject Name or Custom Stat LoadKey</param>
        /// <returns></returns>
        public MonsterStat GetDeepCopiedMonsterStat(string key) {
            var monsterStatData = DataManager.Instance.MonsterStats;
            if (monsterStatData.DataDictionary.TryGetValue(key, out MonsterStat loadedStatData)) {
                return loadedStatData.DeepCopyMonsterStat();
            }
            else {
                CatLog.ELog($"Not Found Monster Stat. Key: {key}");
                return null;
            }
        }

        public void RequestProvide(string key, Action onCompleted = null) {
            requestQueue.Enqueue(() => Provide(key, onCompleted));
        }

        private void Provide(string key, Action onCompleted = null) {
            // 이미 요청된 작업이거나 완료된 작업
            if (providedDictionary.TryGetValue(key, out var provide)) {
                provide.IsCompleted(onCompleted);
                return;
            }
            
            // Provider가 무시된 경우
            if (ObjectPoolManager.Instance.IsExistInPoolDictionary(key)) {
                CatLog.ELog("Provider를 통하지 않고 생성된 Object는 관리 대상이 아닙니다. key : " + key);
                return;
            }
            
            // Dictionary 할당으로 작업이 요청되었음을 정의 (Unity제어권 동안 다른 오브젝트의 재요청 방지)
            providedDictionary.Add(key, new Provided());
            // ResourceManager를 통한 로드 및 ObjectPool 등록
            ResourceManager.Instance.AddressablesAsyncLoad<GameObject>(key, false, (loadedGameObject) => {
                ObjectPoolManager.Instance.AddToPool(PoolInformation.Create(loadedGameObject));
                            
                // 작업 처리 완료
                providedDictionary[key].RequestComplete();
                onCompleted?.Invoke();
            });
        }

        private void OnSceneChangeBefore(SceneName nextSceneName) {
            IsProcessing = false;
            providedDictionary.Clear();
            requestQueue.Clear();
        }

        #region PROVIDER
        private class Provided {
            public STATE State = STATE.REQUESTED;
            private Action onCompleted = default;

            public enum STATE {
                REQUESTED,
                COMPLETED,
            }
            
            public void RequestComplete() {
                State = STATE.COMPLETED;
                onCompleted?.Invoke();
            }

            public bool IsCompleted(Action action) {
                switch (State) {
                    case STATE.COMPLETED: action?.Invoke(); 
                        return true;
                    case STATE.REQUESTED: onCompleted += action; 
                        return false;
                }
                return false;
            }
        }
        #endregion
    }
}
