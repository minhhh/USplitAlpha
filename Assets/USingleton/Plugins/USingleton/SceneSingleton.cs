using UnityEngine;
using UnityEngine.Assertions;

namespace USingleton
{
    /// <summary>
    /// Scene singleton will be initialized once per scene. It does not persist when scenes are loaded and reloaded
    /// It is recommended to replaced MonoBehaviour with your BaseBehaviour class
    /// </summary>
    public class SceneSingleton<T> : MonoBehaviour where T : SceneSingleton<T>
    {
        private static T instance;

        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<T> ();

                    Assert.IsNotNull (instance, "Instance should not be null. You must call this in the correct scene setup");
                }

                return instance;
            }
        }

        protected virtual void OnDestroy ()
        {
            instance = null;
        }
    }

}
