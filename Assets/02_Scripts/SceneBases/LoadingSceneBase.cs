using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UniRx.Triggers;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class LoadingSceneBase : SceneSingleton<LoadingSceneBase> {
        [Header("FIELDS")]
        [SerializeField] Slider sliderLoading = null;
        [SerializeField] TextMeshProUGUI tmpLoadingState = null;
        [SerializeField] TextMeshProUGUI tmpLoadingPercent = null;

        public void LoadNextScene(SceneName loadTargetScene) {
            SceneManager.Instance.LoadSceneAsync(loadTargetScene, () => {
                // Change Scene Immidiately Completed Scene Loading
                SceneManager.Instance.ActiveNextScene(false, true);
            });

            SubscribeUpdateLoadingScene();
        }

        private void SubscribeUpdateLoadingScene() {
            this.UpdateAsObservable()
                .Skip(System.TimeSpan.Zero)
                .Select(_ => SceneManager.Instance)
                .TakeWhile(sceneManager => sceneManager.IsNextSceneLoadCompleted == false)
                .DoOnSubscribe(() => {
                    tmpLoadingState.text = "Now Scene Laoding...";
                })
                .DoOnCompleted(() => {
                    tmpLoadingState.text = "Load Completed !";
                    sliderLoading.value = 1f;
                })
                .Subscribe(sceneManager => {
                    sliderLoading.value = sceneManager.SceneLoadAsyncProgress;
                    tmpLoadingPercent.text = sceneManager.SceneLoadAsyncProgress.ToString("#0 %");
                })
                .AddTo(this);
        }
    }
}
