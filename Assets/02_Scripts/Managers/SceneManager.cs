using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Cysharp.Threading.Tasks;

namespace CoffeeCat.FrameWork {
    public class SceneManager : DynamicSingleton<SceneManager> {
        [Title("SceneManager")]
        [ShowInInspector, ReadOnly] public float SceneLoadAsyncProgress { get; private set; } = 0f; // 0f ~ 1f
        [ShowInInspector, ReadOnly] public SceneName CurrentScene { get; private set; } = SceneName.NONE;
        [ShowInInspector, ReadOnly] public SceneName NextScene { get; private set; } = SceneName.NONE;
        [ShowInInspector, ReadOnly] public bool IsNextSceneLoadCompleted { get; private set; } = false;
        private AsyncOperation asyncOperation = null;

        public delegate void OnSceneChangeEvent(SceneName sceneName);
        public event OnSceneChangeEvent OnSceneChangeBeforeEvent = delegate { };
        public event OnSceneChangeEvent OnSceneChangeAfterEvent = delegate { };

        private const float MIN_LOAD_SECONDS = 3f;
        
        protected override void Initialize() {
            // Get Current Scene
            string currentSceneName = UnitySceneManager.GetActiveScene().name;
            if (Enum.TryParse(currentSceneName, out SceneName result)) {
                CurrentScene = result;
            }
            else {
                CatLog.WLog($"SceneManager: Failed to Get CurrentScene.(SceneName: {currentSceneName})" + '\n' +
                            $"Add This Scene Name in SceneName Enum Field.");
            }
        }

        #region LOAD SCENE ASYNC

        public void LoadSceneAdditive(SceneName key, Action onCompleted = null) {
            LoadSceneAsyncUniTask(key, LoadSceneMode.Additive, true, false, false, false, onCompleted).Forget();
        }

        public void LoadSceneSingle(SceneName key, bool activeDirectly, bool useFakeTime, Action onCompleted = null) {
            LoadSceneAsyncUniTask(key, LoadSceneMode.Single, activeDirectly, useFakeTime, true, true, onCompleted).Forget();
        }

        public void LoadSceneAsync(SceneName loadTargetSceneType, Action onLoadCompleted = null) {
            StartCoroutine(LoadSceneSingleAsyncMinTimeCorrection(loadTargetSceneType, onLoadCompleted));
        }

        public void LoadSceneAdditiveAsync(SceneName loadTargetSceneType, Action onLoadCompleted = null) {
            StartCoroutine(LoadSceneAsync(loadTargetSceneType, LoadSceneMode.Additive, onLoadCompleted));
        }

        private IEnumerator LoadSceneAsync(SceneName loadTargetSceneType, LoadSceneMode loadSceneMode, Action onNextSceneLoadCompleted = null) {
            NextScene = loadTargetSceneType;
            asyncOperation = UnitySceneManager.LoadSceneAsync(NextScene.ToKey(), loadSceneMode);
            asyncOperation.allowSceneActivation = false;

            while (asyncOperation.isDone == false) { 
                // (allowSceneActivation이 false인 동안에는 progress가 0.9f 까지만 증가)
                if (asyncOperation.progress >= 0.9f) {
                    break;
                }
                
                SceneLoadAsyncProgress = asyncOperation.progress;
                yield return null;
            }

            SceneLoadAsyncProgress = 1f;
            IsNextSceneLoadCompleted = true;
            onNextSceneLoadCompleted?.Invoke();
        }

        private IEnumerator LoadSceneSingleAsyncMinTimeCorrection(SceneName loadTargetSceneType, Action onNextSceneLoadCompleted = null,
            float minLoadDuration = SceneManager.MIN_LOAD_SECONDS) {
            NextScene = loadTargetSceneType;
            asyncOperation = UnitySceneManager.LoadSceneAsync(NextScene.ToKey(), LoadSceneMode.Single);
            asyncOperation.allowSceneActivation = false;

            // Fake Loading Variables
            float fakeLoadTime = 0f;
            float fakeLoadRatio = 0f;
            SceneLoadAsyncProgress = 0f;

            while (asyncOperation.isDone == false) {
                // Calculate Fake Loading Time
                fakeLoadTime += Time.deltaTime;
                fakeLoadRatio = fakeLoadTime / minLoadDuration;

                SceneLoadAsyncProgress = Mathf.Min(asyncOperation.progress + 0.1f, fakeLoadRatio);
                if (SceneLoadAsyncProgress >= 1f) {
                    break;
                }

                yield return null;
            }

            // Set Max Value Scene Load Progress Value
            SceneLoadAsyncProgress = 1f;
            IsNextSceneLoadCompleted = true;
            onNextSceneLoadCompleted?.Invoke();
        }

