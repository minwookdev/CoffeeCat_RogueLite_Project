using System;
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
        // Loaded Resources Dictioanry
        [SerializeField, Title("Loaded Resources Dictionary")]
        StringResourceInformationDictionary resourcesDict = null;

        [Serializable]
        public class ResourceInformation {
            public enum ASSETLOADSTATUS {
                LOADING,
                FAILED,
                SUCCESS
            }

            [ShowInInspector, ReadOnly] public UnityObject Resource { get; private set; } = null;
            [ShowInInspector, ReadOnly] public bool IsMultipleSceneAllowed { get; private set; } = false;
            [ShowInInspector, ReadOnly] public ASSETLOADSTATUS Status { get; private set; } = ASSETLOADSTATUS.LOADING;
            [ShowInInspector, ReadOnly] public int UsingCount { get; private set; } = 0;
            public string ResourceName {
                get => Resource.name;
            }

            #region CONSTRUCTORS

            public static ResourceInformation New(bool isMultipleSceneAllowedResource) {
                return new ResourceInformation() {
                    IsMultipleSceneAllowed = isMultipleSceneAllowedResource
                };
            }

            #endregion

            public void SetStatusFailed() {
                if (Status != ASSETLOADSTATUS.LOADING || Resource != null) {
                    CatLog.Log("Status has already yielded results.");
                    return;
                }

                Status = ASSETLOADSTATUS.FAILED;
            }

            #region RESOURCES

            public T SetResource<T>(T resource) where T : UnityObject {
                if (this.Resource != null || resource == null) {
                    CatLog.Log("Resource Set Failed.");
                    return null;
                }

                this.Resource = resource;
                Status = ASSETLOADSTATUS.SUCCESS;
                return GetResource<T>();
            }

            public T GetResource<T>() where T : UnityObject {
                UsingCount++;
                return (T)Resource;
            }

            #endregion

            #region RELEASE

            public bool IsRemoveable() {
                UsingCount--;
                if (UsingCount <= 0) {
                    Resource = null;
                    return true;
                }
                return false;
            }

            public void Remove() {
                Resource = null;
            }

            #endregion
        }

        protected override void Initialize() {
            resourcesDict = new StringResourceInformationDictionary();
        }

        protected void Start() {
            SceneManager.Instance.OnSceneChangeBeforeEvent += OnSceneChangeBeforeEvent;
#if UNITY_EDITOR
            CheckDuplicatesInDictionary();
#endif
        }

        #region EVENTS

        private void OnSceneChangeBeforeEvent(SceneName sceneName) {
            this.ReleaseAll();
        }

        #endregion

        #region RESOURCES_LOAD

        public T ResourcesLoad<T>(string loadPath, bool isMultipleSceneAllowedResource = false) where T : UnityObject {
            // Get FileName
            string fileName = GetFileName(loadPath);

            // Check Already Loaded Resources
            if (TryGetResourceInDictionarySync<T>(fileName, out T result)) {
                return result;
            }

            // Load Resource and Add to Dictionary
            result = Resources.Load<T>(loadPath);
            if (result == null) {
                CatLog.ELog($"Not Exist File(name:{fileName}) in Resources Folder or Load Type is MissMatached.");
                return null;
            }

            resourcesDict.Add(fileName, ResourceManager.ResourceInformation.New(isMultipleSceneAllowedResource));
            return resourcesDict[fileName].SetResource<T>(result);
        }

        public bool ResourcesLoad<T>(string loadPath, out T tResult, bool isMultipleSceneAllowedResource = false) where T : UnityObject {
            tResult = ResourcesLoad<T>(loadPath, isMultipleSceneAllowedResource);
            if (tResult == null) {
                return false;
            }

            return true;
        }

        private string GetFileName(string resourcesLoadPath) => resourcesLoadPath.Substring(resourcesLoadPath.LastIndexOf('/') + 1);

        #endregion

        #region ADDRESSABLES_LOAD

        /// <summary>
        /// Addressables AssetLoadAsync by Addressables Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Default: Addressables Name</param>
        /// <param name="isMultipleSceneAllowedResources"></param>
        /// <param name="onCompleted"></param>
        public void AddressablesAsyncLoad<T>(string key, bool isMultipleSceneAllowedResources, Action<T> onCompleted) where T : UnityObject {
            // Dictionary에 이미 로드되거나 요청된 Resource가 존재한다면
            if (TryGetResourceInDictionaryAsync<T>(key, onCompleted)) {
                return;
            }

            // 에셋이 로드중인 상태를 정의하기 위해 Dictionary에 미리 추가 (비동기 로드 전 로드중인 리소스임을 정의하기 위함)
            var resourceInfo = ResourceManager.ResourceInformation.New(isMultipleSceneAllowedResources);
            resourcesDict.Add(key, resourceInfo);
            Addressables.LoadAssetAsync<T>(key).Completed += (AsyncOperationHandle<T> operationHandle) => {
                if (operationHandle.Status != AsyncOperationStatus.Succeeded) {
                    resourceInfo.SetStatusFailed();
                    CatLog.ELog("ResourceManager: Addressables Async Load Failed !");
                }

                onCompleted.Invoke(resourceInfo.SetResource<T>(operationHandle.Result));
                /*Addressables.Release(operationHandle);*/
            };
        }

        /// <summary>
        /// Addressables AssetLoadAsync By AssetReference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetRef"></param>
        /// <param name="isMultipleSceneAllowedResources"></param>
        /// <param name="onCompleted"></param>
        public void AddressablesAsyncLoad<T>(AssetReference assetRef, bool isMultipleSceneAllowedResources, Action<T> onCompleted) where T : UnityObject {
            this.AddressablesAsyncLoad<T>((string)assetRef.RuntimeKey, isMultipleSceneAllowedResources, onCompleted);
            // assetReference.RuntimeKey is GUID
        }

        /// <summary>
        /// Addressables AssetLoadSync By Addressables Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Default: AddressablesName</param>
        /// <param name="isMultipleSceenAllowedResources"></param>
        /// <returns></returns>
        public T AddressablesSyncLoad<T>(string key, bool isMultipleSceenAllowedResources) where T : UnityObject {
            // Dictionary이미 로드된 Resource를 반환
            if (TryGetResourceInDictionarySync<T>(key, out T resource)) {
                return resource;
            }

            // Addressables를 통한 에셋 로드 요청
            resourcesDict.Add(key, ResourceInformation.New(isMultipleSceenAllowedResources));
            var resourceInfo = resourcesDict[key];
            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(key);
            T result = asyncOperationHandle.WaitForCompletion(); // 결과를 받을 때 까지 동기방식으로 대기
            if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded) {
                resourceInfo.SetStatusFailed();
                CatLog.ELog("Addressables AssetSyncLoad Failed.");
                return null;
            }

            /*Addressables.Release(asyncOperationHandle); // Release Handle*/
            return resourceInfo.SetResource<T>(result);

            return null;
        }

        /// <summary>
        /// Addressables AssetLoadAsync By AssetReference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetRef"></param>
        /// <param name="isMultipleSceenAllowedResources"></param>
        /// <returns></returns>
        public T AddressablesSyncLoad<T>(AssetReference assetRef, bool isMultipleSceenAllowedResources) where T : UnityObject {
            return AddressablesSyncLoad<T>((string)assetRef.RuntimeKey, isMultipleSceenAllowedResources);
        }

        #endregion

        #region RELEASE

        public void Release(string key) {
            if (resourcesDict.TryGetValue(key, out ResourceManager.ResourceInformation resourceInfo) == false) {
                CatLog.ELog($"Not Found this Key in ResourceDictionary. Key: {key}");
                return;
            }

            if (resourceInfo.IsRemoveable()) {
                resourcesDict.Remove(key);
            }
        }

        public void Release(AssetReference assetRef) {
            this.Release((string)assetRef.RuntimeKey);
        }

        public void ReleaseAll(bool isClearMultipleSceneAllowedResource = false) {
            if (isClearMultipleSceneAllowedResource) {
                foreach (var keyValuePair in resourcesDict) {
                    keyValuePair.Value.Remove();
                }
                resourcesDict.Clear();
            }
            else {
                var tempKeyList = new List<string>();
                foreach (var keyValuePair in resourcesDict) {
                    if (keyValuePair.Value.IsMultipleSceneAllowed == false) {
                        keyValuePair.Value.Remove();
                        tempKeyList.Add(keyValuePair.Key);
                    }
                }

                for (int i = 0; i < tempKeyList.Count; i++) {
                    resourcesDict.Remove(tempKeyList[i]);
                }
            }
        }

        #endregion

        #region SEARCH_DICTIONARY

        /// <summary>
        /// Resource Dictionary에서 로드된 리소스를 찾고 결과를 반환. (Async전용)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private bool TryGetResourceInDictionaryAsync<T>(string key, Action<T> onCompleted) where T : UnityObject {
            var result = resourcesDict.TryGetValue(key, out ResourceInformation information);
            if (result) {
                switch (information.Status) {
                    case ResourceManager.ResourceInformation.ASSETLOADSTATUS.SUCCESS:
                        onCompleted?.Invoke(information.GetResource<T>());
                        break;
                    case ResourceManager.ResourceInformation.ASSETLOADSTATUS.FAILED:
                        CatLog.ELog("Resource Load Failed.");
                        break;
                    case ResourceManager.ResourceInformation.ASSETLOADSTATUS.LOADING:
                        this.UpdateAsObservable()
                            .Skip(TimeSpan.Zero)
                            .First()
                            .Select(_ => information.Status)
                            .DistinctUntilChanged()
                            .DoOnSubscribe(() => { CatLog.Log("Wait Already Loading Resources..."); })
                            .Subscribe(status => {
                                if (information.Status != ResourceManager.ResourceInformation.ASSETLOADSTATUS.SUCCESS) {
                                    CatLog.ELog("Resource Load Failed.");
                                    return;
                                }
                                onCompleted.Invoke(information.GetResource<T>());
                            })
                            .AddTo(this);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Resource Dictionary에서 로드된 리소스를 찾고 결과를 반환. (Sync전용)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private bool TryGetResourceInDictionarySync<T>(string key, out T resource) where T : UnityObject {
            resource = null;
            var result = resourcesDict.TryGetValue(key, out ResourceInformation information);
            if (result) {
                switch (information.Status) {
                    case ResourceManager.ResourceInformation.ASSETLOADSTATUS.SUCCESS:
                        resource = information.GetResource<T>();
                        break;
                    case ResourceManager.ResourceInformation.ASSETLOADSTATUS.LOADING:
                        CatLog.ELog("TryGetDictionarySync is Not Support Wait Asset Loading Status.");
                        break;
                    case ResourceManager.ResourceInformation.ASSETLOADSTATUS.FAILED:
                        CatLog.ELog("this Resource is Load Failed.");
                        break;
                }
            }

            return result;
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
#if UNITY_EDITOR
            //this.ObserveEveryValueChanged(_ => resourcesDict.Count)
            //    .Throttle(TimeSpan.FromSeconds(waitSeconds))
            //    .Select(_ => this.resourcesDict)
            //    .Where(dict => dict.Count > 1)
            //    .Skip(TimeSpan.Zero)
            //    .TakeUntilDestroy(this)
            //    .Subscribe(dict => { 
            //        
            //    })
            //    .AddTo(this);

            //List<string> strList = new List<string>();
            //this.ObserveEveryValueChanged(_ => resourcesDict.Count)
            //    .Skip(TimeSpan.Zero)
            //    .Select(_ => resourcesDict)
            //    .TakeUntilDestroy(this)
            //    .Subscribe(dictionary => {
            //        // Resource Dictionary에 요소가 추가됨
            //        if (previousCheckedDictioanryCount > resourcesDict.Count) {
            //            //// Dictionary를 순회하며 중복되는 ResourceName을 체크
            //            //foreach (var keyValuePair in resourcesDict) {
            //            //    //strList.Add(keyValuePair.Value.Resource.name);
            //            //    string resourceName = keyValuePair.Value.ResourceName;
            //            //    for (int i = 0; i < strList.Count; i++) {
            //            //        if (resourceName.Equals(strList[i])) {
            //            //            CatLog.ELog($"Checked Duplicate Loaded Resources: Name {resourceName}");
            //            //        }
            //            //    }
            //            //    strList.Add(resourceName);
            //            //}
            //            //
            //            //strList.Clear();
            //
            //            // 동일한 Resource Name을 가진 Value가 존재하는지 체크
            //            isExistDuplicateResourceInDictionary = dictionary.Values.GroupBy(x => x.ResourceName).Any(g => g.Count() > 1);
            //            if (isExistDuplicateResourceInDictionary) {
            //                CatLog.ELog("Checked Duplicate Loaded Resources.");
            //            }
            //        }
            //
            //        CatLog.WLog("Checked !");
            //        previousCheckedDictioanryCount = resourcesDict.Count;
            //    })
            //    .AddTo(this);

            int previousCheckedDictioanryCount = 0;
            bool isExistDuplicateResourceInDictionary = false;
            this.UpdateAsObservable()
                .Skip(TimeSpan.Zero)
                .Select(_ => resourcesDict)
                .Where(dict => dict.Count != previousCheckedDictioanryCount)
                .TakeUntilDestroy(this)
                .Subscribe(dict => {
                    //if (previousCheckedDictioanryCount < dict.Count) {
                    //    isExistDuplicateResourceInDictionary = dict.Values.GroupBy(x => x.ResourceName).Any(g => g.Count() > 1);
                    //    if (isExistDuplicateResourceInDictionary) {
                    //        CatLog.ELog("Checked Duplicate Loaded Resources.");
                    //    }
                    //    previousCheckedDictioanryCount = dict.Count;
                    //}
                    //
                    //if (previousCheckedDictioanryCount != dict.Count) {
                    //    if (dict.Count > previousCheckedDictioanryCount) {
                    //        // Duplicate Check 
                    //    }
                    //
                    //    previousCheckedDictioanryCount = dict.Count;
                    //}

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
    }
}
