using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using CoffeeCat.Utils;
using UnityObject = UnityEngine.Object;

namespace CoffeeCat.FrameWork {
    public static class SafeRegister {
        private static readonly Queue<Action> processQueue = new();
        private static bool IsProcessing = false;
        
        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StartProcess(GameObject bindingObject) {
            if (IsProcessing) {
                return;
            }

            // Main Process Observable Start
            IsProcessing = true;
            Observable.EveryUpdate()
                      .Skip(TimeSpan.Zero)
                      .TakeWhile(_ => IsProcessing)
                      //.TakeUntilDestroy(bindingObject)
                      .Where(_ => processQueue.Count > 0)
                      .Select(_ => processQueue.Dequeue())
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
            int processLeft = processQueue.Count;
            if (processLeft > 0) {
                CatLog.WLog($"Count Of UnProcessed: {processLeft.ToString()}");
            }
            processQueue.Clear();
            // requestDictionary.Clear();
        }

        public static void RequestLoad<T>(string key, bool isGlobalResource = false, Action<T> onCompleted = null) where T: UnityObject {
            processQueue.Enqueue(Request);
            return;

            void Request() {
                ResourceManager.Instance.AddressablesAsyncLoad<T>(key, isGlobalResource, (loadedResource) => {
                    onCompleted?.Invoke(loadedResource);
                });
            }
        }

        public static void RequestRegist(string key, int spawnCount = PoolInformation.DEFAULT_SPAWN_COUNT, Action<bool> onCompleted = null) {
            processQueue.Enqueue(Request);
            return;

            void Request() {
                if (ObjectPoolManager.Instance.IsExistInPoolDictionary(key)) {
                    onCompleted?.Invoke(true);
                    return;
                }
                
                ResourceManager.Instance.AddressablesAsyncLoad<GameObject>(key, false, (loadedGameObject) => {
                    if (!loadedGameObject) {
                        onCompleted?.Invoke(false);
                        return;
                    }
                    ObjectPoolManager.Instance.AddToPool(PoolInformation.Create(loadedGameObject));
                    onCompleted?.Invoke(true);
                });
            }
        }
    }
}