        public void ActiveNextScene(bool invokeAfterEvent = false, bool isInvokeBeforeEvent = false, Action onActivated = null) {
            if (!IsNextSceneLoadCompleted) {
                CatLog.WLog("Scene Not Load Completed !");
                return;
            }

            if (isInvokeBeforeEvent) {
                OnSceneChangeBeforeEvent.Invoke(CurrentScene);
            }
            
            asyncOperation.completed += (asyncOperation) => {
                IsNextSceneLoadCompleted = false;
                SceneLoadAsyncProgress = 0f;
                this.asyncOperation = null;
                onActivated?.Invoke();
                if (invokeAfterEvent) {
                    OnSceneChangeAfterEvent.Invoke(NextScene);
                }
                CurrentScene = NextScene;
                NextScene = SceneName.NONE;
            };
            asyncOperation.allowSceneActivation = true;
            asyncOperation = null;
        }

        private async UniTaskVoid LoadSceneAsyncUniTask(SceneName key, LoadSceneMode mode, bool activeDirectly, bool useFakeTime, bool after, bool before, Action onCompleted) {
            // Clear Variables
            SceneLoadAsyncProgress = 0f;
            IsNextSceneLoadCompleted = false;
            
            // Request LoadAsync AsyncOperation
            asyncOperation = UnitySceneManager.LoadSceneAsync(key.ToKey(), mode);
            if (asyncOperation == null)
                return;
            
            // Add Completed Event To AsyncOperation
            asyncOperation.allowSceneActivation = false;
            asyncOperation.completed += (operation) => {
                IsNextSceneLoadCompleted = false;
                SceneLoadAsyncProgress = 0f;
                asyncOperation = null;
                onCompleted?.Invoke();
                if (after) {
                    OnSceneChangeAfterEvent.Invoke(key);
                }
                CurrentScene = key;
                NextScene = SceneName.NONE;
            };
            
            if (before) {
                OnSceneChangeBeforeEvent.Invoke(key);
            }
            
            // Wait Operation progress Completed With Update Load Progress
            if (useFakeTime) {
                // Fake Loading Variables
                float fakeLoadTime = 0f;
                while (asyncOperation.isDone == false) {
                    // Calculate Fake Loading Time
                    fakeLoadTime += Time.deltaTime;
                    var fakeLoadRatio = fakeLoadTime / MIN_LOAD_SECONDS;
                    SceneLoadAsyncProgress = Mathf.Min(asyncOperation.progress + 0.1f, fakeLoadRatio);
                    // Only increases to 0.9 while allowSceneActivation is false
                    if (asyncOperation.progress >= 0.9f) {
                        break;
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroy());
                }
            }
            else {
                while (asyncOperation.isDone == false) {
                    SceneLoadAsyncProgress = asyncOperation.progress + 0.1f;
                    // Only increases to 0.9 while allowSceneActivation is false
                    if (asyncOperation.progress >= 0.9f) {
                        break;
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroy());
                }
            }
            
            // Set Max Value Scene Load Progress Value
            SceneLoadAsyncProgress = 1f;
            IsNextSceneLoadCompleted = true;

            if (activeDirectly == false)
                return;
            
            // Active Directly Next Scene
            ActiveNextScene();
        }

        private void ActiveNextScene() {
            if (!IsNextSceneLoadCompleted || asyncOperation == null) {
                CatLog.Log("Active NextScene Failed !");
                return;
            }
            
            asyncOperation.allowSceneActivation = true;
            asyncOperation = null;
            IsNextSceneLoadCompleted = false;
        }

        #endregion
    }
}
