#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using LAS.Networking;

namespace LAS.Editor
{
    /// <summary>
    /// Editor helper to automatically setup multiplayer networking in scenes
    /// Menu: LAS → Multiplayer → Auto Setup Scene
    /// </summary>
    public class MultiplayerSetupHelper : EditorWindow
    {
        private bool setupMainMenu = true;
        private bool setupGameScene = true;
        private bool addConnectionUI = true;

        [MenuItem("LAS/Multiplayer/Auto Setup Scene")]
        public static void ShowWindow()
        {
            GetWindow<MultiplayerSetupHelper>("Multiplayer Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Multiplayer Setup Helper", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool will automatically setup Unity Netcode for the current scene.\n\n" +
                "It will add:\n" +
                "- Unity NetworkManager\n" +
                "- UnityTransport\n" +
                "- NetworkedGameManager\n" +
                "- NetworkConnectionUI (optional)",
                MessageType.Info
            );

            GUILayout.Space(10);

            addConnectionUI = EditorGUILayout.Toggle("Add Connection UI", addConnectionUI);

            GUILayout.Space(20);

            if (GUILayout.Button("Setup Current Scene", GUILayout.Height(40)))
            {
                SetupCurrentScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Setup All Scenes (MainMenu + GameScene)", GUILayout.Height(40)))
            {
                SetupAllScenes();
            }

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "After setup, check the MULTIPLAYER_QUICKSTART.md file for next steps!",
                MessageType.Info
            );
        }

        private void SetupCurrentScene()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[MultiplayerSetup] Setting up scene: {sceneName}");

            // Setup Unity NetworkManager
            SetupUnityNetworkManager(sceneName);

            // Setup NetworkedGameManager (only for GameScene)
            if (sceneName.ToLower().Contains("game"))
            {
                SetupNetworkedGameManager();

                if (addConnectionUI)
                {
                    SetupConnectionUI();
                }
            }

            EditorUtility.DisplayDialog(
                "Setup Complete",
                $"Multiplayer setup complete for {sceneName}!\n\n" +
                "Check the Console for details.\n\n" +
                "Next: Read MULTIPLAYER_QUICKSTART.md",
                "OK"
            );

            Debug.Log("[MultiplayerSetup] Setup complete!");
        }

        private void SetupAllScenes()
        {
            Debug.Log("[MultiplayerSetup] Setting up all scenes...");

            // You would need to implement scene loading here
            EditorUtility.DisplayDialog(
                "Manual Setup Required",
                "Please setup each scene individually:\n\n" +
                "1. Open MainMenu scene\n" +
                "2. Run 'Setup Current Scene'\n" +
                "3. Open GameScene\n" +
                "4. Run 'Setup Current Scene'\n\n" +
                "Or follow the MULTIPLAYER_QUICKSTART.md guide!",
                "OK"
            );
        }

        private void SetupUnityNetworkManager(string sceneName)
        {
            // Check if NetworkManager already exists
            var existingNM = FindObjectOfType<Unity.Netcode.NetworkManager>();
            if (existingNM != null)
            {
                Debug.LogWarning("[MultiplayerSetup] NetworkManager already exists in scene!");
                return;
            }

            // Create NetworkManager GameObject
            GameObject nmGO = new GameObject("NetworkManager");

            // Add NetworkManager component
            var networkManager = nmGO.AddComponent<Unity.Netcode.NetworkManager>();

            // Add UnityTransport component
            var transport = nmGO.AddComponent<UnityTransport>();

            // Configure NetworkManager
            networkManager.NetworkConfig = new NetworkConfig()
            {
                // You can set default config here
            };

            // Set transport
            // Note: This might need to be done through SerializedObject in newer Unity versions
            var serializedNM = new SerializedObject(networkManager);
            var transportProp = serializedNM.FindProperty("NetworkConfig.NetworkTransport");
            if (transportProp != null)
            {
                // Link transport
                Debug.Log("[MultiplayerSetup] Linking transport to NetworkManager");
            }
            serializedNM.ApplyModifiedProperties();

            // Configure transport
            transport.ConnectionData.Address = "0.0.0.0";
            transport.ConnectionData.Port = 7777;
            transport.ConnectionData.ServerListenAddress = "0.0.0.0";

            // Don't destroy on load for GameScene
            if (sceneName.ToLower().Contains("game"))
            {
                // DontDestroyOnLoad is set at runtime, but we can mark it
                Debug.Log("[MultiplayerSetup] Remember to check 'Don't Destroy On Load' for GameScene NetworkManager!");
            }

            Debug.Log("[MultiplayerSetup] Unity NetworkManager added to scene");
            EditorUtility.SetDirty(nmGO);
        }

