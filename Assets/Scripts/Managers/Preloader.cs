// CODER: KIM MIN WOOK
// DATE : 2023. 08. 30
// DESC : ResourceManager를 통한 원본 프리팹 동적 로드 및 ObjectPool에 등록하는 과정을 간단하게 처리하기 위한 static class
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using CoffeeCat.Utils;

namespace CoffeeCat.FrameWork {
    public static class Preloader {
        private static readonly Dictionary<string, Request> requestDictionary = new Dictionary<string, Request>();
        private static readonly Queue<Action> processQueue = new Queue<Action>();
        private static bool IsProcessing = false;
        
        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StartProcess(SceneBase key) {
            if (IsProcessing) {
                return;
            }

            // Main Process Observable Start
            IsProcessing = true;
            Observable.EveryUpdate()
                      .Skip(TimeSpan.Zero)
                      .TakeWhile(_ => IsProcessing)
                      .Where(_ => processQueue.Count > 0)
                      .Select(_ => processQueue.Dequeue())
                      .Subscribe(request => {
                          // ~ per 1 Frame
                          request.Invoke();
                      })
                      .AddTo(key);
            //.TakeUntilDisable(key)
        }

        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StopProcess(SceneBase key) {
            IsProcessing = false;
            int processLeft = processQueue.Count;
            if (processLeft > 0) {
                CatLog.WLog($"Count Of UnProcessed: {processLeft.ToString()}");
            }
            processQueue.Clear();
            requestDictionary.Clear();
        }

        /// <summary>
        /// Pre Resource Load And Add To ObjectPool Method
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onCompleted"></param>
        public static void Process(string key, Action onCompleted = null) {
            processQueue.Enqueue(() => Preload(key, onCompleted));
        }

        /// <summary>
        /// Pre Only Resource Load Method
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onCompleted"></param>
        /// <typeparam name="T"></typeparam>
        private static void Process<T>(string key, Action onCompleted = null) where T: UnityEngine.Object {
            processQueue.Enqueue(() => Preload<T>(key, onCompleted));
        }
        
        private static void Preload(string key, Action onCompleted) {
            // Already Requested Or Completed Process
            if (requestDictionary.TryGetValue(key, out Request request)) {
                request.RequestedOrCompleted(onCompleted);
                return;
            }
            
            // Preloader Ignored
            if (ObjectPoolManager.Instance.IsExistInPoolDictionary(key)) {
                CatLog.WLog("Key already registered in ObjectPool without using Preloader.");
                return;
            }
            
            // Defined that it was requested as a Dictionary assignment (Prevent re-requests while taking control of Unity)
            requestDictionary.Add(key, new Request(onCompleted));
            ResourceManager.Instance.AddressablesAsyncLoad<GameObject>(key, false, (loadedGameObject) => {
                ObjectPoolManager.Instance.AddToPool(PoolInformation.New(loadedGameObject));
                requestDictionary[key].RequestComplete();
            });
        }
        
        private static void Preload<T>(string key, Action onCompleted) where T: UnityEngine.Object {
            // Already Requested Or Completed Process
            if (requestDictionary.TryGetValue(key, out Request request)) {
                request.RequestedOrCompleted(onCompleted);
                return;
            }

            // Preloader Ignored
            if (ResourceManager.Instance.IsRequestedOrCompleted(key)) {
                CatLog.WLog("Key already registered in Resources Dictionary without using Preloader.");
                return;   
            }
            
            // Defined that it was requested as a Dictionary assignment (Prevent re-requests while taking control of Unity)
            requestDictionary.Add(key, new Request(onCompleted));
            ResourceManager.Instance.AddressablesAsyncLoad<T>(key, false, (resource) => {
                requestDictionary[key].RequestComplete();
            });
        }

        private class Request {
            private STATE state = STATE.REQUESTED;
            private Action onCompleted = null;

            public Request(Action onCompleted) {
                this.onCompleted = onCompleted;
            }

            private enum STATE {
                REQUESTED,
                COMPLETED,
            }

            public void RequestComplete() {
                state = STATE.COMPLETED;
                onCompleted?.Invoke();
            }

            public void RequestedOrCompleted(Action callback) {
                if (state == STATE.REQUESTED) {
                    onCompleted += callback;
                }
                else {
                    callback?.Invoke();
                }
            }
        }
    }
}

