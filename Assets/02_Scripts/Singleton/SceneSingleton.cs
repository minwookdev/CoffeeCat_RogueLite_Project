using UnityEngine;
using CoffeeCat.Utils;

namespace CoffeeCat.FrameWork
{
    public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static object _lock = new object();
        private static T inst = null;

        public static bool IsExist => inst != null;

        public static T Inst {
            get {

                lock (_lock)
                {
                    if (inst) {
                        return inst;
                    }
                    CatLog.WLog($"Prelocated Singleton: {nameof(T)} Is Not Exist !");
                    return null;
                }
            }
        }

        protected virtual void Initialize() { }

        private void Awake() {
            //_instance = (T)FindObjectOfType(typeof(T));
            inst = this as T;
            Initialize();
        }

        private void OnDestroy() => inst = null;

        private void OnApplicationQuit() => inst = null;
    }
}
