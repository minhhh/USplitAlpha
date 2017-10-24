using UnityEngine;
using UnityEngine.Assertions;

namespace USingleton
{
    /// <summary>
    /// Game singleton will be initialized once in the game. It persists when scenes are loaded and reloaded
    /// It is recommended to replaced MonoBehaviour with your BaseBehaviour class
    /// </summary>
    public class GameSingleton<T> : MonoBehaviour where T : GameSingleton<T>
    {
        private static T instance;

        public static T Instance {
            get {
                Assert.IsNotNull (instance, "Instance is null. Please call CreateInstance first!");
                return instance;
            }
        }

        public static T CreateInstance ()
        {
            if (instance != null) {
                Assert.IsNull (instance, "Instance is not null. Please call CreateInstance once only");
                return instance;
            }

            GameObject go = new GameObject (typeof(T).Name);
            instance = go.AddComponent<T> ();

            instance.OnCreated ();

            DontDestroyOnLoad (go);

            return instance;
        }

        protected virtual void OnDestroy ()
        {
            instance = null;
        }

        protected virtual void OnCreated ()
        {
        }

    }

}
