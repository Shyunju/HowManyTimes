using UnityEngine;


namespace UGESystem
{
    /// <summary>
    /// MonoBehaviour 구성 요소를 위한 제네릭 싱글톤 기본 클래스입니다.
    /// Generic Singleton base class for MonoBehaviour components.
    /// 씬에 싱글톤의 인스턴스가 하나만 존재하도록 보장합니다.
    /// Ensures that only one instance of the singleton exists in the scene.
    /// </summary>
    /// <typeparam name="T">싱글톤의 타입입니다.</typeparam>
    /// <typeparam name="T">The type of the singleton.</typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // 싱글톤의 정적 인스턴스입니다.
        // The static instance of the singleton.
        private static T _instance;

        // 스레드로부터 안전한 접근을 위한 잠금 객체입니다.
        // Lock object for thread-safe access.
        private static readonly object _lock = new object();

        // 싱글톤이 씬 로드 시에도 유지될지 여부를 제어합니다.
        // Controls whether the singleton persists across scene loads.
        // Unity 인스펙터에서 설정할 수 있습니다.
        // Can be configured in the Unity Inspector.
        [SerializeField]
        protected bool _isDontDestroy = false;

        /// <summary>
        /// 싱글톤 인스턴스를 가져옵니다. 존재하지 않으면 생성됩니다.
        /// Gets the singleton instance. If it doesn't exist, it will be created.
        /// </summary>
        public static T Instance
        {
            get
            {
                // 스레드 안전성을 위해 잠금을 사용합니다.
                lock (_lock)
                {
                    // 인스턴스가 이미 설정되어 있으면 반환합니다.
                    if (_instance != null)
                    {
                        return _instance;
                    }

                    // 씬에서 기존 인스턴스를 찾습니다.
                    _instance = FindFirstObjectByType<T>();

                    // 인스턴스를 찾지 못하면 새 GameObject를 만들고 구성 요소를 추가합니다.
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// 스크립트 인스턴스가 로드될 때 호출됩니다.
        /// Called when the script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            // 인스턴스가 이미 존재하는지 확인합니다.
            // Check if an instance already exists.
            if (_instance != null && _instance != this)
            {
                // 다른 인스턴스가 이미 존재하면 이 인스턴스를 파괴합니다.
                // If another instance already exists, destroy this instance.
#if UNITY_EDITOR
                Debug.LogWarning($"[Singleton] An instance of {typeof(T)} already exists. Destroying this duplicate instance.");
#endif
                Destroy(gameObject);
                return;
            }

            // 이 인스턴스를 싱글톤 인스턴스로 설정합니다.
            // Set this instance as the singleton instance.
            _instance = this as T;

            // 설정된 경우, 씬 간에 싱글톤을 유지합니다.
            // If configured, keep the singleton across scenes.
            if (_isDontDestroy)
            {
                DontDestroyOnLoad(gameObject);
            }
        
            OnAwake();
        }

        /// <summary>
        /// 하위 클래스에서 재정의할 수 있는 사용자 지정 초기화 메서드입니다.
        /// Awake()의 끝에서 호출됩니다.
        /// A customizable initialization method that can be overridden in subclasses.
        /// Called at the end of Awake().
        /// </summary>
        protected virtual void OnAwake() { }

        /// <summary>
        /// MonoBehaviour가 파괴될 때 호출됩니다.
        /// Called when the MonoBehaviour is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}