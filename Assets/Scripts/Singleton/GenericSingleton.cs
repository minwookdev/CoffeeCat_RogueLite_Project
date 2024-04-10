using UnityEngine;
using CoffeeCat.Utils;

namespace CoffeeCat.FrameWork {
    [DisallowMultipleComponent]
    public class GenericSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        // Destroy 여부 확인용
        private static bool _shuttingDown = false;
        private static object _lock = new object();
        private static T _instance;

        /// <summary>
        /// Check Singleton Instance Exist 
        /// </summary>
        public static bool IsExist {
            get {
                return _instance != null;
            }
        }

        public static T Instance {
            get {
                // 게임 종료 시 Object 보다 싱글톤의 OnDestroy 가 먼저 실행 될 수도 있다. 
                // 해당 싱글톤을 gameObject.Ondestory() 에서는 사용하지 않거나 사용한다면 null 체크를 해주자
                if (_shuttingDown) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T).Name + "' already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)    //Thread Safe
                {
                    if (_instance == null) {
                        // 인스턴스 존재 여부 확인
                        _instance = (T)FindObjectOfType(typeof(T));

                        // 아직 생성되지 않았다면 인스턴스 생성
                        if (_instance == null) {
                            // 새로운 게임오브젝트를 만들어서 싱글톤 Attach
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).Name + " (Singleton)";

                            // Make instance persistent.
                            DontDestroyOnLoad(singletonObject);

                            // Singleton Initialize Call. 
                            _instance.SendMessage(nameof(GenericSingleton<T>.Initialize)); // Type 1. SendMessage
                            //_instance.GetComponent<GenericSingleton<T>>().Initialize();  // Type 2. GetComponent
                            CatLog.Log($"Initialized Singleton {typeof(T).Name}");
                        }
                    }
                    return _instance;
                }
            }
        }

        // 비 싱글턴 생성자 사용 방지
        protected GenericSingleton() { }

        /// <summary>
        /// 대형 로직 작성 금지
        /// </summary>
        protected virtual void Initialize() { }

        //private void Awake() => Initialize();

        private void OnApplicationQuit() => _shuttingDown = true;

        private void OnDestroy() => _shuttingDown = true;

        public virtual void ReleaseSingleton() {
            _instance = null;
            Destroy(this);
        }
    }
}
