using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor script to automatically set up the complete Snakes and Ladders game
/// </summary>
public class SimpleGameSetup : EditorWindow
{
    [MenuItem("Snakes & Ladders/Setup Complete Game")]
    public static void SetupCompleteGame()
    {
        if (EditorUtility.DisplayDialog("Setup Snakes & Ladders",
            "This will create:\n- MainMenu scene\n- GameScene scene\n- All required prefabs\n\nContinue?",
            "Yes", "Cancel"))
        {
            CreatePrefabs();
            CreateMainMenuScene();
            CreateGameScene();
            AddScenesToBuildSettings();
            EditorUtility.DisplayDialog("Setup Complete",
                "Game setup complete!\n\nOpen MainMenu scene and press Play to start.",
                "OK");
        }
    }

    static void AddScenesToBuildSettings()
    {
        // Get existing scenes
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // Add MainMenu scene
        string mainMenuPath = "Assets/Scenes/MainMenu.unity";
        if (!scenes.Exists(s => s.path == mainMenuPath))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(mainMenuPath, true));
        }

        // Add GameScene
        string gameScenePath = "Assets/Scenes/GameScene.unity";
        if (!scenes.Exists(s => s.path == gameScenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(gameScenePath, true));
        }

        // Update build settings
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Added scenes to build settings");
    }

    static void CreatePrefabs()
    {
        string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Create Square Prefab
        CreateSquarePrefab(prefabFolder);

        // Create Player Prefab
        CreatePlayerPrefab(prefabFolder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void CreateSquarePrefab(string folder)
    {
        string path = folder + "/Square.prefab";

        // Check if prefab already exists
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("Square prefab already exists");
            return;
        }

        // Create square
        GameObject square = GameObject.CreatePrimitive(PrimitiveType.Cube);
        square.name = "Square";
        square.transform.localScale = new Vector3(1f, 0.1f, 1f);

        // Set material color to white
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.white;
        square.GetComponent<Renderer>().material = mat;

        // Add text for number
        GameObject textObj = new GameObject("NumberText");
        textObj.transform.SetParent(square.transform);
        textObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = "1";
        tmp.fontSize = 2;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(square, path);
        DestroyImmediate(square);

        Debug.Log("Created Square prefab");
    }

    static void CreatePlayerPrefab(string folder)
    {
        string path = folder + "/Player.prefab";

        // Check if prefab already exists
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("Player prefab already exists");
            return;
        }

        // Create player (cylinder)
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        player.name = "Player";
        player.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);

        // Set material
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        player.GetComponent<Renderer>().material = mat;

        // Remove collider
        DestroyImmediate(player.GetComponent<Collider>());

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(player, path);
        DestroyImmediate(player);

        Debug.Log("Created Player prefab");
    }

    static void CreateMainMenuScene()
    {
        // Create new scene
        Scene mainMenuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Create Main Panel
        GameObject panel = new GameObject("MainPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Create Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SNAKES & LADDERS";
        titleText.fontSize = 80;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.yellow;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(800, 150);

        // Create buttons
        CreateMenuButton(panel.transform, "Play2PlayerButton", "2 Players", new Vector2(0.5f, 0.5f), 0);
        CreateMenuButton(panel.transform, "Play3PlayerButton", "3 Players", new Vector2(0.5f, 0.4f), 1);
        CreateMenuButton(panel.transform, "Play4PlayerButton", "4 Players", new Vector2(0.5f, 0.3f), 2);
        CreateMenuButton(panel.transform, "QuitButton", "Quit", new Vector2(0.5f, 0.15f), 3);

        // Create GameManager
        GameObject managerObj = new GameObject("MainMenuManager");
        SimpleMainMenu menuScript = managerObj.AddComponent<SimpleMainMenu>();

        // Wire up references
        menuScript.GetType().GetField("play2PlayerButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(menuScript, GameObject.Find("Play2PlayerButton").GetComponent<Button>());
        menuScript.GetType().GetField("play3PlayerButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(menuScript, GameObject.Find("Play3PlayerButton").GetComponent<Button>());
        menuScript.GetType().GetField("play4PlayerButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(menuScript, GameObject.Find("Play4PlayerButton").GetComponent<Button>());
        menuScript.GetType().GetField("quitButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(menuScript, GameObject.Find("QuitButton").GetComponent<Button>());
        menuScript.GetType().GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(menuScript, titleText);

        // Save scene
        EditorSceneManager.SaveScene(mainMenuScene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("Created MainMenu scene");
    }

    static GameObject CreateMenuButton(Transform parent, string name, string text, Vector2 anchorPos, int colorIndex)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorPos;
        rect.anchorMax = anchorPos;
        rect.sizeDelta = new Vector2(400, 80);

        Image img = buttonObj.AddComponent<Image>();
        Color[] colors = { new Color(0.2f, 0.7f, 0.2f), new Color(0.2f, 0.5f, 0.8f), new Color(0.8f, 0.5f, 0.2f), new Color(0.8f, 0.2f, 0.2f) };
        img.color = colors[colorIndex % colors.Length];

        Button btn = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 36;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return buttonObj;
    }

    static void CreateGameScene()
    {
        // Create new scene
        Scene gameScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Configure camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(4.5f, 15f, 4.5f);
            mainCam.transform.rotation = Quaternion.Euler(60, 0, 0);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.3f, 0.5f, 0.8f);
        }

        // Create Directional Light
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Create UI elements
        GameObject turnText = CreateUIText(canvasObj.transform, "TurnText", "Player 1's Turn", new Vector2(0.5f, 0.95f), 48);
        GameObject diceText = CreateUIText(canvasObj.transform, "DiceText", "Dice: -", new Vector2(0.5f, 0.88f), 36);
        GameObject messageText = CreateUIText(canvasObj.transform, "MessageText", "Click Roll Dice to start!", new Vector2(0.5f, 0.1f), 32);

        // Create Roll Dice Button
        GameObject rollButton = CreateGameButton(canvasObj.transform, "RollDiceButton", "Roll Dice", new Vector2(0.5f, 0.05f));

        // Create Win Panel
        GameObject winPanel = CreateWinPanel(canvasObj.transform);

        // Create GameManager
        GameObject managerObj = new GameObject("GameManager");
        SimpleGameManager gameScript = managerObj.AddComponent<SimpleGameManager>();

        // Load prefabs
        GameObject squarePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Square.prefab");
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

        // Wire up references using reflection
        var fields = gameScript.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (field.Name == "squarePrefab") field.SetValue(gameScript, squarePrefab);
            else if (field.Name == "playerPrefab") field.SetValue(gameScript, playerPrefab);
            else if (field.Name == "rollDiceButton") field.SetValue(gameScript, rollButton.GetComponent<Button>());
            else if (field.Name == "turnText") field.SetValue(gameScript, turnText.GetComponent<TextMeshProUGUI>());
            else if (field.Name == "diceText") field.SetValue(gameScript, diceText.GetComponent<TextMeshProUGUI>());
            else if (field.Name == "messageText") field.SetValue(gameScript, messageText.GetComponent<TextMeshProUGUI>());
            else if (field.Name == "winPanel") field.SetValue(gameScript, winPanel);
            else if (field.Name == "winnerText") field.SetValue(gameScript, winPanel.transform.Find("WinnerText").GetComponent<TextMeshProUGUI>());
            else if (field.Name == "playAgainButton") field.SetValue(gameScript, winPanel.transform.Find("PlayAgainButton").GetComponent<Button>());
            else if (field.Name == "mainMenuButton") field.SetValue(gameScript, winPanel.transform.Find("MainMenuButton").GetComponent<Button>());
        }

        // Save scene
        EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/GameScene.unity");
        Debug.Log("Created GameScene scene");
    }

    static GameObject CreateUIText(Transform parent, string name, string text, Vector2 anchorPos, float fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Add outline
        tmp.fontStyle = FontStyles.Bold;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorPos;
        rect.anchorMax = anchorPos;
        rect.sizeDelta = new Vector2(800, 100);

        return textObj;
    }

    static GameObject CreateGameButton(Transform parent, string name, string text, Vector2 anchorPos)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorPos;
        rect.anchorMax = anchorPos;
        rect.sizeDelta = new Vector2(300, 70);

        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.8f, 0.2f);

        Button btn = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 32;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return buttonObj;
    }

    static GameObject CreateWinPanel(Transform parent)
    {
        GameObject panel = new GameObject("WinPanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.8f);

        // Winner Text
        GameObject winnerText = CreateUIText(panel.transform, "WinnerText", "Player 1 Wins!", new Vector2(0.5f, 0.6f), 72);
        winnerText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        // Play Again Button
        GameObject playAgainBtn = CreateGameButton(panel.transform, "PlayAgainButton", "Play Again", new Vector2(0.5f, 0.4f));
        playAgainBtn.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f);

        // Main Menu Button
        GameObject mainMenuBtn = CreateGameButton(panel.transform, "MainMenuButton", "Main Menu", new Vector2(0.5f, 0.3f));
        mainMenuBtn.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.2f);

        panel.SetActive(false);

        return panel;
    }
}
