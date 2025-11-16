using UnityEngine;

namespace LAS.UI
{
    /// <summary>
    /// Ensures RuntimeUIBuilder is present on the GameCanvas at runtime.
    /// This is a failsafe that automatically adds the builder if it's missing.
    /// Attach this to any GameObject in the scene (or it will auto-create).
    /// </summary>
    [DefaultExecutionOrder(-100)] // Execute before other scripts
    public class UIAutoInitializer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            // This runs automatically when the game starts, before any scene loads
            Debug.Log("[UIAutoInitializer] Checking for UI setup...");
        }

        private void Awake()
        {
            // Find or create GameCanvas
            var canvas = GameObject.Find("GameCanvas");

            if (canvas == null)
            {
                Debug.LogWarning("[UIAutoInitializer] GameCanvas not found! UI may not work properly.");
                return;
            }

            // Ensure RuntimeUIBuilder exists
            var builder = canvas.GetComponent<RuntimeUIBuilder>();
            if (builder == null)
            {
                Debug.Log("[UIAutoInitializer] Adding RuntimeUIBuilder to GameCanvas");
                builder = canvas.AddComponent<RuntimeUIBuilder>();
            }
            else
            {
                Debug.Log("[UIAutoInitializer] RuntimeUIBuilder already present on GameCanvas");
            }

            // This script has done its job, destroy it
            Destroy(gameObject);
        }
    }
}
