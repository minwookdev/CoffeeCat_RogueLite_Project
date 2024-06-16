using UnityEngine;
using CoffeeCat.Utils;

namespace CoffeeCat.FrameWork
{
    public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static object _lock = new object();
        private static T _instance = null;

        public static bool IsExistInstance => _instance != null;

        public static T Instance {
            get {

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        CatLog.WLog($"Prelocated Singleton: {nameof(T)} Is Not Exist !");
                        return null;
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Initialize() { }

        private void Awake() {
            //_instance = (T)FindObjectOfType(typeof(T));
            _instance = this as T;
            Initialize();
        }

        private void OnDestroy() => _instance = null;

        private void OnApplicationQuit() => _instance = null;
    }
}
