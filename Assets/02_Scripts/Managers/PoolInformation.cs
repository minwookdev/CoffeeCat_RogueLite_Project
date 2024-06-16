using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;

namespace CoffeeCat.FrameWork {
    [Serializable]
    public class PoolInformation {
        // Staic Default Init Spawn Count
        public const int DEFAULT_SPAWN_COUNT = 10;

        public enum LoadType { 
            None,
            Caching,
            Resource_Load,
            Addressables_Key,
            Addressables_AssetRef,
            Custom,
        }

        // Pool Object Load Type (ReadOnly)
        [TitleGroup("PoolObject")]
        [ReadOnly] public LoadType PoolObjectLoadType = LoadType.None;

        // Pool Object Load Required
        [BoxGroup("Pool Object"), EnableIf(nameof(PoolObjectLoadType), LoadType.Caching)] 
        public GameObject PoolObject = null;

        [BoxGroup("Pool Object"), ShowIf(nameof(PoolObjectLoadType), LoadType.Resource_Load), FilePath(ParentFolder = "Assets/Resources")] 
        public string ResourcesPath = string.Empty;

        [BoxGroup("Pool Object"), ShowIf(nameof(PoolObjectLoadType), LoadType.Addressables_Key)] 
        public string AddressablesName = string.Empty;

        [BoxGroup("Pool Object"), ShowIf(nameof(PoolObjectLoadType), LoadType.Addressables_AssetRef)] 
        public AssetReference AssetRef = null;

        // Object Spawn Options
        [BoxGroup("Spawn Option")] 
        public int InitSpawnCount = DEFAULT_SPAWN_COUNT;

        [BoxGroup("Spawn Option"), ReadOnly] 
        public bool IsStayEnabling = false;

        [BoxGroup("Spawn Option")] 
        public bool HasRootParent = true;

        [BoxGroup("Spawn Option"), ShowIf(nameof(HasRootParent), true)]
        public Transform CustomRootParent = null;

        public bool HasCustomRootParent { get => CustomRootParent != null; }

        #region INSPECTOR BUTTONS
        
        [ButtonGroup("PoolObject/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void Caching()
        {
            PoolObjectLoadType = LoadType.Caching;
            Clear();
        }

        [ButtonGroup("PoolObject/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void ResourcesLoad()
        {
            PoolObjectLoadType = LoadType.Resource_Load;
            Clear();
        }

        [ButtonGroup("PoolObject/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void AddressablesKey()
        {
            PoolObjectLoadType = LoadType.Addressables_Key;
            Clear();
        }

        [ButtonGroup("PoolObject/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void AssetReference()
        {
            PoolObjectLoadType = LoadType.Addressables_AssetRef;
            Clear();
        }

        #endregion

        private void Clear() {
            AssetRef = null;
            AddressablesName = string.Empty;
            ResourcesPath = string.Empty;
            PoolObject = null;
            CustomRootParent = null;
        }

        public static PoolInformation Create(GameObject poolObject, bool hasRootParent = true, int initSpawnCount = DEFAULT_SPAWN_COUNT, Transform customRootParent = null) {
            return new PoolInformation() {
                PoolObject = poolObject,
                InitSpawnCount = initSpawnCount,
                HasRootParent = hasRootParent,
                CustomRootParent = customRootParent,
                PoolObjectLoadType = LoadType.Custom
            };
        }

        public void LoadOriginPrefab(Action<PoolInformation> onComplete = null) {
            switch (PoolObjectLoadType) {
                case LoadType.Resource_Load:
                    // Check Path Included Extensions
                    int fileExtensionPosition = ResourcesPath.LastIndexOf(".", StringComparison.Ordinal);
                    if (fileExtensionPosition >= 0) { // Remove Paths Extension
                        ResourcesPath = ResourcesPath.Substring(0, fileExtensionPosition);
                    }
                    PoolObject = ResourceManager.Instance.ResourcesLoad<GameObject>(ResourcesPath, false);
                    break;
                case LoadType.Addressables_Key:
                    ResourceManager.Instance.AddressablesAsyncLoad<GameObject>(AddressablesName, false, loadedObject => {
                        if (!loadedObject) {
                            CatLog.ELog("PoolInfo.LoadOriginPrefab() Failed. Addressables Load Failed.");
                            return;
                        }
                        PoolObject = loadedObject;
                        onComplete?.Invoke(this);
                    });
                    return;
                case LoadType.Caching:
                case LoadType.Custom:
                case LoadType.None:
                    break;
                case LoadType.Addressables_AssetRef:
                default: 
                    throw new NotImplementedException();
            }

            if (!PoolObject) {
                CatLog.ELog("Origin Prefab is Null.");
                return;
            }
            
            onComplete?.Invoke(this);
        }

        /// <summary>
        /// Bool Implicit Operator 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static implicit operator bool(PoolInformation info) => info != null;

        #region CONSTRUCTOR
        /// <summary>
        /// Not allowed public Constructor. using PoolInformation.New() static function.
        /// </summary>
        private PoolInformation() {}
        #endregion
    }
}
