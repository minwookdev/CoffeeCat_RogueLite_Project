using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using CoffeeCat.Utils;
using UnityObject = UnityEngine.Object;

namespace CoffeeCat.FrameWork {
    public static class SafeLoader {
        private static readonly Queue<Action> requestQueue = new();
        private static bool IsProcessing = false;
        
        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StartProcess(GameObject bindingObject) {
            if (IsProcessing) {
                return;
            }
            
            // TODO: 한번에 처리되는 작업의 양 지정

            // Main Process Observable Start
            IsProcessing = true;
            Observable.EveryUpdate()
                      .Skip(TimeSpan.Zero)
                      .TakeWhile(_ => IsProcessing)
                      //.TakeUntilDestroy(bindingObject)
                      .Where(_ => requestQueue.Count > 0)
                      .Select(_ => requestQueue.Dequeue())
                      .Subscribe(request => {
                          // ~ per 1 Frame
                          request.Invoke();
                      })
                      .AddTo(bindingObject);
        }

        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StopProcess() {
            IsProcessing = false;
            int processLeft = requestQueue.Count;
            if (processLeft > 0) {
                CatLog.WLog($"Count Of UnProcessed: {processLeft.ToString()}");
            }
            requestQueue.Clear();
        }

        public static void RequestLoad<T>(string key, bool isGlobalResource = false, Action<T> onCompleted = null) where T: UnityObject {
            requestQueue.Enqueue(Request);
            return;

            void Request() {
                ResourceManager.Inst.AddressablesAsyncLoad<T>(key, isGlobalResource, (loadedResource) => {
                    onCompleted?.Invoke(loadedResource);
                });
            }
        }

        public static void RequestRegist(string key, Action<bool> onCompleted = null, int spawnCount = PoolInformation.DEFAULT_SPAWN_COUNT) {
            requestQueue.Enqueue(Request);
            return;

            void Request() {
                if (ObjectPoolManager.Inst.IsExistInPoolDictionary(key)) {
                    // CatLog.WLog($"{key} is Already Containing in Pool Dictionary.");
                    onCompleted?.Invoke(true);
                    return;
                }
                
                ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(key, false, (loadedGameObject) => {
                    if (!loadedGameObject) {
                        onCompleted?.Invoke(false);
                        return;
                    }
                    ObjectPoolManager.Inst.AddToPool(PoolInformation.Create(loadedGameObject, initSpawnCount: spawnCount));
                    onCompleted?.Invoke(true);
                });
            }
        }
    }
}