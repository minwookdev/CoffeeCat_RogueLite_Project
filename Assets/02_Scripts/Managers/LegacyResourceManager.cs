using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityObject = UnityEngine.Object;
using UniRx;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;
using CoffeeCat.Utils.SerializedDictionaries;
using CoffeeCat.Utils.Defines;
using CoffeeCat.FrameWork;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.AddressableAssets.Initialization;

namespace CoffeeCat.Legacy {

    public class LegacyResourceManager : DynamicSingleton<LegacyResourceManager> {
        // Resources Dictionary
        [SerializeField, Title("Resource Dictionary")]
        Dictionary<string, ResourceInformation> resourcesDict = null;
        List<string> tempStrList = new List<string>();

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
             
            public static ResourceInformation New(UnityObject resource, bool isMultipleSceneAllowedResource) {
                var result = new ResourceInformation() {
                    Resource = resource,
                    IsMultipleSceneAllowed = isMultipleSceneAllowedResource
                };

                return result;
            }

            public static ResourceInformation New(bool isMultipleSceneAllowedResource) {
                return new ResourceInformation() {
                    IsMultipleSceneAllowed = isMultipleSceneAllowedResource
                };
            }

            public void SetStatusFailed() {
                if (Status != ASSETLOADSTATUS.LOADING || Resource != null) {
                    CatLog.Log("Status has already yielded results.");
                    return;
                }

                Status = ASSETLOADSTATUS.FAILED;
            }

            public T SetResource<T>(T resource) where T : UnityObject {
                if (this.Resource != null || resource == null) {
                    CatLog.Log("Resource Set Failed.");
                    return null;
                }

                this.Resource = resource;
                Status = ASSETLOADSTATUS.SUCCESS;
                return GetResource<T>();
            }

            public T CastingResource<T>() where T : UnityObject {
                var result = Resource as T;
                if (result == null) {
                    CatLog.ELog($"Resource Casting Failed. Resource Name: {Resource.name}, Casting Try: {nameof(T)}");
                }
                return result;
            }

            public string GetResourceName() => Resource.name;

            public void ClearResource() {
                //Destroy(Resource);
                this.Resource = null;
            }

            public T GetResource<T>() where T : UnityObject {
                UsingCount++;
                return (T)Resource;
            }

            public bool TryCastingResource<T>(out T result) where T : UnityObject {
                result = null;

                // Resource is Null
                if (Resource == null) {
                    CatLog.ELog("Resource Is Null !");
                    return false;
                }

                result = Resource as T;
                // Casting Failed
                if (result == null) {
                    CatLog.ELog($"Loaded Resource Casting Failed. Resource Name: {Resource.name}, Try Casting Type: {nameof(T)}");
                    return false;
                }

                // Casting Success
                return true;
            }

            public bool IsCheckResource<T>() where T : UnityObject {
                // Resource is Null
                if (Resource == null) {
                    CatLog.ELog("Resource Is Null !");
                    return false;
                }

                // Casting Failed
                if (Resource is T == false) {
                    CatLog.ELog($"Loaded Resource Casting Failed. Resource Name: {Resource.name}, Try Casting Type: {nameof(T)}");
                    return false;
                }

                // Resource is Ready to Use
                return true;
            }

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
        }

        protected override void Initialize() => resourcesDict = new Dictionary<string, ResourceInformation>();

        protected void Start() {
            SceneManager.Instance.OnSceneChangeBeforeEvent += OnSceneChangeBeforeEvent;
        }

        #region RESOURCES LOAD

        public T ResourcesLoad<T>(string loadPath, bool isAllSceneAllowedResources = false) where T : UnityObject {
            // Get FileName
            string fileName = GetFileName(loadPath);

            // Check Already Loaded Resources
            if (TryGetResourceInDictionary(fileName, out T result)) {
                return result;
            }

            // Load Resource and Add to Dictionary
            result = Resources.Load<T>(loadPath);
            if (result == null) {
                CatLog.ELog($"Not Exist File(name:{fileName}) in Resources Folder or Load Type is MissMatached.");
                return null;
            }

            AddResourceInDictionaryWithCheckDuplicate(fileName, result, isAllSceneAllowedResources);
            return result;
        }

        public bool ResourcesLoad<T>(string loadPath, out T tResult, bool isAllSceneAllowedResources = false) where T : UnityObject {
            // Get FileName
            string fileName = GetFileName(loadPath);

            // Check Already Loaded Resources
            if (TryGetResourceInDictionary(fileName, out tResult)) {
                return tResult != null;
            }

            // Load Reosurce and Add to Dictionary
            tResult = Resources.Load<T>(loadPath);
            if (tResult == null) {
                CatLog.ELog($"Not Exist File(name:{fileName}) in Resources Folder or Load Type is MissMatached.");
                return false;
            }

            AddResourceInDictionaryWithCheckDuplicate(fileName, tResult, isAllSceneAllowedResources);
            return true;
        }

