using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UniRx;
using UniRx.Triggers;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.SerializedDictionaries;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using UnityObject = UnityEngine.Object;

namespace CoffeeCat.FrameWork {
    public class ResourceManager : GenericSingleton<ResourceManager> {
        [Serializable]
        public class ResourceInfo {
            public enum ASSETLOADSTATUS {
                LOADING,
                FAILED,
                SUCCESS
            }

            [ShowInInspector, ReadOnly] public UnityObject Resource { get; private set; } = null;
            [ShowInInspector, ReadOnly] public bool isGlobalResource { get; private set; } = false;
            [ShowInInspector, ReadOnly] public ASSETLOADSTATUS Status { get; private set; } = ASSETLOADSTATUS.LOADING;
            private bool isAddressablesAsset = false;
            private AsyncOperationHandle handle;
            public string ResourceName => Resource.name;

            public static ResourceInfo Create(bool isGlobal, bool loadByAddressables) {
                return new ResourceInfo() {
                    isGlobalResource = isGlobal,
                    isAddressablesAsset = loadByAddressables
                };
            }
            
            public void Dispose() {
                if (!isAddressablesAsset) {
                    Resources.UnloadAsset(Resource);
                }
                else {
                    Addressables.Release(Resource);
                    if (handle.IsValid()) {
                        Addressables.Release(handle);
                    }
                }
                Resource = null;
            }
            
            public void SetFailed() {
                if (Status != ASSETLOADSTATUS.LOADING || Resource) {
                    CatLog.Log("Status has already yielded results.");
                    return;
                }

                Status = ASSETLOADSTATUS.FAILED;
            }
            
            public T SetResource<T>(T resource) where T : UnityObject {
                if (Status != ASSETLOADSTATUS.LOADING || Resource) {
                    CatLog.ELog("Status has already yielded results.");
                    return null;
                }

                if (!resource) {
                    Status = ASSETLOADSTATUS.FAILED;
                    CatLog.ELog("Failed To Set Resource. Resource is Null.");
                    return null;
                }

                Resource = resource;
                Status = ASSETLOADSTATUS.SUCCESS;
                return GetResource<T>();
            }

            public T GetResource<T>() where T : UnityObject {
                if (Status != ASSETLOADSTATUS.SUCCESS || !Resource) {
                    CatLog.ELog("Resource is Not Prepared Yet.");
                    return null;
                }
                var casting = Resource as T;
                if (casting) {
                    return casting;
                }
                CatLog.ELog($"Resource Casting Failed. Target: {ResourceName}, Type: {nameof(T)}");
                return null;
            }

            public void SetHandle(AsyncOperationHandle asyncOperationHandle) {
                if (!isAddressablesAsset) {
                    CatLog.ELog("Invalid Operation: This Information is Not Allowed Setting The Handle.");
                    return;
                }
                handle = asyncOperationHandle;
            }
        }
        
        // Loaded Resources Dictioanry
        [SerializeField, Title("Loaded Resources Dictionary")]
        private StringResourceInformationDictionary resourcesDict = null;
        
        protected override void Initialize() {
            resourcesDict = new StringResourceInformationDictionary();
        }

        protected void Start() {
            SceneManager.Instance.OnSceneChangeBeforeEvent += OnSceneChangeBeforeEvent;
            SceneManager.Instance.OnSceneChangeAfterEvent += OnSceneChangeAfterEvent;
#if UNITY_EDITOR
            CheckDuplicatesInDictionary();
#endif
        }

        #region EVENTS

        private void OnSceneChangeBeforeEvent(SceneName sceneName) {
            ReleaseAll();
        }