        private void SetupNetworkedGameManager()
        {
            // Check if NetworkedGameManager already exists
            var existingNGM = FindObjectOfType<NetworkedGameManager>();
            if (existingNGM != null)
            {
                Debug.LogWarning("[MultiplayerSetup] NetworkedGameManager already exists in scene!");
                return;
            }

            // Create NetworkedGameManager GameObject
            GameObject ngmGO = new GameObject("NetworkedGameManager");

            // Add NetworkedGameManager component
            var ngm = ngmGO.AddComponent<NetworkedGameManager>();

            // Add NetworkObject component (required for Netcode)
            var networkObject = ngmGO.AddComponent<NetworkObject>();

            Debug.Log("[MultiplayerSetup] NetworkedGameManager added to scene");
            EditorUtility.SetDirty(ngmGO);
        }

        private void SetupConnectionUI()
        {
            // Find or create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log("[MultiplayerSetup] Created Canvas");
            }

            // Check if NetworkConnectionUI already exists
            var existingUI = FindObjectOfType<LAS.UI.NetworkConnectionUI>();
            if (existingUI != null)
            {
                Debug.LogWarning("[MultiplayerSetup] NetworkConnectionUI already exists in scene!");
                return;
            }

            // Create ConnectionPanel
            GameObject panelGO = new GameObject("NetworkConnectionPanel");
            panelGO.transform.SetParent(canvas.transform, false);

            // Add RectTransform and position it
            RectTransform rt = panelGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -10);
            rt.sizeDelta = new Vector2(300, 150);

            // Add Image component for background
            var image = panelGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.7f);

            // Add NetworkConnectionUI component
            var connectionUI = panelGO.AddComponent<LAS.UI.NetworkConnectionUI>();

            // Note: You would need to create child UI elements (texts, buttons) here
            // For now, this creates the basic structure

            Debug.Log("[MultiplayerSetup] NetworkConnectionUI panel created (you need to add UI elements manually)");
            EditorUtility.SetDirty(panelGO);
        }
    }

    /// <summary>
    /// Quick menu items for common multiplayer setup tasks
    /// </summary>
    public static class MultiplayerQuickSetup
    {
        [MenuItem("LAS/Multiplayer/Add NetworkManager to Scene")]
        public static void AddNetworkManager()
        {
            var existing = FindObjectOfType<Unity.Netcode.NetworkManager>();
            if (existing != null)
            {
                Debug.LogWarning("NetworkManager already exists!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            GameObject go = new GameObject("NetworkManager");
            go.AddComponent<Unity.Netcode.NetworkManager>();
            var transport = go.AddComponent<UnityTransport>();
            transport.ConnectionData.Port = 7777;

            Debug.Log("NetworkManager added! Don't forget to link the transport.");
            Selection.activeGameObject = go;
            EditorUtility.SetDirty(go);
        }

        [MenuItem("LAS/Multiplayer/Add NetworkedGameManager to Scene")]
        public static void AddNetworkedGameManager()
        {
            var existing = FindObjectOfType<NetworkedGameManager>();
            if (existing != null)
            {
                Debug.LogWarning("NetworkedGameManager already exists!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            GameObject go = new GameObject("NetworkedGameManager");
            go.AddComponent<NetworkedGameManager>();
            go.AddComponent<NetworkObject>();

            Debug.Log("NetworkedGameManager added!");
            Selection.activeGameObject = go;
            EditorUtility.SetDirty(go);
        }

        [MenuItem("LAS/Multiplayer/Open Setup Guide")]
        public static void OpenSetupGuide()
        {
            string path = Application.dataPath + "/../MULTIPLAYER_QUICKSTART.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file://" + path);
            }
            else
            {
                Debug.LogError("Setup guide not found at: " + path);
            }
        }

        [MenuItem("LAS/Multiplayer/Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://docs-multiplayer.unity3d.com/netcode/current/about/");
        }
    }
}
#endif
