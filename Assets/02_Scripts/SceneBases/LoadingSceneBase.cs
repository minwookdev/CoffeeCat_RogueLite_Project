using Cysharp.Threading.Tasks;
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
        [SerializeField] private Slider sliderLoading = null;
        [SerializeField] private TextMeshProUGUI tmpLoadingState = null;
        [SerializeField] private TextMeshProUGUI tmpLoadingPercent = null;
        
        private const float WAIT_SECONDS = 0.5f;
        
        private async UniTaskVoid LoadNextSceneAsync(SceneName targetScene) {
            // Request Load Next Scene
            SceneManager.Inst.LoadSceneSingle(targetScene, false, true);
            
            // Update Loading Progress
            SubscribeUpdateLoadingScene();
            
            var tokenOnDestroy = gameObject.GetCancellationTokenOnDestroy();
            
            // Wait Scene Load Completed
            await UniTask.WaitUntil(() => SceneManager.Inst.IsNextSceneLoadCompleted, PlayerLoopTiming.Update, tokenOnDestroy);
            
            // Wait 0.5 Seconds
            await UniTask.WaitForSeconds(WAIT_SECONDS, delayTiming: PlayerLoopTiming.Update, cancellationToken: tokenOnDestroy);
            
            SceneManager.Inst.ActiveNextScene();
        }

        private void SubscribeUpdateLoadingScene() {
            this.UpdateAsObservable()
                .Skip(System.TimeSpan.Zero)
                .Select(_ => SceneManager.Inst)
                .TakeWhile(sceneMgr => sceneMgr.IsNextSceneLoadCompleted == false)
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
