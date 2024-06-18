using System;
using System.Collections;
using System.Linq;
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
    public class ResourceManager : DynamicSingleton<ResourceManager> {
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
            private Action onCompleted = null;
            private bool isAddressablesAsset = false;
            private AsyncOperationHandle handle;
            public string ResourceName => Resource ? Resource.name : "NOT_LOADED";

            public static ResourceInfo Create(bool isGlobal) {
                var info = new ResourceInfo {
                    isGlobalResource = isGlobal,
                    isAddressablesAsset = false
                };
                return info;
            }

            public static ResourceInfo Create<T>(bool isGlobal, Action<T> onComplete = null) where T : UnityObject {
                var info = new ResourceInfo {
                    isGlobalResource = isGlobal,
                    isAddressablesAsset = true
                };
                info.onCompleted = Wrapped;
                return info;

                void Wrapped() {
                    onComplete?.Invoke(info.GetResource<T>());
                }
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
                    return;
                }

                Status = ASSETLOADSTATUS.FAILED;
            }
            
            public void SetResource<T>(T resource) where T : UnityObject {
                if (Status != ASSETLOADSTATUS.LOADING || Resource) {
                    CatLog.ELog("Status has already yielded results.");
                    return;
                }

                if (!resource) {
                    CatLog.ELog("Failed To Set Resource. Resource is Null.");
                    return;
                }

                Resource = resource;
                Status = ASSETLOADSTATUS.SUCCESS;
            }

            public T GetResource<T>() where T : UnityObject {
                if (Status != ASSETLOADSTATUS.SUCCESS || !Resource) {
                    CatLog.ELog("Reosurce Get Failed. Resource is Null or Status is Not Success.");
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

            public void AddCallback<T>(Action<T> callback) where T : UnityObject {
                onCompleted += Wrapped;
                return;

                void Wrapped() {
                    callback?.Invoke(GetResource<T>());
                }
            }

            public void TriggerCallback() {
                onCompleted?.Invoke();
                onCompleted = null;
            }
        }
        
        // Loaded Resources Dictioanry
        [SerializeField, Title("Loaded Resources Dictionary")]
        private StringResourceInformationDictionary resourcesDict = null;
        
        protected override void Initialize() {
            resourcesDict = new StringResourceInformationDictionary();
        }

        protected void Start() {
            SceneManager.Inst.OnSceneChangeBeforeEvent += OnSceneChangeBeforeEvent;
            SceneManager.Inst.OnSceneChangeAfterEvent += OnSceneChangeAfterEvent;
#if UNITY_EDITOR
            CheckDuplicatesInDictionary();
#endif
        }

        #region Events

        private void OnSceneChangeBeforeEvent(SceneName sceneName) {
            ReleaseAll();
        }

        private void OnSceneChangeAfterEvent(SceneName sceneName) {
            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region Resources

        public T ResourcesLoad<T>(string loadPath, bool isGlobal = false) where T : UnityObject {
            // Check Already Loaded Resources
            string fileName = GetFileName(loadPath);
            if (string.IsNullOrEmpty(fileName))
            {
                CatLog.ELog("Invalid Resources Load Path.");
                return null;
            }
            if (TryGetResourceSync(fileName, out T result)) {
                return result;
            }

            // Load Resource and Add to Dictionary
            var info = ResourceInfo.Create(isGlobal);
            resourcesDict.Add(fileName, info);
            result = Resources.Load<T>(loadPath);
            if (result) {
                info.SetResource<T>(result);
                return info.GetResource<T>();
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

        #region Addressables

        /// <summary>
        /// Addressables AssetLoadAsync by Addressables Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Default: Addressables Name</param>
        /// <param name="isGlobalResource"></param>
        /// <param name="onCompleted"></param>
        public void AddressablesAsyncLoad<T>(string key, bool isGlobalResource, Action<T> onCompleted) where T : UnityObject {
            if (string.IsNullOrEmpty(key))
            {
                CatLog.ELog("Invalid Key. Key is Null or Empty.");
                return;
            }
            
            // Dictionary에 이미 로드되거나 요청된 Resource가 존재한다면
            if (TryGetResourceAsync(key, onCompleted)) {
                return;
            }

            // 에셋이 로드중인 상태를 정의하기 위해 Dictionary에 미리 추가 (비동기 로드 전 로드중인 리소스임을 정의하기 위함)
            var info = ResourceInfo.Create<T>(isGlobalResource, onCompleted);
            resourcesDict.Add(key, info);
            Addressables.LoadAssetAsync<T>(key).Completed += (AsyncOperationHandle<T> operationHandle) => {
                info.SetHandle(operationHandle);
                if (operationHandle.Status != AsyncOperationStatus.Succeeded) {
                    info.SetFailed();
                    CatLog.ELog("ResourceMgr: Failed Addressables Async Load.");
                }
                else {
                    info.SetResource<T>(operationHandle.Result);
                }

                /*onCompleted?.Invoke(result);*/
                info.TriggerCallback();
            };
        }
        
        #endregion

        #region Release

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

        #region Find in Resource Dictionary
        
        private bool TryGetResourceSync<T>(string key, out T result) where T :UnityObject {
            var isExist = resourcesDict.TryGetValue(key, out ResourceInfo info);
            if (!isExist) {
                result = null;
                return false;
            }
            
            result = info.GetResource<T>();
            return true;
        }

        /// <summary>
        /// Find Resource in Dictionary and Return Result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private bool TryGetResourceAsync<T>(string key, Action<T> onCompleted) where T : UnityObject {
            var result = resourcesDict.TryGetValue(key, out ResourceInfo info);
            if (!result) { // Target Resource is Not Requested
                return false;
            }
            
            switch (info.Status) {
                case ResourceInfo.ASSETLOADSTATUS.SUCCESS:
                    onCompleted?.Invoke(info.GetResource<T>());
                    break;
                case ResourceInfo.ASSETLOADSTATUS.FAILED:
                    onCompleted?.Invoke(null);
                    CatLog.ELog("This Resource is Load Failed.");
                    break;
                case ResourceInfo.ASSETLOADSTATUS.LOADING:
                    info.AddCallback(onCompleted);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return true;
        }
        
        #endregion

        #region CHECK_DUPLICATES

        /// <summary>
        /// 마지막 Resource Dictionary추가 이후 n초간 대기 후 더 이상 추가되는 요소가 없다면 Dictionary내부 중복 요소 검사
        /// </summary>
        private void CheckDuplicatesInDictionary() {
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
            /*// Dictionary이미 로드된 Resource를 반환
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

            /*Addressables.Release(asyncOperationHandle); // Release Handle#1#
            return resourceInfo.SetResource<T>(result);*/
            throw new NotImplementedException();
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
            // return AddressablesSyncLoad<T>((string)assetRef.RuntimeKey, isMultipleSceenAllowedResources);
            throw new NotImplementedException();
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
            /*resource = null;
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

            return result;*/
            
            throw new NotImplementedException();
        }
        
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
            throw new NotImplementedException();
        }
        
        #endregion
    }
}
