using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using CoffeeCat.Utils;
using UnityObject = UnityEngine.Object;

namespace CoffeeCat.FrameWork {
    public static class SafeLoader {
        private static readonly Queue<Action> processQueue = new();
        private static bool IsProcessing = false;
        private const int maxProcessPerFrame = 5;
        
        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StartProcess(GameObject bindingObject) {
            if (IsProcessing) {
                return;
            }
            // Main Process Observable Start
            int processPerFrame = 0;
            IsProcessing = true;
            Observable.EveryUpdate()
                      //.TakeUntilDestroy(bindingObject)
                      .Skip(TimeSpan.Zero)
                      .TakeWhile(_ => IsProcessing)
                      .Select(_ => processQueue)
                      .Where(queue => queue.Count > 0)
                      .Subscribe(queue => {
                          while (queue.Count > 0 && processPerFrame < maxProcessPerFrame)
                          {
                              var process = queue.Dequeue();
                              process.Invoke();
                              processPerFrame++;
                          }
                          // Clear Processed Count
                          processPerFrame = 0;
                      })
                      .AddTo(bindingObject);
        }

        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StopProcess() {
            IsProcessing = false;
            int processLeft = processQueue.Count;
            if (processLeft > 0) {
                CatLog.WLog($"Count Of UnProcessed: {processLeft.ToString()}");
            }

            processQueue.Clear();
        }

        public static void Request<T>(string key, bool isGlobalResource = false, Action<T> onCompleted = null) where T: UnityObject {
            processQueue.Enqueue(() => RequestLoad(key, isGlobalResource, onCompleted));
        }

        public static void Request(string key, Action<bool> onCompleted = null, int spawnCount = PoolInfo.DEFAULT_SPAWN_COUNT) {
            processQueue.Enqueue(() => RequestLoadWithRegist(key, spawnCount, onCompleted));
        }
        
        private static void RequestLoad<T>(string key, bool isGlobalResource = false, Action<T> onCompleted = null) where T : UnityObject {
            ResourceManager.Inst.AddressablesAsyncLoad<T>(key, isGlobalResource, (loadedResource) => {
                onCompleted?.Invoke(loadedResource);
            });
        }

        private static void RequestLoadWithRegist(string key, int spawnCount, Action<bool> onCompleted = null) {
            if (ObjectPoolManager.Inst.IsExistInPool(key)) {
                // CatLog.WLog($"{key} is Already Containing in Pool Dictionary.");
                onCompleted?.Invoke(true);
                return;
            }

            ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(key, false, (loadedGameObject) => {
                if (!loadedGameObject) {
                    onCompleted?.Invoke(false);
                    return;
                }

                ObjectPoolManager.Inst.AddToPool(PoolInfo.Create(loadedGameObject, initSpawnCount: spawnCount));
                onCompleted?.Invoke(true);
            });
        }
    }
}