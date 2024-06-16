using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.FrameWork {
    public class SceneManager : DynamicSingleton<SceneManager> {
        public const float SCENELOAD_CORRECTION_MINTIME = 3f;

        [Title("SCENE MANAGER FIELDS")]
        [ShowInInspector, ReadOnly] public float SceneLoadAsyncProgress { get; private set; } = 0f; // 0f ~ 1f
        [ShowInInspector, ReadOnly] public SceneName CurrentScene { get; private set; } = SceneName.NONE;
        [ShowInInspector, ReadOnly] public SceneName NextScene { get; private set; } = SceneName.NONE;
        [ShowInInspector, ReadOnly] public bool IsNextSceneLoadCompleted { get; private set; } = false;
        private AsyncOperation sceneLoadAsyncOperation = null;

        public delegate void OnSceneChangeEvent(SceneName sceneName);
        public event OnSceneChangeEvent OnSceneChangeBeforeEvent = delegate { };
        public event OnSceneChangeEvent OnSceneChangeAfterEvent = delegate { };

        //
        // !씬 이벤트 관리 방식 변경
        //
        //private void ActiveSceneChanger() {
        //    //CatLog.Log($"First Scene Name: {UnitySceneManager.GetSceneAt(0).name}");
        //    //CatLog.Log($"Seconds Scene Name: {UnitySceneManager.GetSceneAt(1).name}");
        //    //UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneAt(0));
        //}
        //
        //private void SceneOrderChanger() {
        //    
        //}
        //
        //private void MergeScene() {
        //    UnitySceneManager.MergeScenes();
        //}
        //
        //private void AddListnerToSceneLoadedEvent() {
        //    UnitySceneManager.sceneLoaded += SceneChangeCallback;
        //    
        //    void SceneChangeCallback(Scene scene, LoadSceneMode loadSceneMode) {
        //        if (scene.name.Equals(Tags.Scene.SampleScene.ToString())) {
        //
        //        }
        //    }
        //}

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

        #region LOAD SCENE SYNC (UNITY DEFAULT)

        public void LoadSceneUnityDefault(SceneName loadTargetScene) {
            UnitySceneManager.LoadScene(loadTargetScene.ToString(), LoadSceneMode.Single);
        }

        public void LoadSceneAdditiveUnityDefault(SceneName loadTargetScene) {
            UnitySceneManager.LoadScene(loadTargetScene.ToString(), LoadSceneMode.Additive);
        }

        #endregion

        #region LOAD SCENE ASYNC

        public void LoadSceneAsyncAfterLoadingScene(SceneName loadTargetScene) {
            // Load Loading Scene and Immidiate Active
            StartCoroutine(LoadSceneAsync(SceneName.LoadingScene, LoadSceneMode.Single, () => {
                ActiveNextScene(true, false, () => {
                    LoadingSceneBase.Inst.LoadNextScene(loadTargetScene);
                });
            }));
        }

        public void LoadSceneAsync(SceneName loadTargetSceneType, Action onNextSceneLoadCompletedAction = null) {
            StartCoroutine(LoadSceneSingleAsyncMinTimeCorrection(loadTargetSceneType, onNextSceneLoadCompletedAction));
        }

        public void LoadSceneAdditiveAsync(SceneName loadTargetSceneType, Action onNextSceneLoadCompletedAction = null) {
            StartCoroutine(LoadSceneAsync(loadTargetSceneType, LoadSceneMode.Additive, onNextSceneLoadCompletedAction));
        }

        private IEnumerator LoadSceneAsync(SceneName loadTargetSceneType, LoadSceneMode loadSceneMode, Action onNextSceneLoadCompleted = null) {
            NextScene = loadTargetSceneType;
            sceneLoadAsyncOperation = UnitySceneManager.LoadSceneAsync(NextScene.ToString(), loadSceneMode);
            sceneLoadAsyncOperation.allowSceneActivation = false;

            while (sceneLoadAsyncOperation.isDone == false) { 
                // (allowSceneActivation이 false인 동안에는 progress가 0.9f 까지만 증가)
                if (sceneLoadAsyncOperation.progress >= 0.9f) {
                    break;
                }
                
                SceneLoadAsyncProgress = sceneLoadAsyncOperation.progress;
                yield return null;
            }

            SceneLoadAsyncProgress = 1f;
            IsNextSceneLoadCompleted = true;
            onNextSceneLoadCompleted?.Invoke();
        }

        private IEnumerator LoadSceneSingleAsyncMinTimeCorrection(SceneName loadTargetSceneType, Action onNextSceneLoadCompleted = null,
            float minLoadDuration = SceneManager.SCENELOAD_CORRECTION_MINTIME) {
            NextScene = loadTargetSceneType;
            sceneLoadAsyncOperation = UnitySceneManager.LoadSceneAsync(NextScene.ToString(), LoadSceneMode.Single);
            sceneLoadAsyncOperation.allowSceneActivation = false;

            // Fake Loading Variables
            float fakeLoadTime = 0f;
            float fakeLoadRatio = 0f;
            SceneLoadAsyncProgress = 0f;

            while (sceneLoadAsyncOperation.isDone == false) {
                // Calculate Fake Loading Time
                fakeLoadTime += Time.deltaTime;
                fakeLoadRatio = fakeLoadTime / minLoadDuration;

                SceneLoadAsyncProgress = Mathf.Min(sceneLoadAsyncOperation.progress + 0.1f, fakeLoadRatio);
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

        public void ActiveNextScene(bool isInvokeSceneChangeBeforeEvent = false, bool isInvokeSceneChangeAfterEvent = false, Action onNextSceneActiveCompletedAction = null) {
            if (!IsNextSceneLoadCompleted) {
                CatLog.WLog("Scene Not Load Completed !");
                return;
            }

            OnSceneChangeBeforeEvent.Invoke(CurrentScene);
            
            sceneLoadAsyncOperation.completed += (asyncOperation) => {
                IsNextSceneLoadCompleted = false;
                SceneLoadAsyncProgress = 0f;
                sceneLoadAsyncOperation = null;
                onNextSceneActiveCompletedAction?.Invoke();
                OnSceneChangeAfterEvent.Invoke(NextScene);
                CurrentScene = NextScene;
                NextScene = SceneName.NONE;
            };
            sceneLoadAsyncOperation.allowSceneActivation = true;
            sceneLoadAsyncOperation = null;
        }

        #endregion

        #region ADDRESSABLES LOAD SCENE ASYNC

        public void AddressablesLoadSceneAsync()
        {
            return;
        }

        public void AddressablesLoadSceneAdditiveAsync()
        {
            return;
        }

        #endregion
    }
}
