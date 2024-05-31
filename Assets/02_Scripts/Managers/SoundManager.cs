using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat.FrameWork { 
    public class SoundManager : GenericSingleton<SoundManager> {
        [Title("AUDIO GROUP", TitleAlignment = TitleAlignments.Centered)]
        [ShowInInspector, ReadOnly] public Transform AudioCameraTr { get; private set; } = null;
        [ShowInInspector, ReadOnly] public Transform AudioGroupTr { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioListener Listener { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioSource ChannelBgm { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioSource ChannelSe { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioSource ChannelAmbient { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioMixer MainAudioMixer { get; private set; } = null;
        public Transform Tr { get; private set; } = null;

        [Title("CLIPS")]
        [SerializeField, ReadOnly] private StringAudioClipDictionary audioClipDictionary = null;

        #region EXPERIMENTAL (CUSTOM CHANNEL)

        [Title("CUSTOM CHANNEL")]
        [SerializeField, ReadOnly] private StringAudioSourceDictionary customChannelDictionary = null;
        [SerializeField, ReadOnly] private GameObject customChannelOrigin = null;

        public void RegistCustomChannel(string key, AudioClip audioClip, float volume = 1f) {
            if (customChannelDictionary.ContainsKey(key))
                return;

            if (customChannelOrigin == null) {
                customChannelOrigin = ResourceManager.Instance.ResourcesLoad<GameObject>("Audio/AudioChannel_Custom", false);
            }

            var spawnedCustomChannel = Instantiate(customChannelOrigin, Vector3.zero, Quaternion.identity, AudioGroupTr).GetComponent<AudioSource>();
            spawnedCustomChannel.transform.localPosition = Vector3.zero;
            spawnedCustomChannel.volume = volume;
            spawnedCustomChannel.clip = audioClip;
            
            customChannelDictionary.Add(key, spawnedCustomChannel);
        }

        public void PlayCustomChannel(string key) {
            if (!customChannelDictionary.TryGetValue(key, out AudioSource audioSource))
                return;
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            audioSource.Play();
        }

        public void StopCustomChannel(string key) {
            if (!customChannelDictionary.TryGetValue(key, out AudioSource audioSource))
                return;
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
        }

        public void StopAllCustomChannel() {
            foreach (var keyValuePair in customChannelDictionary) {
                var audioSource = keyValuePair.Value;
                if (audioSource.isPlaying) {
                    keyValuePair.Value.Stop();
                }
            }
        }

        public void ReleaseCustomChannel(string key) {
            if (customChannelDictionary.Remove(key, out AudioSource audioSource)) {
                audioSource.transform.SetParent(null);
                audioSource.gameObject.SetActive(false);
            }
        }

        public void ReleaseAllCustomChannel() {
            foreach (var keyValuePair in customChannelDictionary) {
                if (customChannelDictionary.Remove(keyValuePair.Key, out AudioSource audioSource)) {
                    audioSource.transform.SetParent(null);
                    audioSource.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        protected override void Initialize() {
            audioClipDictionary = new StringAudioClipDictionary();
            //customChannelDictionary = new StringAudioSourceDictionary();
            AudioCameraTr = Camera.main.GetComponent<Transform>();
            var resourcesLoadAudioGroup = ResourceManager.Instance.ResourcesLoad<GameObject>("Audio/AudioGroup", false);
            if (resourcesLoadAudioGroup != null) {
                AudioGroupTr = Instantiate<Transform>(resourcesLoadAudioGroup.transform, Vector3.zero, Quaternion.identity, AudioCameraTr);

                // Get AudioGroup Components
                var audioGroup = AudioGroupTr.GetComponent<AudioGroup>();
                Listener   = audioGroup.Listener;
                ChannelBgm = audioGroup.ChannelBgm;
                ChannelSe  = audioGroup.ChannelSE;
                ChannelAmbient = audioGroup.ChannelAmbient;
                MainAudioMixer = audioGroup.MainAudioMixer;
            }
        }

        private void Start() {
            Tr = GetComponent<Transform>();
            SceneManager.Instance.OnSceneChangeBeforeEvent += OnSceneChangeBeforeEvent;
            SceneManager.Instance.OnSceneChangeAfterEvent += OnSceneChangeAfterEvent;
        }

        public void RegistAudioClips(StringAudioClipDictionary sceneExistAudioClipDictionary) {
            foreach (var keyValuePair in sceneExistAudioClipDictionary) {
                audioClipDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        #region SE

        public void PlaySE(string key, float volume = 1f) {
            if (!audioClipDictionary.TryGetValue(key, out AudioClip clip)) {
                CatLog.ELog($"AudioClip Not Exist in Dictioanry. key : {key}");
                return;
            }

            PlaySE(clip, volume);
        }

        public void PlaySE(AudioClip clip, float volume = 1f) {
            ChannelSe.PlayOneShot(clip, volume);
        }

        #endregion

        #region BGM

        public void PlayBgm(string key, float volume = 1f, bool isLoop = true) {
            if (!audioClipDictionary.TryGetValue(key, out AudioClip clip)) {
                CatLog.ELog($"AudioClip Not Exist in Dictioanry. key : {key}");
                return;
            }

            PlayBgm(clip, volume);
        }

        public void PlayBgm(AudioClip clip, float volume = 1f, bool isLoop = true) {
            if (ChannelBgm.isPlaying) {
                ChannelBgm.Stop();
            }

            ChannelBgm.volume = volume;
            ChannelBgm.loop = isLoop;
            ChannelBgm.clip = clip;
            ChannelBgm.Play();
        }

        public void StopBgm(bool isReleaseClip = true) {
            ChannelBgm.Stop();
            if (isReleaseClip) {
                ChannelBgm.clip = null;
            }
        }

        public void FadeInBgm(bool isStop) {

        }

        public void FadeOutBgm() {

        }

        #endregion

        #region AMBIENT

        public void PlayAmbient(string key, float volume = 1f, bool isLoop = true) {
            if (!audioClipDictionary.TryGetValue(key, out AudioClip clip)) {
                CatLog.ELog($"AudioClip Not Exist in Dictioanry. key : {key}");
                return;
            }

            PlayAmbient(clip, volume);
        }

        public void PlayAmbient(AudioClip clip, float volume = 1f, bool isLoop = true) {
            if (ChannelAmbient.isPlaying) {
                ChannelAmbient.Stop();
            }
            
            ChannelAmbient.volume = volume;
            ChannelAmbient.loop = isLoop;
            ChannelAmbient.clip = clip;
            ChannelAmbient.Play();
        }

        public void StopAmbient(bool isReleaseClip = true) {
            ChannelAmbient.Stop();
            if (isReleaseClip) {
                ChannelAmbient.clip = null;
            }
        }

        #endregion

        private void OnSceneChangeBeforeEvent(SceneName sceneName) {
            ClearAudioClipDictionary();
            ReleaseParentAudioGroup();
        }

        private void OnSceneChangeAfterEvent(SceneName sceneName) {
            SetParentAudioGroupToMainCamera();
        }

        private void ClearAudioClipDictionary() => audioClipDictionary.Clear();

        private void ReleaseParentAudioGroup() {
            AudioGroupTr.SetParent(Tr);
            AudioCameraTr = null;
        }

        private void SetParentAudioGroupToMainCamera() {
            AudioCameraTr = Camera.main.transform;
            if (AudioCameraTr == null) {
                CatLog.WLog("Main Camera was Not Found !");
            }
            AudioGroupTr.SetParent(AudioCameraTr);
            AudioGroupTr.localPosition = Vector3.zero;
            AudioGroupTr.localRotation = Quaternion.identity;
        }
    }
}
