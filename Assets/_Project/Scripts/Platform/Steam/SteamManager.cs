using UnityEngine;

namespace Robot.Platform.Steam
{
    /// <summary>
    /// Handles Steam API initialization, callbacks, and platform lifecycle.
    /// </summary>
    public class SteamManager : MonoBehaviour
    {
        private static SteamManager _instance;
        public static SteamManager Instance => _instance;

        [Header("Steam Configuration")]
        [SerializeField] private uint appId = 480; // Default Spacewar AppID for testing
        [SerializeField] private bool initializeOnAwake = true;

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (initializeOnAwake)
            {
                InitializeSteam();
            }
        }

        public bool InitializeSteam()
        {
#if UNITY_STANDALONE || STEAMWORKS_NET
            Debug.Log($"[SteamManager] Initializing Steam API (AppID: {appId})...");
            // Note: Integrate Steamworks.NET or Facepunch.Steamworks SDK when importing package.
            IsInitialized = true;
            return true;
#else
            Debug.Log("[SteamManager] Steam API skipped (Non-Standalone platform).");
            IsInitialized = false;
            return false;
#endif
        }

        private void Update()
        {
            if (!IsInitialized) return;
            // SteamAPI.RunCallbacks();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
