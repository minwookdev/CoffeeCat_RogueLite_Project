namespace DonutMonsterDev {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.AddressableAssets.ResourceLocators;
    using PoolDictKey = System.String;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using CoffeeCat.Utils;

    public class CatObjectPoolManager : MonoBehaviour {
        public List<PoolObjectInfo> poolInfoList = new List<PoolObjectInfo>();
        Dictionary<PoolDictKey, PoolObjectInfo> poolInfoDict = new Dictionary<PoolDictKey, PoolObjectInfo>();
        Dictionary<PoolDictKey, Stack<GameObject>> poolDict = new Dictionary<PoolDictKey, Stack<GameObject>>();
    }

    [System.Serializable]
    public class PoolObjectInfo {
        public Transform loopParentTr = null;
        public int initPoolSize = 10;
        public int maxPoolSize = 100;
        public PoolDictKey key;
        public ASSETLOADTYPE loadType = ASSETLOADTYPE.NONE;
        public GameObject prefabGO = null;
        public AssetReference prefabAssetRef = null;
        public string addressableKey;
    }

    public enum ASSETLOADTYPE {
        NONE,
        DEFAULT,
        ADDRESSABLE,
    }
#if UNITY_EDITOR
    //[CustomEditor(typeof(PoolObjectInfo))]
    //public class PoolObjectInfoEditor : Editor
    //{
    //    private PoolObjectInfo poolInfo = null;
    //
    //    private SerializedObject sobject = null;
    //    private SerializedProperty loadTypeProp = null;
    //    private SerializedProperty gameObjectPrefabProp = null;
    //    private SerializedProperty assetRefProp = null;
    //    private SerializedProperty addressableKeyProp = null;
    //
    //    private void OnEnable()
    //    {
    //        poolInfo = (PoolObjectInfoEditor)target;
    //        sobject = new SerializedObject(target);
    //        loadTypeProp = sobject.FindProperty(nameof(PoolObjectInfo.loadType));
    //        gameObjectPrefabProp = sobject.FindProperty(nameof(PoolObjectInfo.prefabGO));
    //        addressableKeyProp = sobject.FindProperty(nameof(PoolObjectInfo.addressableKey));
    //
    //    }
    //
    //    public override void OnInspectorGUI()
    //    {
    //        //base.OnInspectorGUI();
    //
    //        sobject.Update();
    //
    //        switch (loadTypeProp)
    //        {
    //            default:
    //                break;
    //        }
    //
    //        sobject.ApplyModifiedProperties();
    //    }
    //}

    [CustomPropertyDrawer(typeof(PoolObjectInfo))]
    public class PoolObjectInfoDrawer : PropertyDrawer {
        int currentEnumIndex = -1;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            base.OnGUI(position, property, label);
            currentEnumIndex = property.FindPropertyRelative(nameof(PoolObjectInfo.loadType)).enumValueIndex;
            switch (currentEnumIndex) {
                case 0:
                    CatLog.Log("NONE.");
                    break;
                case 1: 
                    CatLog.Log("DEFAULT");
                    break;
                case 2: 
                    CatLog.Log("ADDRESSABLE");
                    break;
                default: 
                    break;
            }
        }
    }
#endif
}
