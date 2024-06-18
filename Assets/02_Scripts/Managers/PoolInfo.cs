using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;

namespace CoffeeCat.FrameWork {
    [Serializable]
    public class PoolInfo {
        // Staic Default Init Spawn Count
        public const int DEFAULT_SPAWN_COUNT = 10;

        public enum LoadType { 
            None,
            Caching,
            Resource_Load,
            Addressables_Key,
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

        // Object Spawn Options
        [BoxGroup("Spawn Option")] 
        public int InitSpawnCount = DEFAULT_SPAWN_COUNT;

        [BoxGroup("Spawn Option")]
        public Transform CustomRootParent = null;

        public bool HasCustomRootParent => !CustomRootParent;

        public bool HasRootParent { get; private set; } = false;

        public bool IsStayEnabling { get; private set; } = false;

        #region INSPECTOR BUTTONS
        
        [ButtonGroup("PoolObject/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void Caching()
        {
            PoolObjectLoadType = LoadType.Caching;
            Clear();
        }

        [ButtonGroup("PoolObject/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void Resources()
        {
            PoolObjectLoadType = LoadType.Resource_Load;
            Clear();
        }

        [ButtonGroup("PoolObject/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void Addressables()
        {
            PoolObjectLoadType = LoadType.Addressables_Key;
            Clear();
        }

        #endregion

        private void Clear() {
            AddressablesName = string.Empty;
            ResourcesPath = string.Empty;
            PoolObject = null;
            CustomRootParent = null;
        }

        public static PoolInfo Create(GameObject poolObject, int initSpawnCount = DEFAULT_SPAWN_COUNT, Transform customRootParent = null) {
            return new PoolInfo() {
                PoolObject = poolObject,
                InitSpawnCount = initSpawnCount,
                CustomRootParent = customRootParent,
                PoolObjectLoadType = LoadType.Custom
            };
        }

        public void LoadOriginPrefab(Action<PoolInfo> onComplete = null) {
            switch (PoolObjectLoadType) {
                case LoadType.Resource_Load:
                    // Check Path Included Extensions
                    int fileExtensionPosition = ResourcesPath.LastIndexOf(".", StringComparison.Ordinal);
                    if (fileExtensionPosition >= 0) { // Remove Paths Extension
                        ResourcesPath = ResourcesPath.Substring(0, fileExtensionPosition);
                    }
                    PoolObject = ResourceManager.Inst.ResourcesLoad<GameObject>(ResourcesPath, false);
                    break;
                case LoadType.Addressables_Key:
                    ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(AddressablesName, false, loadedObject => {
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
        public static implicit operator bool(PoolInfo info) => info != null;

        #region CONSTRUCTOR
        /// <summary>
        /// Not allowed public Constructor. using PoolInformation.New() static function.
        /// </summary>
        private PoolInfo() {}
        #endregion
    }
}
