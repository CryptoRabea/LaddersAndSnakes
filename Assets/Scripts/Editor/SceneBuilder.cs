#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using LAS.UI;
using LAS.Networking;
using LAS.Gameplay;

namespace LAS.Editor
{
    /// <summary>
    /// Automated scene builder for creating MainMenu and Game scenes
    /// </summary>
    public static class SceneBuilder
    {
        private const string SCENES_PATH = "Assets/Scenes/";
        private const string MAIN_MENU_SCENE = "MainMenu.unity";
        private const string GAME_SCENE = "GameScene.unity";

        [MenuItem("LAS/Build All Scenes", priority = 0)]
        public static void BuildAllScenes()
        {
            BuildMainMenuScene();
            BuildGameScene();
            ConfigureBuildSettings();
            Debug.Log("[SceneBuilder] ✓ All scenes built successfully!");
            EditorUtility.DisplayDialog("Scene Builder", "All scenes have been built successfully!\n\n" +
                "- MainMenu scene\n" +
                "- GameScene\n" +
                "- Build settings configured\n\n" +
                "You can now build and test the game!", "OK");
        }

        [MenuItem("LAS/Build MainMenu Scene", priority = 1)]
        public static void BuildMainMenuScene()
        {
            Debug.Log("[SceneBuilder] Building MainMenu scene...");

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create EventSystem for UI
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Create Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Create Main Panel
            GameObject mainPanel = CreatePanel(canvasGO.transform, "MainPanel");
            SetPanelColor(mainPanel, new Color(0.1f, 0.1f, 0.15f, 0.95f));

            // Create Title
            CreateTitle(mainPanel.transform, "SNAKES & LADDERS");

            // Create Main Menu Buttons
            GameObject playLocalBtn = CreateButton(mainPanel.transform, "PlayLocalButton", "Play Local Multiplayer", new Vector2(0, 150));
            GameObject playAIBtn = CreateButton(mainPanel.transform, "PlayAIButton", "Play vs AI", new Vector2(0, 50));
            GameObject playOnlineBtn = CreateButton(mainPanel.transform, "PlayOnlineButton", "Play Online", new Vector2(0, -50));
            GameObject settingsBtn = CreateButton(mainPanel.transform, "SettingsButton", "Settings", new Vector2(0, -150));
            GameObject quitBtn = CreateButton(mainPanel.transform, "QuitButton", "Quit", new Vector2(0, -250));

            // Create Multiplayer Panel
            GameObject multiplayerPanel = CreatePanel(canvasGO.transform, "MultiplayerPanel");
            SetPanelColor(multiplayerPanel, new Color(0.1f, 0.1f, 0.15f, 0.95f));
            multiplayerPanel.SetActive(false);

            CreateTitle(multiplayerPanel.transform, "MULTIPLAYER");

            GameObject hostBtn = CreateButton(multiplayerPanel.transform, "HostGameButton", "Host Game", new Vector2(0, 100));
            GameObject joinBtn = CreateButton(multiplayerPanel.transform, "JoinGameButton", "Join Game", new Vector2(0, 0));

            // Create Server Address Input
            GameObject serverInput = CreateInputField(multiplayerPanel.transform, "ServerAddressInput", "127.0.0.1", new Vector2(0, -100));

            // Player Count Dropdown
            GameObject playerDropdown = CreateDropdown(multiplayerPanel.transform, "PlayerCountDropdown", new Vector2(0, 50));

            GameObject backBtn = CreateButton(multiplayerPanel.transform, "BackButton", "Back", new Vector2(0, -200));

            // Create Settings Panel
            GameObject settingsPanel = CreatePanel(canvasGO.transform, "SettingsPanel");
            SetPanelColor(settingsPanel, new Color(0.1f, 0.1f, 0.15f, 0.95f));
            settingsPanel.SetActive(false);

            CreateTitle(settingsPanel.transform, "SETTINGS");
            GameObject backBtn2 = CreateButton(settingsPanel.transform, "BackButton", "Back", new Vector2(0, -200));

            // Add MainMenuController
            GameObject controllerGO = new GameObject("MenuController");
            MainMenuController controller = controllerGO.AddComponent<MainMenuController>();

            // Assign references using reflection (since fields are serialized)
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
            so.FindProperty("multiplayerPanel").objectReferenceValue = multiplayerPanel;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            so.FindProperty("playLocalButton").objectReferenceValue = playLocalBtn.GetComponent<Button>();
            so.FindProperty("playAIButton").objectReferenceValue = playAIBtn.GetComponent<Button>();
            so.FindProperty("playOnlineButton").objectReferenceValue = playOnlineBtn.GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            so.FindProperty("hostGameButton").objectReferenceValue = hostBtn.GetComponent<Button>();
            so.FindProperty("joinGameButton").objectReferenceValue = joinBtn.GetComponent<Button>();
            so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();
            so.FindProperty("serverAddressInput").objectReferenceValue = serverInput.GetComponent<TMP_InputField>();
            so.FindProperty("playerCountDropdown").objectReferenceValue = playerDropdown.GetComponent<TMP_Dropdown>();
            so.FindProperty("gameSceneName").stringValue = "GameScene";
            so.ApplyModifiedProperties();

            // Save scene
            if (!AssetDatabase.IsValidFolder(SCENES_PATH.TrimEnd('/')))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, SCENES_PATH + MAIN_MENU_SCENE);
            Debug.Log($"[SceneBuilder] ✓ MainMenu scene created at {SCENES_PATH + MAIN_MENU_SCENE}");
        }