        private string GetFileName(string resourcesLoadPath) => resourcesLoadPath.Substring(resourcesLoadPath.LastIndexOf('/') + 1);

        #endregion

        #region ADDRESSABLES LOAD SYNCHRONOUS

        public T AddressablesLoadSync<T>(string addressablesName, bool isAllSceneAllowedResource = false, Action<T> onSuccessfullyCompletedAction = null) where T : UnityObject {
            if (TryGetResourceInDictionary(addressablesName, out T result, onSuccessfullyCompletedAction)) {
                return result;
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(addressablesName);
            result = asyncOperationHandle.WaitForCompletion(); // Block Code Until Completed LoadAssetAsync Operation.
            if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded) {
                CatLog.ELog("Addressables AssetLoadAsync Failed.");
                return null;
            }

            Addressables.Release(asyncOperationHandle); // Release AsyncOperationHandle
            AddResourceInDictionaryWithCheckDuplicate(addressablesName, result, isAllSceneAllowedResource);
            onSuccessfullyCompletedAction?.Invoke(result);
            return result;
        }

        public T AddressablesLoadSync<T>(AssetReference assetRef, bool isAllSceneAllowedResource = false, Action<T> onSuccessfullyCompletedAction = null) where T : UnityObject {
            if (TryGetResourceInDictionary(assetRef.RuntimeKey.ToString(), out T result, onSuccessfullyCompletedAction)) {
                return result;
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(assetRef);
            result = asyncOperationHandle.WaitForCompletion();
            if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded) {
                CatLog.ELog("Addressables AssetLoadAsync Failed.");
                return null;
            }

            Addressables.Release(asyncOperationHandle);
            AddResourceInDictionaryWithCheckDuplicate(assetRef.RuntimeKey.ToString(), result, isAllSceneAllowedResource);
            onSuccessfullyCompletedAction?.Invoke(result);
            return result;
        }

        #endregion

        #region ADDRESSABELS LOAD ASYNCHRONOUS

        public void AddressablesLoadAsync<T>(string addressablesName, bool isAllSceneAllowedResource, Action<T> onSuccessfullyCompletedAction = null) where T : UnityObject {
            if (TryGetResourceInDictionary(addressablesName, out T result, onSuccessfullyCompletedAction)) {
                return;
            }

            Addressables.LoadAssetAsync<T>(addressablesName).Completed += (AsyncOperationHandle<T> handle) => {
                if (handle.Status == AsyncOperationStatus.Succeeded) {
                    result = handle.Result;
                    AddResourceInDictionaryWithCheckDuplicate(addressablesName, result, isAllSceneAllowedResource);
                    onSuccessfullyCompletedAction?.Invoke(result);
                }
                else {
                    CatLog.ELog("Addressables AssetLoadAsync Failed.");
                }

                Addressables.Release(handle);
            };
        }

        public void AddressablesLoadAsync<T>(AssetReference assetRef, bool isAllSceneAllowedResource, Action<T> onSuccessfullyCompletedAction = null) where T : UnityObject {
            if (TryGetResourceInDictionary(assetRef.RuntimeKey.ToString(), out T result, onSuccessfullyCompletedAction)) {
                return;
            }

            Addressables.LoadAssetAsync<T>(assetRef).Completed += (AsyncOperationHandle<T> handle) => {
                if (handle.Status == AsyncOperationStatus.Succeeded) {
                    result = handle.Result;
                    AddResourceInDictionaryWithCheckDuplicate(assetRef.RuntimeKey.ToString(), result, isAllSceneAllowedResource);
                    onSuccessfullyCompletedAction?.Invoke(result);
                }
                else {
                    CatLog.ELog("Addressables AssetLoadAsync Failed.");
                }

                Addressables.Release(handle);
            };
        }

        #endregion

        #region ADDRESSABLES LOAD NON-DICTIONARY

        public void AddressablesLoadAsyncNonDict<T>(string addressablesName, Action<T> onCompletedAction = null) where T : UnityObject {
            Addressables.LoadAssetAsync<T>(addressablesName).Completed += (AsyncOperationHandle<T> handle) => {
                if (handle.Status == AsyncOperationStatus.Succeeded) {
                    onCompletedAction?.Invoke(handle.Result);
                }
                else {
                    CatLog.ELog("Addressables AssetLoadAsync Failed.");
                }

                Addressables.Release(handle);
            };
        }

        public T AddressablesLoadSyncNonDict<T>(string addressablesName, Action<T> onCompletedAction = null) where T : UnityObject {
            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(addressablesName);
            T result = asyncOperationHandle.WaitForCompletion();
            if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded) {
                CatLog.ELog("Addressables AssetLoadAsync Failed.");
                return null;
            }