        private void OnSceneChangeAfterEvent(SceneName sceneName) {
            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region RESOURCES_LOAD

        public T ResourcesLoad<T>(string loadPath, bool isGlobal = false) where T : UnityObject {
            // Check Already Loaded Resources
            string fileName = GetFileName(loadPath);
            if (TryGetResource(fileName, out T result)) {
                return result;
            }

            // Load Resource and Add to Dictionary
            var info = ResourceInfo.Create(isGlobal, false);
            resourcesDict.Add(fileName, info);
            result = Resources.Load<T>(loadPath);
            if (result) {
                return info.SetResource<T>(result);
            }
            info.SetFailed();
            CatLog.ELog($"Not Exist Asset(name:{fileName}) in Resources Folder or Load Type is MissMatached.");
            return null;
        }

        public bool ResourcesLoad<T>(string loadPath, out T tResult, bool isGlobal = false) where T : UnityObject {
            tResult = ResourcesLoad<T>(loadPath, isGlobal);
            return tResult != null;
        }

        private static string GetFileName(string resourcesLoadPath) => resourcesLoadPath.Substring(resourcesLoadPath.LastIndexOf('/') + 1);

        #endregion

        #region ADDRESSABLES_LOAD
        
        /// <summary>
        /// Addressables AssetLoadAsync By AssetReference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetRef"></param>
        /// <param name="isGlobalResource"></param>
        /// <param name="onCompleted"></param>
        public void AddressablesAsyncLoad<T>(AssetReference assetRef, bool isGlobalResource, Action<T> onCompleted) where T : UnityObject {
            /*var key = assetRef.ToString();
            if (string.IsNullOrEmpty(key)) { // assetReference.RuntimeKey is GUID
                CatLog.ELog($"Invalid Asset Reference Key: {key}");
                return;
            }
            AddressablesAsyncLoad<T>(key, isGlobalResource, onCompleted); */
        }

        /// <summary>
        /// Addressables AssetLoadAsync by Addressables Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Default: Addressables Name</param>
        /// <param name="isGlobalResource"></param>
        /// <param name="onCompleted"></param>
        public void AddressablesAsyncLoad<T>(string key, bool isGlobalResource, Action<T> onCompleted) where T : UnityObject {
            // Dictionary에 이미 로드되거나 요청된 Resource가 존재한다면
            if (TryGetResource(key, onCompleted)) {
                return;
            }

            // 에셋이 로드중인 상태를 정의하기 위해 Dictionary에 미리 추가 (비동기 로드 전 로드중인 리소스임을 정의하기 위함)
            var info = ResourceInfo.Create(isGlobalResource, true);
            resourcesDict.Add(key, info);
            Addressables.LoadAssetAsync<T>(key).Completed += (AsyncOperationHandle<T> operationHandle) => {
                info.SetHandle(operationHandle);
                if (operationHandle.Status != AsyncOperationStatus.Succeeded) {
                    info.SetFailed();
                    CatLog.ELog("ResourceManager: Addressables Async Load Failed !");
                    return;
                }

                var result = info.SetResource<T>(operationHandle.Result);
                if (result) {
                    onCompleted?.Invoke(result);
                }
            };
        }
        
        #endregion

        #region RELEASE

        public void Release(string key) {
            if (!resourcesDict.TryGetValue(key, out ResourceInfo info)) {
                return;
            }
            info.Dispose();
            resourcesDict.Remove(key);
        }

        private void ReleaseAll(bool disposeGloalResources = false) {
            if (disposeGloalResources) {
                foreach (var keyValuePair in resourcesDict) {
                    keyValuePair.Value.Dispose();
                }
                resourcesDict.Clear();
            }
            else {
                var transientResources = resourcesDict.Where(pair => !pair.Value.isGlobalResource);
                foreach (var pair in transientResources) {
                    pair.Value.Dispose();
                    resourcesDict.Remove(pair.Key);
                }
            }
        }

        #endregion

        #region SEARCH_DICTIONARY

        /// <summary>
        /// Find Resource in Dictionary and Return Result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private bool TryGetResource<T>(string key, Action<T> callback) where T : UnityObject {
            var result = resourcesDict.TryGetValue(key, out ResourceInfo info);
            if (!result) { // Target Resource is Not Requested
                return false;
            }
            
            switch (info.Status) {
                case ResourceInfo.ASSETLOADSTATUS.SUCCESS:
                    callback?.Invoke(info.GetResource<T>());
                    break;
                case ResourceInfo.ASSETLOADSTATUS.FAILED:
                    CatLog.ELog("This Resource is Load Failed.");
                    break;
                case ResourceInfo.ASSETLOADSTATUS.LOADING:
                    StartCoroutine(WaitLoadingResource(info, callback));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return true;
        }

        private bool TryGetResource<T>(string key, out T result) where T :UnityObject {
            var isExist = resourcesDict.TryGetValue(key, out ResourceInfo info);
            if (!isExist) {
                result = null;
                return false;
            }
            
            result = info.GetResource<T>();
            return true;
        }
        
        private static IEnumerator WaitLoadingResource<T>(ResourceInfo info, Action<T> onCompleted) where T : UnityObject {
            const float waitTimeOutSeconds = 5.0f;
            float waitTime = 0f;
            string targetName = info.ResourceName;
            while (info.Status == ResourceInfo.ASSETLOADSTATUS.LOADING) {
                waitTime += Time.deltaTime;
                if (waitTime >= waitTimeOutSeconds) {
                    CatLog.ELog($"Resource Load Wait TimedOut ! Target: {targetName}");
                    yield break;
                }
                yield return null;
            }

            if (info.Status != ResourceInfo.ASSETLOADSTATUS.SUCCESS) {
                CatLog.ELog($"Resource Load Failed. Target: {targetName}");
                yield break;
            }

            onCompleted?.Invoke(info.GetResource<T>());
        } 

        public bool IsRequestedOrCompleted(string key) {
            return resourcesDict.ContainsKey(key);
            
            //if (!resourcesDict.TryGetValue(key, out ResourceInformation information)) {
            //    return false;
            //}
            //
            //return information.Status switch {
            //    ResourceInformation.ASSETLOADSTATUS.LOADING => true,
            //    ResourceInformation.ASSETLOADSTATUS.FAILED  => true,
            //    ResourceInformation.ASSETLOADSTATUS.SUCCESS => true,
            //    _                                           => false
            //};
        }

        #endregion

        #region CHECK_DUPLICATES

        /// <summary>
        /// 마지막 Resource Dictionary추가 이후 n초간 대기 후 더 이상 추가되는 요소가 없다면 Dictionary내부 중복 요소 검사
        /// </summary>
        private void CheckDuplicatesInDictionary(float waitSeconds = 2f) {
            // TODO: Make Simple !
#if UNITY_EDITOR
            int previousCheckedDictioanryCount = 0;
            bool isExistDuplicateResourceInDictionary = false;
            this.UpdateAsObservable()
                .Skip(TimeSpan.Zero)
                .Select(_ => resourcesDict)
                .Where(dict => dict.Count != previousCheckedDictioanryCount)
                .TakeUntilDestroy(this)
                .Subscribe(dict => {
                    // Resource Dictionary에 요소가 추가된 경우 (총 요소가 1개 이상)
                    if (previousCheckedDictioanryCount < dict.Count && dict.Count > 1) {
                        this.UpdateAsObservable()
                        .Skip(TimeSpan.Zero)
                        .Select(_ => dict.Values.Where(info => info.Resource == null).Count())
                        .Where(resourceNullInfosCount => resourceNullInfosCount == 0)
                        .First()
                        .TakeUntilDestroy(this)
                        .Subscribe(_ => {
                            isExistDuplicateResourceInDictionary = dict.Values.GroupBy(x => x.ResourceName).Any(g => g.Count() > 1);
                            if (isExistDuplicateResourceInDictionary) {
                                CatLog.ELog($"Checked Duplicate Loaded Resources. Dictionary.Count({dict.Count})");
                            }
                        })
                        .AddTo(this);
                    }

                    previousCheckedDictioanryCount = dict.Count;
                })
                .AddTo(this);
#endif
        }

        #endregion
        
        #region Obsolete
        
        /// <summary>
        /// Addressables AssetLoadSync By Addressables Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Default: AddressablesName</param>
        /// <param name="isMultipleSceenAllowedResources"></param>
        /// <returns></returns>
        [Obsolete("This Mehod is Not Support Wait Asset Loading Status. Use AddressablesAsyncLoad Method.", true)]
        public T AddressablesSyncLoad<T>(string key, bool isMultipleSceenAllowedResources) where T : UnityObject {
            // Dictionary이미 로드된 Resource를 반환
            if (TryGetResourceInDictionarySync<T>(key, out T resource)) {
                return resource;
            }

            // Addressables를 통한 에셋 로드 요청
            resourcesDict.Add(key, ResourceInfo.Create(isMultipleSceenAllowedResources, true));
            var resourceInfo = resourcesDict[key];
            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(key);
            T result = asyncOperationHandle.WaitForCompletion(); // 결과를 받을 때 까지 동기방식으로 대기
            if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded) {
                resourceInfo.SetFailed();
                CatLog.ELog("Addressables AssetSyncLoad Failed.");
                return null;
            }

            /*Addressables.Release(asyncOperationHandle); // Release Handle*/
            return resourceInfo.SetResource<T>(result);
        }