        [MenuItem("LAS/Build Game Scene", priority = 2)]
        public static void BuildGameScene()
        {
            Debug.Log("[SceneBuilder] Building Game scene...");

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create Game Controller
            GameObject gameControllerGO = new GameObject("GameController");
            gameControllerGO.AddComponent<MultiplayerGameController>();

            // Create Network Manager (persistent)
            GameObject networkManagerGO = new GameObject("NetworkManager");
            networkManagerGO.AddComponent<NetworkManager>();

            // Create placeholder for board
            GameObject boardRoot = new GameObject("BoardRoot");
            boardRoot.transform.position = Vector3.zero;

            // Add a simple camera setup marker
            GameObject cameraRig = new GameObject("CameraRig");
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.parent = cameraRig.transform;
                mainCam.transform.localPosition = new Vector3(0, 10, -10);
                mainCam.transform.localEulerAngles = new Vector3(45, 0, 0);
            }

            // Save scene
            if (!AssetDatabase.IsValidFolder(SCENES_PATH.TrimEnd('/')))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, SCENES_PATH + GAME_SCENE);
            Debug.Log($"[SceneBuilder] ✓ Game scene created at {SCENES_PATH + GAME_SCENE}");
        }

        [MenuItem("LAS/Configure Build Settings", priority = 3)]
        public static void ConfigureBuildSettings()
        {
            Debug.Log("[SceneBuilder] Configuring build settings...");

            // Get current scenes in build
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            // Add MainMenu scene (index 0)
            string mainMenuPath = SCENES_PATH + MAIN_MENU_SCENE;
            if (System.IO.File.Exists(mainMenuPath))
            {
                scenes.Add(new EditorBuildSettingsScene(mainMenuPath, true));
            }

            // Add Game scene (index 1)
            string gamePath = SCENES_PATH + GAME_SCENE;
            if (System.IO.File.Exists(gamePath))
            {
                scenes.Add(new EditorBuildSettingsScene(gamePath, true));
            }

            // Update build settings
            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("[SceneBuilder] ✓ Build settings configured with " + scenes.Count + " scenes");
        }

        // Helper methods for UI creation
        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            return panel;
        }

        private static void SetPanelColor(GameObject panel, Color color)
        {
            Image image = panel.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        private static GameObject CreateTitle(Transform parent, string text)
        {
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(parent, false);

            RectTransform rt = titleGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -50);
            rt.sizeDelta = new Vector2(800, 100);

            TextMeshProUGUI tmp = titleGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 72;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;

            return titleGO;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            RectTransform rt = buttonGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(400, 80);

            Image image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.8f, 1f);

            Button button = buttonGO.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 1f);
            colors.pressedColor = new Color(0.15f, 0.3f, 0.6f, 1f);
            button.colors = colors;

            // Button Text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return buttonGO;
        }

        private static GameObject CreateInputField(Transform parent, string name, string placeholder, Vector2 position)
        {
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);

            RectTransform rt = inputGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(400, 60);

            Image image = inputGO.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();

            // Text Area
            GameObject textAreaGO = new GameObject("TextArea");
            textAreaGO.transform.SetParent(inputGO.transform, false);
            RectTransform textAreaRT = textAreaGO.AddComponent<RectTransform>();
            textAreaRT.anchorMin = Vector2.zero;
            textAreaRT.anchorMax = Vector2.one;
            textAreaRT.sizeDelta = new Vector2(-20, -20);

            // Placeholder
            GameObject placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(textAreaGO.transform, false);
            RectTransform phRT = placeholderGO.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI phText = placeholderGO.AddComponent<TextMeshProUGUI>();
            phText.text = placeholder;
            phText.fontSize = 24;
            phText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            phText.alignment = TextAlignmentOptions.Left;

            // Text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(textAreaGO.transform, false);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;

            inputField.textViewport = textAreaRT;
            inputField.textComponent = text;
            inputField.placeholder = phText;

            return inputGO;
        }

        private static GameObject CreateDropdown(Transform parent, string name, Vector2 position)
        {
            GameObject dropdownGO = new GameObject(name);
            dropdownGO.transform.SetParent(parent, false);

            RectTransform rt = dropdownGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(400, 60);

            Image image = dropdownGO.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            TMP_Dropdown dropdown = dropdownGO.AddComponent<TMP_Dropdown>();

            // Label
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(dropdownGO.transform, false);
            RectTransform labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.offsetMin = new Vector2(10, 0);
            labelRT.offsetMax = new Vector2(-30, 0);

            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = "2 Players";
            labelText.fontSize = 24;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Left;

            // Arrow
            GameObject arrowGO = new GameObject("Arrow");
            arrowGO.transform.SetParent(dropdownGO.transform, false);
            RectTransform arrowRT = arrowGO.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(20, 20);
            arrowRT.anchoredPosition = new Vector2(-15, 0);

            Image arrowImage = arrowGO.AddComponent<Image>();
            arrowImage.color = Color.white;

            // Template (simplified)
            GameObject templateGO = new GameObject("Template");
            templateGO.transform.SetParent(dropdownGO.transform, false);
            RectTransform templateRT = templateGO.AddComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta = new Vector2(0, 150);

            Image templateImage = templateGO.AddComponent<Image>();
            templateImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            GameObject itemGO = new GameObject("Item");
            itemGO.transform.SetParent(templateGO.transform, false);
            RectTransform itemRT = itemGO.AddComponent<RectTransform>();

            GameObject itemLabelGO = new GameObject("ItemLabel");
            itemLabelGO.transform.SetParent(itemGO.transform, false);
            TextMeshProUGUI itemLabel = itemLabelGO.AddComponent<TextMeshProUGUI>();
            itemLabel.alignment = TextAlignmentOptions.Left;

            dropdown.targetGraphic = image;
            dropdown.template = templateRT;
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabel;

            templateGO.SetActive(false);

            return dropdownGO;
        }
    }
}
#endif