            Addressables.Release(asyncOperationHandle);
            onCompletedAction?.Invoke(result);
            return result;
        }

        #endregion

        #region ADDRESSABLES INSTANTIATE

        public void AddressablesInstantiateAsync(string addressablesName, Action<GameObject> onCompletedAction = null) {
            Addressables.InstantiateAsync(addressablesName).Completed += (AsyncOperationHandle<GameObject> handle) => {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    onCompletedAction?.Invoke(handle.Result);
            };
        }

        public void AddressablesInstantiateAsync<T>(AssetReference assetRef, Action<GameObject> onCompletedAction = null) {
            Addressables.InstantiateAsync(assetRef).Completed += (AsyncOperationHandle<GameObject> handle) => {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    onCompletedAction?.Invoke(handle.Result);
            };
        }

        #endregion

        /// <summary>
        /// Try Get Resource in Dictionary and Casting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="tResult"></param>
        /// <param name="onSuccessfullyCompletedAction"></param>
        /// <returns></returns>
        public bool TryGetResourceInDictionary<T>(string key, out T tResult, Action<T> onSuccessfullyCompletedAction = null) where T : UnityObject {
            tResult = null;
            var result = resourcesDict.TryGetValue(key, out ResourceInformation information);
            if (result) {
                CatLog.Log($"this Resource is Already Loaded. Name: {information.GetResourceName()}");
                tResult = information.CastingResource<T>();
                if (tResult != null) {
                    onSuccessfullyCompletedAction?.Invoke(tResult);
                }
            }
            return result;
        }

        /// <summary>
        /// Check Duplicated Reosurce In Dictionary After Add To Dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <param name="isAllSceneAllowedResource"></param>
        private void AddResourceInDictionaryWithCheckDuplicate(string key, UnityObject resource, bool isAllSceneAllowedResource) {
            CheckDuplicatedReosurceInDictionary(resource.name);
            resourcesDict.Add(key, ResourceInformation.New(resource, isAllSceneAllowedResource));

            // find duplicated resource in dictionary by name
            void CheckDuplicatedReosurceInDictionary(string loadedResourceName) {
                foreach (var keyValuePair in resourcesDict) {
                    if (keyValuePair.Value.GetResourceName().Equals(loadedResourceName))
                        CatLog.ELog($"Find Duplicated UnityObjects in ResourcesDictioanry. Object name: {loadedResourceName}");
                }
            }
        }

        public void RemoveResourceInDictionary(string key) {
            if (!resourcesDict.Remove(key, out ResourceInformation resource)) {
                //CatLog.WLog($"Resource Not Deleted in Dictionary. Key Not Found. Key: {key}");
                return;
            }

            resource.ClearResource();
        }

        private void OnSceneChangeBeforeEvent(SceneName sceneName) {
            ClearResources(false);
        }

        /// <summary>
        /// Clear Resources Dictionary
        /// </summary>
        /// <param name="isClearAllSceneAllowedResourcesDictionary"></param>
        public void ClearResources(bool isClearAllSceneAllowedResourcesDictionary = false) {
            // Remove Only Current Scene Allowed Loaded Resources
            if (!isClearAllSceneAllowedResourcesDictionary) {
                foreach (KeyValuePair<string, ResourceInformation> keyValuePair in resourcesDict) {
                    var resourceInformation = keyValuePair.Value;
                    if (resourceInformation.IsMultipleSceneAllowed)
                        return;

                    resourceInformation.ClearResource();
                    tempStrList.Add(keyValuePair.Key);
                }

                foreach (var key in tempStrList) {
                    resourcesDict.Remove(key);
                }
            }
            // Remove All Loaded Resources
            else {
                foreach (KeyValuePair<string, ResourceInformation> keyValuePair in resourcesDict) {
                    keyValuePair.Value.ClearResource();
                }

                resourcesDict.Clear();
            }
        }

        #region TEMP

        private void SubscribeCheckDuplicatedResourceInDictionaryObservable(bool isPrintDetail = false) {
            this.ObserveEveryValueChanged(_ => resourcesDict.Count)
                .Where(dictCount => dictCount > 0)
                .Skip(TimeSpan.Zero)
                .Subscribe(_ => {
                    tempStrList.Clear();
                    foreach (var keyValuePair in resourcesDict) {
                        tempStrList.Add(keyValuePair.Value.GetResourceName());
                        //CatLog.Log($"Reosurces Name: {keyValuePair.Value.GetResourceName()}");
                    }

                    if (tempStrList.Count != tempStrList.Distinct().Count()) {
                        CatLog.ELog("Find Duplicated Resource in ResourcesDictionary. Check the Details.");
                        if (isPrintDetail) {
                            var duplicates = tempStrList.GroupBy(i => i)
                                                     .Where(g => g.Count() > 1)
                                                     .Select(g => g.Key);
                            foreach (var duplicateString in duplicates) {
                                CatLog.ELog($"Duplicate ResourceName: {duplicateString}");
                            }
                        }
                    }

                    tempStrList.Clear();
                })
                .AddTo(this);
        }

        #endregion
    }
}
