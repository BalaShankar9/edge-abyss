using UnityEngine;

namespace EdgeAbyss.Utils
{
    /// <summary>
    /// Thread-safe generic singleton base class for MonoBehaviours.
    /// Inherit from this class to create a singleton component.
    /// 
    /// Usage:
    ///   public class MyManager : Singleton&lt;MyManager&gt; { }
    /// 
    /// The singleton instance persists across scene loads.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_instance;
        private static readonly object s_lock = new object();
        private static bool s_applicationIsQuitting;

        /// <summary>
        /// Gets the singleton instance. Creates one if it doesn't exist.
        /// Returns null if the application is quitting to prevent ghost objects.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (s_applicationIsQuitting)
                {
                    return null;
                }

                lock (s_lock)
                {
                    if (s_instance == null)
                    {
                        s_instance = FindFirstObjectByType<T>();

                        if (s_instance == null)
                        {
                            var singletonObject = new GameObject();
                            s_instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"[Singleton] {typeof(T).Name}";
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return s_instance;
                }
            }
        }

        /// <summary>
        /// Returns true if an instance exists without creating one.
        /// </summary>
        public static bool HasInstance => s_instance != null;

        protected virtual void Awake()
        {
            if (s_instance == null)
            {
                s_instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (s_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called once when the singleton instance is first initialized.
        /// Override this instead of Awake() in derived classes.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnApplicationQuit()
        {
            s_applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }
    }
}
