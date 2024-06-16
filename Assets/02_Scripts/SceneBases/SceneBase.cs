using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat.FrameWork {
    /// <summary>
    /// Scene Base Scripts
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneBase : SceneSingleton<SceneBase> {
        [Space(5f), Title(title: "SCENE BASE", titleAlignment: TitleAlignments.Centered, horizontalLine: true, bold: true)]

        [Title("PRELOAD DATA")]
        public bool isPreloadData = true;

        [Title("OBJECT POOL"), ListDrawerSettings(NumberOfItemsPerPage = 3, Expanded = false)]
        public PoolInformation[] DefaultPoolInformation = null;

        [Title("AUDIO CLIPS")]
        public StringAudioClipDictionary AudioClipDictionary = null;
        private List<AudioClip> clips = new();
        
        // 임시적 TargetFrameRate
        [ReadOnly] public int TargetFrameRate = 60;
        public Camera uiCamera = null;
        public Canvas damageTextCanvas = null;
        public RectTransform damageTextParent = null;

        protected override void Initialize() {
            // Set Target Frame
            Application.targetFrameRate = TargetFrameRate;
            SoundManager.Instance.RegistAudioClips(AudioClipDictionary);

            if (isPreloadData) {
                DataManager.Instance.DataLoad();
            }
        }

        public void Start() {
            ObjectPoolManager.Instance.AddToPool(DefaultPoolInformation);
            DamageTextManager.Instance.Setup(damageTextCanvas, uiCamera);
            SafeLoader.StartProcess(gameObject);
        }

        private void OnDisable() {
            SafeLoader.StopProcess();
        }
    }
}