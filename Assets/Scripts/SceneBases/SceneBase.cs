using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UniRx;
using UniRx.Triggers;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.SerializedDictionaries;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.FrameWork {
    /// <summary>
    /// Scene Base Scripts
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneBase : PrelocatedSingleton<SceneBase> {
        [Space(5f), Title(title: "SCENE BASE", titleAlignment: TitleAlignments.Centered, horizontalLine: true, bold: true)]

        [Title("PRELOAD DATA")]
        public bool isPreloadData = true;
        public bool IsSetupDamageText = false;

        [Title("OBJECT POOL"), ListDrawerSettings(NumberOfItemsPerPage = 3, Expanded = false)]
        public PoolInformation[] DefaultPoolInformation = null;

        [Title("AUDIO CLIPS")]
        public StringAudioClipDictionary AudioClipDictionary = null;
        private List<AudioClip> clips = new List<AudioClip>();
        
        // 임시적 TargetFrameRate
        [ReadOnly] public int TargetFrameRate = 60;

        public AssetReference assetRef = null;

        public Camera uiCamera = null;
        public Canvas damageTextCanvas = null;
        public RectTransform damageTextParent = null;

        protected override void Initialize() {
            // Set Target Frame
            Application.targetFrameRate = TargetFrameRate;
            
            if (isPreloadData) {
                DataManager.Instance.DataLoad();
            }

            if (DefaultPoolInformation is { Length: > 0 }) {
                ObjectPoolManager.Instance.AddToPool(DefaultPoolInformation);
            }

            if (AudioClipDictionary is { Count: > 0 }) {
                SoundManager.Instance.RegistAudioClips(AudioClipDictionary);
            }
        }

        public void Start() {
            Preloader.StartProcess(this);
            
            ResourceManager.Instance.AddressablesAsyncLoad<AudioClip>("coin", true, (clip) => {
                clips.Add(clip);
            });
            ResourceManager.Instance.AddressablesAsyncLoad<AudioClip>("coin2", true, (clip) => {
                clips.Add(clip);
            });
            ResourceManager.Instance.AddressablesAsyncLoad<AudioClip>("coin3", true, (clip) => {
                clips.Add(clip);
            });

            this.UpdateAsObservable()
                .Skip(0)
                .TakeUntilDestroy(this)
                .Subscribe(_ => {
                    if (Input.GetKeyDown(KeyCode.A)) {
                        SoundManager.Instance.PlayBgm("song18");
                        SoundManager.Instance.PlayAmbient("birdsongloop16s");
                        //SoundManager.Instance.RegistCustomChannel("Coin", clips[0]);
                    }

                    if (Input.GetKeyDown(KeyCode.D)) {
                        SoundManager.Instance.StopBgm();
                        SoundManager.Instance.StopAmbient();
                        //SoundManager.Instance.PlayCustomChannel("Coin");
                    }

                    if (Input.GetKeyDown(KeyCode.E)) {
                        SceneManager.Instance.LoadSceneAsyncAfterLoadingScene(SceneName.MonsterSampleScene);
                        //SoundManager.Instance.StopCustomChannel("Coin");
                    }

                    if (Input.GetKeyDown(KeyCode.R)) {
                        //SoundManager.Instance.PlaySE(clips[Random.Range(0, clips.Count)], .3f);
                        //SoundManager.Instance.ReleaseCustomChannel("Coin");
                    }
                })
                .AddTo(this);

            if (IsSetupDamageText) {
                DamageTextManager.Instance.Setup(damageTextCanvas, uiCamera);
            }
            else {
                if (DamageTextManager.IsExist) {
                    DamageTextManager.Instance.ReleaseSingleton();
                }
            }
        }

        private void OnDisable() {
            Preloader.StopProcess(this);
        }
    }
}
