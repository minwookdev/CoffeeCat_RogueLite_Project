// ReSharper disable InvalidXmlDocComment
/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION: 
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CoffeeCat.Simplify {
	[DisallowMultipleComponent]
	public class PoolManagerLight : MonoBehaviour {
		private static PoolManagerLight instance = null;
		public static PoolManagerLight Instance {
			get => instance;
		}
		
		[SerializeField]
		private List<PoolInformation> poolInfos = null;
		private Dictionary<string, Stack<GameObject>> poolDictionary = null;

		private void Awake() {
			if (instance == null) {
				var existedPoolManager = FindObjectOfType<PoolManagerLight>();
				instance = (existedPoolManager != null) ? existedPoolManager : this;
			}
			else {
				Destroy(this);
			}
		}

		private void Start() {
			Initialize();
		}
		
		private void Initialize() {
			poolDictionary = new Dictionary<string, Stack<GameObject>>();
			if (poolInfos == null || poolInfos.Count <= 0) {
				return;
			}

			for (int i = 0; i < poolInfos.Count; i++) {
				Stack<GameObject> poolStack = new Stack<GameObject>();
				if (poolDictionary.ContainsKey(poolInfos[i].PoolObject.name)) {
					Debug.LogWarning($"중복된 키: {poolInfos[i].PoolObject.name}");
					continue;
				}
				
				poolDictionary.Add(poolInfos[i].PoolObject.name, poolStack);
				
				// Create Root Parent If Has Root Parent Options
				Transform rootParentTr = null;
				if (poolInfos[i].HasRootParent) {
					rootParentTr = new GameObject(poolInfos[i].PoolObject.name + "_Root").GetComponent<Transform>();
					rootParentTr.position = Vector3.zero;
					rootParentTr.rotation = Quaternion.identity;
				}

				// Spawn Pool Objects
				for (int j = 0; j < poolInfos[i].InitialSpawnCount; j++) {
					var clone = Instantiate(poolInfos[i].PoolObject, Vector3.zero, Quaternion.identity, rootParentTr);
					clone.SetActive(false);
					poolStack.Push(clone);
				}
			}
		}

		public void AddToPool(GameObject target, int initSpawnCount, bool hasRootParent = true) {
			var poolInfo = new PoolInformation() {
				PoolObject = target,
				InitialSpawnCount = initSpawnCount,
				HasRootParent = hasRootParent
			};

			Stack<GameObject> poolStack = new Stack<GameObject>();
			poolDictionary.Add(poolInfo.PoolObject.name, poolStack);

			Transform rootParentTr = null;
			if (poolInfo.HasRootParent) {
				rootParentTr = new GameObject(poolInfo.PoolObject.name + "_Root").GetComponent<Transform>();
				rootParentTr.position = Vector3.zero;
				rootParentTr.rotation = Quaternion.identity;
			}
			
			for (int i = 0; i < poolInfo.InitialSpawnCount; i++) {
				var clone = Instantiate(poolInfo.PoolObject, Vector3.zero, Quaternion.identity, rootParentTr);
				clone.SetActive(false);
				poolStack.Push(clone);
			}
		}

		public GameObject SpawnToPool(string key, Vector3 position, Quaternion rotation) {
			if (!poolDictionary.ContainsKey(key)) {
				Debug.LogError($"Pool Dictionary에서 키를 찾을 수 없음. key: {key}");
				return null;
			}

			GameObject result = null;
			if (poolDictionary[key].Count <= 0) {
				GameObject originPrefab = poolInfos.Select(info => info.PoolObject)
				                                   .FirstOrDefault(prefab => prefab.name == key);
				if (!originPrefab) {
					return null;
				}
				
				GameObject parentObject = GameObject.Find(originPrefab.name + "_Root");
				Transform parentTr = null;
				if (parentObject != null) {
					parentTr = parentObject.GetComponent<Transform>();
				}

				var clone = Instantiate(originPrefab, Vector3.zero, Quaternion.identity, parentTr);
				clone.SetActive(false);
				poolDictionary[key].Push(clone);
			}

			result = poolDictionary[key].Pop();
			result.transform.position = position;
			result.transform.rotation = rotation;
			result.SetActive(true);
			return result;
		}

		public void Despawn(GameObject despawnTarget, float delaySeconds = 0f) {
			if (!poolDictionary.ContainsKey(despawnTarget.name)) {
				Debug.LogError($"Pool Dictionary에서 키를 찾을 수 없음. key: {despawnTarget.name}");
				return;
			}

			if (delaySeconds > 0f) {
				StartCoroutine(DespawnDelay(delaySeconds));
				return;
			}
			
			Execute();

			IEnumerator DespawnDelay(float seconds) {
				yield return new WaitForSeconds(seconds);
				Execute();
			}

			void Execute() {
				despawnTarget.SetActive(false);
				poolDictionary[despawnTarget.name].Push(despawnTarget);
			}
		}

		[Serializable]
		public class PoolInformation {
			public GameObject PoolObject;
			public int InitialSpawnCount;
			public bool HasRootParent = true;
		}
	}
}