        /// <summary>
        /// Addressables AssetLoadAsync By AssetReference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetRef"></param>
        /// <param name="isMultipleSceenAllowedResources"></param>
        /// <returns></returns>
        [Obsolete("This Mehod is Not Support Wait Asset Loading Status. Use AddressablesAsyncLoad Method.", true)]
        public T AddressablesSyncLoad<T>(AssetReference assetRef, bool isMultipleSceenAllowedResources) where T : UnityObject {
            return AddressablesSyncLoad<T>((string)assetRef.RuntimeKey, isMultipleSceenAllowedResources);
        }
        
        
        /// <summary>
        /// Resource Dictionary에서 로드된 리소스를 찾고 결과를 반환. (Sync전용)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        [Obsolete("This Mehod is Not Support Wait Asset Loading Status. Use AddressablesAsyncLoad Method.")]
        private bool TryGetResourceInDictionarySync<T>(string key, out T resource) where T : UnityObject {
            resource = null;
            var result = resourcesDict.TryGetValue(key, out ResourceInfo information);
            if (result) {
                switch (information.Status) {
                    case ResourceManager.ResourceInfo.ASSETLOADSTATUS.SUCCESS:
                        resource = information.GetResource<T>();
                        break;
                    case ResourceManager.ResourceInfo.ASSETLOADSTATUS.LOADING:
                        CatLog.ELog("TryGetDictionarySync is Not Support Wait Asset Loading Status.");
                        break;
                    case ResourceManager.ResourceInfo.ASSETLOADSTATUS.FAILED:
                        CatLog.ELog("this Resource is Load Failed.");
                        break;
                }
            }

            return result;
        }
        
        #endregion
    }
}
