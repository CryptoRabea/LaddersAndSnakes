using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Automated scene builder for Snakes and Ladders game.
/// Creates a complete working scene with all required GameObjects, components, and references.
/// </summary>
public class SceneBuilder : EditorWindow
{
    private const string RESOURCES_PATH = "Assets/Resources";
    private const string PREFABS_PATH = "Assets/Prefabs";
    private const string SCENES_PATH = "Assets/Scenes";
    private const string CONFIG_PATH = "Assets/ScriptableObjects";

    [MenuItem("Tools/Snakes and Ladders/Build Complete Game Scene")]
    public static void BuildCompleteScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Build Complete Game Scene",
            "This will create a fully working GameScene with:\n\n" +
            "• Game Manager with all components\n" +
            "• Board with 10x10 grid\n" +
            "• 4 Player pieces (prefabs)\n" +
            "• Dice system\n" +
            "• Complete UI (HUD + Game Over)\n" +
            "• ScriptableObject configs\n" +
            "• Camera and lighting\n\n" +
            "Continue?",
            "Build Scene",
            "Cancel"))
        {
            return;
        }

        try
        {
            CreateDirectories();
            CreateScriptableObjects();
            GameObject[] prefabs = CreatePrefabs();
            CreateGameScene(prefabs);

            EditorUtility.DisplayDialog(
                "Success!",
                "GameScene created successfully!\n\n" +
                "Scene Location: Assets/Scenes/GameScene.unity\n\n" +
                "Next Steps:\n" +
                "1. Open the GameScene\n" +
                "2. Select the Board object\n" +
                "3. In BoardGenerator component, click 'Generate Board'\n" +
                "4. Save the scene\n" +
                "5. Press Play to test!",
                "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to build scene: {e.Message}", "OK");
            Debug.LogError($"Scene build failed: {e}");
        }
    }

    private static void CreateDirectories()
    {
        string[] directories = { RESOURCES_PATH, PREFABS_PATH, SCENES_PATH, CONFIG_PATH };
        foreach (string dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.Log($"Created directory: {dir}");
            }
        }
        AssetDatabase.Refresh();
    }

    private static void CreateScriptableObjects()
    {
        // Create GameConfig
        string gameConfigPath = $"{CONFIG_PATH}/GameConfig.asset";
        if (!File.Exists(gameConfigPath))
        {
            GameConfig gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.boardSize = 100;
            gameConfig.moveSpeed = 4f;
            gameConfig.movementAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            AssetDatabase.CreateAsset(gameConfig, gameConfigPath);
            Debug.Log($"Created GameConfig at {gameConfigPath}");
        }

        // Create DiceConfig
        string diceConfigPath = $"{CONFIG_PATH}/DiceConfig.asset";
        if (!File.Exists(diceConfigPath))
        {
            DiceConfig diceConfig = ScriptableObject.CreateInstance<DiceConfig>();
            diceConfig.diceSides = 6;
            diceConfig.rollDuration = 1.2f;
            diceConfig.spinTorque = 10f;
            AssetDatabase.CreateAsset(diceConfig, diceConfigPath);
            Debug.Log($"Created DiceConfig at {diceConfigPath}");
        }

        // Create BoardConfig
        string boardConfigPath = $"{CONFIG_PATH}/BoardConfig.asset";
        if (!File.Exists(boardConfigPath))
        {
            BoardConfig boardConfig = ScriptableObject.CreateInstance<BoardConfig>();
            AssetDatabase.CreateAsset(boardConfig, boardConfigPath);
            Debug.Log($"Created BoardConfig at {boardConfigPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static GameObject[] CreatePrefabs()
    {
        GameObject[] playerPieces = new GameObject[4];
        Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
        string[] names = { "Player1_Red", "Player2_Blue", "Player3_Green", "Player4_Yellow" };

        for (int i = 0; i < 4; i++)
        {
            string prefabPath = $"{PREFABS_PATH}/{names[i]}.prefab";

            // Create or load existing prefab
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piece.name = names[i];
            piece.transform.localScale = Vector3.one * 0.5f;

            // Add material with color
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = colors[i];
            piece.GetComponent<Renderer>().material = mat;

            // Add PlayerPiece component
            PlayerPiece playerPiece = piece.AddComponent<PlayerPiece>();

            // Add Animator component (optional, for future animations)
            Animator animator = piece.AddComponent<Animator>();

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(piece, prefabPath);
            playerPieces[i] = prefab;

            // Clean up temporary object
            DestroyImmediate(piece);

            Debug.Log($"Created prefab: {prefabPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return playerPieces;
    }

    private static void CreateGameScene(GameObject[] playerPrefabs)
    {
        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        newScene.name = "GameScene";

        // Create Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(5f, 15f, -10f);
        cameraObj.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        cameraObj.AddComponent<AudioListener>();

        // Create Directional Light
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Create Game Manager
        GameObject gameManager = new GameObject("GameManager");

        // Add ServiceLocator first (required by other components)
        ServiceLocator serviceLocator = gameManager.AddComponent<ServiceLocator>();

        // Add EventBus
        EventBus eventBus = gameManager.AddComponent<EventBus>();

        // Add PoolManager
        PoolManager poolManager = gameManager.AddComponent<PoolManager>();

        // Add core gameplay components
        GameController gameController = gameManager.AddComponent<GameController>();
        PlayerManager playerManager = gameManager.AddComponent<PlayerManager>();
        MovementSystem movementSystem = gameManager.AddComponent<MovementSystem>();
        BoardManager boardManager = gameManager.AddComponent<BoardManager>();
        BoardModel boardModel = gameManager.AddComponent<BoardModel>();
        GameStateMachine stateMachine = gameManager.AddComponent<GameStateMachine>();

        // Assign player prefabs to PlayerManager
        playerManager.playerPiecePrefabs = playerPrefabs;
        playerManager.spawnPosition = new Vector3(0, 0.5f, 0);

        // Load and assign configs
        GameConfig gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>($"{CONFIG_PATH}/GameConfig.asset");
        BoardConfig boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfig>($"{CONFIG_PATH}/BoardConfig.asset");

        if (gameConfig != null)
        {
            SerializedObject so = new SerializedObject(movementSystem);
            so.FindProperty("gameConfig").objectReferenceValue = gameConfig;
            so.ApplyModifiedProperties();
        }

        if (boardConfig != null)
        {
            SerializedObject so = new SerializedObject(boardManager);
            so.FindProperty("boardConfig").objectReferenceValue = boardConfig;
            so.ApplyModifiedProperties();
        }

        // Create Board
        GameObject board = new GameObject("Board");
        BoardGenerator boardGenerator = board.AddComponent<BoardGenerator>();
        boardGenerator.rows = 10;
        boardGenerator.columns = 10;
        boardGenerator.tileSize = 1.5f;
        boardGenerator.snakeCount = 5;
        boardGenerator.ladderCount = 5;

        // Create Dice
        GameObject dice = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dice.name = "Dice";
        dice.transform.position = new Vector3(15f, 1f, 5f);
        dice.transform.localScale = Vector3.one * 0.5f;

        DiceController diceController = dice.AddComponent<DiceController>();
        DiceView diceView = dice.AddComponent<DiceView>();
        DiceModel diceModel = dice.AddComponent<DiceModel>();
        Animator diceAnimator = dice.AddComponent<Animator>();

        DiceConfig diceConfig = AssetDatabase.LoadAssetAtPath<DiceConfig>($"{CONFIG_PATH}/DiceConfig.asset");
        if (diceConfig != null)
        {
            SerializedObject so = new SerializedObject(diceController);
            so.FindProperty("diceConfig").objectReferenceValue = diceConfig;
            so.ApplyModifiedProperties();
        }

        // Create UI Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create GameHUD
        GameObject hudObj = new GameObject("GameHUD");
        hudObj.transform.SetParent(canvasObj.transform);
        GameHUD gameHUD = hudObj.AddComponent<GameHUD>();

        // Create HUD UI elements
        CreateHUDElements(hudObj, gameHUD, diceController);

        // Create GameOverUI
        GameObject gameOverObj = new GameObject("GameOverPanel");
        gameOverObj.transform.SetParent(canvasObj.transform);
        GameOverUI gameOverUI = gameOverObj.AddComponent<GameOverUI>();

        // Create Game Over UI elements
        CreateGameOverElements(gameOverObj, gameOverUI);

        // Initially hide game over panel
        gameOverObj.SetActive(false);

        // Add GameInitializer to setup services
        GameObject initializer = new GameObject("GameInitializer");
        GameInitializer gameInitializer = initializer.AddComponent<GameInitializer>();

        // Save scene
        string scenePath = $"{SCENES_PATH}/GameScene.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"Created scene: {scenePath}");
    }

    private static void CreateHUDElements(GameObject parent, GameHUD hud, DiceController diceController)
    {
        // Create background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(parent.transform);
        UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.8f);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Current Player Text
        GameObject currentPlayerText = CreateUIText("CurrentPlayerText", "Current Player: Player 1", panel.transform);
        PositionText(currentPlayerText, new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(300, 30));

        // Position Text
        GameObject positionText = CreateUIText("PositionText", "Position: 0", panel.transform);
        PositionText(positionText, new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -50), new Vector2(300, 30));

        // Turn Text
        GameObject turnText = CreateUIText("TurnText", "Turn: 1", panel.transform);
        PositionText(turnText, new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -90), new Vector2(300, 30));

        // Dice Result Text
        GameObject diceResultText = CreateUIText("DiceResultText", "Roll the dice!", panel.transform);
        PositionText(diceResultText, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(400, 40));
        diceResultText.GetComponent<UnityEngine.UI.Text>().fontSize = 24;
        diceResultText.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleCenter;

        // Roll Dice Button
        GameObject rollButton = CreateUIButton("RollDiceButton", "ROLL DICE", panel.transform);
        PositionButton(rollButton, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(150, 40));

        // Connect button to DiceController
        UnityEngine.UI.Button buttonComponent = rollButton.GetComponent<UnityEngine.UI.Button>();
        buttonComponent.onClick.AddListener(() => diceController.RollDice());

        // Event Log (ScrollView)
        GameObject eventLog = CreateEventLog("EventLog", panel.transform);

        // Assign references to GameHUD
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("currentPlayerText").objectReferenceValue = currentPlayerText.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("positionText").objectReferenceValue = positionText.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("turnText").objectReferenceValue = turnText.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("diceResultText").objectReferenceValue = diceResultText.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("rollDiceButton").objectReferenceValue = rollButton.GetComponent<UnityEngine.UI.Button>();
        so.ApplyModifiedProperties();
    }

    private static void CreateGameOverElements(GameObject parent, GameOverUI gameOverUI)
    {
        // Full screen panel
        UnityEngine.UI.Image panelImage = parent.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = parent.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Victory Text
        GameObject victoryText = CreateUIText("VictoryText", "PLAYER 1 WINS!", parent.transform);
        PositionText(victoryText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(600, 100));
        UnityEngine.UI.Text victoryTextComponent = victoryText.GetComponent<UnityEngine.UI.Text>();
        victoryTextComponent.fontSize = 48;
        victoryTextComponent.alignment = TextAnchor.MiddleCenter;
        victoryTextComponent.fontStyle = FontStyle.Bold;

        // Restart Button
        GameObject restartButton = CreateUIButton("RestartButton", "PLAY AGAIN", parent.transform);
        PositionButton(restartButton, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-100, -50), new Vector2(180, 50));

        // Main Menu Button
        GameObject menuButton = CreateUIButton("MainMenuButton", "MAIN MENU", parent.transform);
        PositionButton(menuButton, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(100, -50), new Vector2(180, 50));

        // Assign references
        SerializedObject so = new SerializedObject(gameOverUI);
        so.FindProperty("gameOverPanel").objectReferenceValue = parent;
        so.FindProperty("victoryText").objectReferenceValue = victoryTextComponent;
        so.FindProperty("restartButton").objectReferenceValue = restartButton.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("mainMenuButton").objectReferenceValue = menuButton.GetComponent<UnityEngine.UI.Button>();
        so.ApplyModifiedProperties();
    }

    private static GameObject CreateUIText(string name, string text, Transform parent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        UnityEngine.UI.Text textComponent = textObj.AddComponent<UnityEngine.UI.Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 18;
        textComponent.color = Color.white;
        return textObj;
    }

    private static GameObject CreateUIButton(string name, string text, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        UnityEngine.UI.Text textComponent = textObj.AddComponent<UnityEngine.UI.Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 18;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return buttonObj;
    }

    private static GameObject CreateEventLog(string name, Transform parent)
    {
        GameObject scrollView = new GameObject(name);
        scrollView.transform.SetParent(parent);
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 0.3f);
        scrollRect.offsetMin = new Vector2(10, 10);
        scrollRect.offsetMax = new Vector2(-10, -10);

        UnityEngine.UI.Image scrollBg = scrollView.AddComponent<UnityEngine.UI.Image>();
        scrollBg.color = new Color(0, 0, 0, 0.3f);

        return scrollView;
    }

    private static void PositionText(GameObject textObj, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private static void PositionButton(GameObject buttonObj, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }
}
