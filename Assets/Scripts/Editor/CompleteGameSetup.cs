#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using LAS.Config;
using LAS.Entities;
using LAS.Gameplay;
using LAS.Networking;
using LAS.UI;

namespace LAS.Editor
{
    /// <summary>
    /// Complete unified game setup tool - configure prefabs and setup the entire playable game in one go
    /// </summary>
    public class CompleteGameSetupWindow : EditorWindow
    {
        // Prefab References
        private GameObject boardPrefab;
        private GameObject playerPiecePrefab;
        private GameObject dicePrefab;
        private BoardConfig boardConfig;
        private GameConfig gameConfig;
        private DiceConfig diceConfig;

        // Setup Options
        private bool setupBoard = true;
        private bool setupUI = true;
        private bool setupPlayers = true;
        private bool setupDice = true;
        private bool setupAI = false;
        private int numberOfPlayers = 2;
        private bool autoBindUI = true;

        // Scene References (auto-found)
        private GameController gameController;
        private Canvas gameCanvas;

        private Vector2 scrollPosition;

        [MenuItem("LAS/Complete Game Setup (All-in-One)", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<CompleteGameSetupWindow>("Complete Game Setup");
            window.minSize = new Vector2(450, 700);
            window.Show();
        }

        private void OnEnable()
        {
            // Try to load default configs
            LoadDefaultConfigs();
        }

        private void LoadDefaultConfigs()
        {
            // Find configs in the project
            var boardConfigs = AssetDatabase.FindAssets("t:BoardConfig");
            if (boardConfigs.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(boardConfigs[0]);
                boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfig>(path);
            }

            var gameConfigs = AssetDatabase.FindAssets("t:GameConfig");
            if (gameConfigs.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(gameConfigs[0]);
                gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>(path);
            }

            var diceConfigs = AssetDatabase.FindAssets("t:DiceConfig");
            if (diceConfigs.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(diceConfigs[0]);
                diceConfig = AssetDatabase.LoadAssetAtPath<DiceConfig>(path);
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            GUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 16;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("COMPLETE GAME SETUP", headerStyle);
            EditorGUILayout.HelpBox("Configure all prefabs and setup a fully playable game scene in one click!", MessageType.Info);

            GUILayout.Space(10);

            // Prefab Configuration Section
            DrawSectionHeader("PREFAB CONFIGURATION");
            boardPrefab = (GameObject)EditorGUILayout.ObjectField("Board Prefab", boardPrefab, typeof(GameObject), false);
            playerPiecePrefab = (GameObject)EditorGUILayout.ObjectField("Player Piece Prefab", playerPiecePrefab, typeof(GameObject), false);
            dicePrefab = (GameObject)EditorGUILayout.ObjectField("Dice Prefab", dicePrefab, typeof(GameObject), false);

            GUILayout.Space(5);

            // Configuration Assets Section
            DrawSectionHeader("CONFIGURATION ASSETS");
            boardConfig = (BoardConfig)EditorGUILayout.ObjectField("Board Config", boardConfig, typeof(BoardConfig), false);
            gameConfig = (GameConfig)EditorGUILayout.ObjectField("Game Config", gameConfig, typeof(GameConfig), false);
            diceConfig = (DiceConfig)EditorGUILayout.ObjectField("Dice Config", diceConfig, typeof(DiceConfig), false);

            GUILayout.Space(5);

            // Quick Create Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Default Board Config", GUILayout.Height(25)))
            {
                BoardConfigGenerator.CreateDefaultBoardConfig();
                LoadDefaultConfigs();
            }
            if (GUILayout.Button("Open Board Generator", GUILayout.Height(25)))
            {
                BoardGeneratorWindow.ShowWindow();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Setup Options Section
            DrawSectionHeader("SETUP OPTIONS");
            setupBoard = EditorGUILayout.Toggle("Setup Board", setupBoard);
            setupUI = EditorGUILayout.Toggle("Setup UI", setupUI);
            setupPlayers = EditorGUILayout.Toggle("Setup Players", setupPlayers);
            setupDice = EditorGUILayout.Toggle("Setup Dice", setupDice);

            GUILayout.Space(5);
            setupAI = EditorGUILayout.Toggle("Enable AI Opponent", setupAI);
            numberOfPlayers = EditorGUILayout.IntSlider("Number of Players", numberOfPlayers, 2, 4);
            autoBindUI = EditorGUILayout.Toggle("Auto-Bind UI References", autoBindUI);

            GUILayout.Space(10);

            // Current Scene Info
            DrawSectionHeader("CURRENT SCENE");
            FindSceneReferences();

            EditorGUILayout.LabelField("GameController:", gameController != null ? "✓ Found" : "✗ Missing",
                gameController != null ? EditorStyles.label : GetErrorStyle());
            EditorGUILayout.LabelField("Canvas:", gameCanvas != null ? "✓ Found" : "✗ Missing",
                gameCanvas != null ? EditorStyles.label : GetErrorStyle());

            GUILayout.Space(10);

            // Main Setup Button
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("SETUP COMPLETE GAME NOW", GUILayout.Height(50)))
            {
                SetupCompleteGame();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Quick Actions
            DrawSectionHeader("QUICK ACTIONS");
            if (GUILayout.Button("Create New Game Scene", GUILayout.Height(30)))
            {
                CreateNewGameScene();
            }
            if (GUILayout.Button("Validate Current Setup", GUILayout.Height(30)))
            {
                ValidateSetup();
            }

            GUILayout.Space(10);

            EditorGUILayout.EndScrollView();
        }

        private void DrawSectionHeader(string title)
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.fontSize = 12;
            EditorGUILayout.LabelField(title, style);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(5);
        }

        private GUIStyle GetErrorStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = Color.red;
            return style;
        }

        private void FindSceneReferences()
        {
            gameController = FindFirstObjectByType<GameController>();
            gameCanvas = FindFirstObjectByType<Canvas>();
        }

        private void SetupCompleteGame()
        {
            if (!ValidateInputs())
                return;

            Debug.Log("[CompleteGameSetup] Starting complete game setup...");

            // Ensure we have a game controller
            EnsureGameController();

            // Setup components in order
            if (setupBoard)
                SetupBoardInScene();

            if (setupUI)
                SetupUISystem();

            if (setupPlayers)
                SetupPlayerPieces();

            if (setupDice)
                SetupDiceSystem();

            if (setupAI)
                SetupAIController();

            // Final steps
            if (autoBindUI)
                UIReferenceEditorMenu.BindAllUIReferencesInScene();

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("[CompleteGameSetup] ✓ Complete game setup finished!");
            EditorUtility.DisplayDialog("Setup Complete",
                "Game is now fully configured and ready to play!\n\n" +
                "✓ Board setup\n" +
                "✓ UI configured\n" +
                "✓ Players ready\n" +
                "✓ Dice system active\n" +
                (setupAI ? "✓ AI enabled\n" : "") +
                "\nPress PLAY to test the game!", "Awesome!");
        }

        private bool ValidateInputs()
        {
            string errors = "";

            if (setupBoard && boardConfig == null)
                errors += "- Board Config is required\n";

            if (setupDice && diceConfig == null)
                errors += "- Dice Config is required\n";

            if (setupPlayers && gameConfig == null)
                errors += "- Game Config is required\n";

            if (!string.IsNullOrEmpty(errors))
            {
                EditorUtility.DisplayDialog("Missing Configuration",
                    "Please assign the following:\n\n" + errors, "OK");
                return false;
            }

            return true;
        }

        private void EnsureGameController()
        {
            FindSceneReferences();

            if (gameController == null)
            {
                // Create GameController
                GameObject controllerGO = new GameObject("GameController");
                gameController = controllerGO.AddComponent<MultiplayerGameController>();
                Debug.Log("[CompleteGameSetup] Created GameController");
            }

            // Ensure NetworkManager exists
            var networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                GameObject netGO = new GameObject("NetworkManager");
                networkManager = netGO.AddComponent<NetworkManager>();

                // Configure for single player with AI
                SerializedObject so = new SerializedObject(networkManager);
                so.FindProperty("isSinglePlayerAI").boolValue = setupAI;
                so.ApplyModifiedProperties();

                Debug.Log("[CompleteGameSetup] Created NetworkManager");
            }
        }

        private void SetupBoardInScene()
        {
            // Find or create BoardRoot
            GameObject boardRoot = GameObject.Find("BoardRoot");
            if (boardRoot == null)
            {
                boardRoot = new GameObject("BoardRoot");
                boardRoot.transform.position = Vector3.zero;
            }

            // Instantiate board prefab if provided
            if (boardPrefab != null)
            {
                // Clear existing board
                while (boardRoot.transform.childCount > 0)
                {
                    DestroyImmediate(boardRoot.transform.GetChild(0).gameObject);
                }

                GameObject board = (GameObject)PrefabUtility.InstantiatePrefab(boardPrefab, boardRoot.transform);
                board.transform.localPosition = Vector3.zero;
                Debug.Log("[CompleteGameSetup] Instantiated board prefab");
            }

            // Setup BoardModel
            var boardModel = boardRoot.GetComponent<BoardModel>();
            if (boardModel == null)
            {
                boardModel = boardRoot.AddComponent<BoardModel>();
            }

            // Assign board config
            SerializedObject so = new SerializedObject(boardModel);
            so.FindProperty("config").objectReferenceValue = boardConfig;
            so.ApplyModifiedProperties();

            // Generate square transforms if not present
            GenerateBoardSquares(boardRoot);

            Debug.Log("[CompleteGameSetup] Board setup complete");
        }

        private void GenerateBoardSquares(GameObject boardRoot)
        {
            if (boardConfig == null)
                return;

            // Create simple square layout (10x10 grid)
            GameObject squaresContainer = boardRoot.transform.Find("Squares")?.gameObject;
            if (squaresContainer == null)
            {
                squaresContainer = new GameObject("Squares");
                squaresContainer.transform.SetParent(boardRoot.transform);
                squaresContainer.transform.localPosition = Vector3.zero;
            }

            // Only create if empty
            if (squaresContainer.transform.childCount == 0)
            {
                int gridSize = 10;
                float spacing = 1.5f;

                for (int i = 0; i < 100; i++)
                {
                    GameObject square = new GameObject($"Square_{i + 1}");
                    square.transform.SetParent(squaresContainer.transform);

                    // Calculate position in snake pattern
                    int row = i / gridSize;
                    int col = (row % 2 == 0) ? (i % gridSize) : (gridSize - 1 - (i % gridSize));

                    square.transform.localPosition = new Vector3(col * spacing, 0, row * spacing);

                    // Add visual cube
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.SetParent(square.transform);
                    cube.transform.localPosition = Vector3.zero;
                    cube.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);

                    // Color alternating squares
                    var renderer = cube.GetComponent<Renderer>();
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = ((i / gridSize + i % gridSize) % 2 == 0) ?
                        new Color(0.9f, 0.9f, 0.9f) : new Color(0.7f, 0.7f, 0.7f);
                }

                Debug.Log("[CompleteGameSetup] Generated 100 board squares");
            }
        }

        private void SetupUISystem()
        {
            // Find or create Canvas
            if (gameCanvas == null)
            {
                GameObject canvasGO = new GameObject("GameCanvas");
                gameCanvas = canvasGO.AddComponent<Canvas>();
                gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasGO.AddComponent<GraphicRaycaster>();
                Debug.Log("[CompleteGameSetup] Created Canvas");
            }

            // Ensure EventSystem exists
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[CompleteGameSetup] Created EventSystem");
            }

            // Add RuntimeUIBuilder
            var uiBuilder = gameCanvas.GetComponent<RuntimeUIBuilder>();
            if (uiBuilder == null)
            {
                uiBuilder = gameCanvas.gameObject.AddComponent<RuntimeUIBuilder>();
                Debug.Log("[CompleteGameSetup] Added RuntimeUIBuilder");
            }

            // Add GameUIManager
            var uiManager = gameCanvas.GetComponent<GameUIManager>();
            if (uiManager == null)
            {
                uiManager = gameCanvas.gameObject.AddComponent<GameUIManager>();
                Debug.Log("[CompleteGameSetup] Added GameUIManager");
            }

            // Force build UI now
            uiBuilder.BuildUI();

            Debug.Log("[CompleteGameSetup] UI system setup complete");
        }

        private void SetupPlayerPieces()
        {
            // Find or create Players container
            GameObject playersRoot = GameObject.Find("Players");
            if (playersRoot == null)
            {
                playersRoot = new GameObject("Players");
            }

            // Clear existing players
            while (playersRoot.transform.childCount > 0)
            {
                DestroyImmediate(playersRoot.transform.GetChild(0).gameObject);
            }

            // Create player pieces
            for (int i = 0; i < numberOfPlayers; i++)
            {
                GameObject playerGO;

                if (playerPiecePrefab != null)
                {
                    playerGO = (GameObject)PrefabUtility.InstantiatePrefab(playerPiecePrefab, playersRoot.transform);
                }
                else
                {
                    // Create simple cylinder as player piece
                    playerGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    playerGO.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
                    playerGO.transform.SetParent(playersRoot.transform);
                }

                playerGO.name = $"Player{i + 1}";
                playerGO.transform.position = Vector3.zero + Vector3.up * 0.5f;

                // Add PlayerPiece component
                var playerPiece = playerGO.GetComponent<PlayerPiece>();
                if (playerPiece == null)
                {
                    playerPiece = playerGO.AddComponent<PlayerPiece>();
                }

                // Set player color
                var renderer = playerGO.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = GetPlayerColor(i);
                }

                SerializedObject so = new SerializedObject(playerPiece);
                so.FindProperty("playerIndex").intValue = i;
                so.FindProperty("currentIndex").intValue = 1;
                so.ApplyModifiedProperties();
            }

            // Setup MovementSystem
            GameObject boardRoot = GameObject.Find("BoardRoot");
            if (boardRoot != null)
            {
                var movementSystem = boardRoot.GetComponent<MovementSystem>();
                if (movementSystem == null)
                {
                    movementSystem = boardRoot.AddComponent<MovementSystem>();
                }

                // Assign references
                SerializedObject so = new SerializedObject(movementSystem);

                // Set player pieces array
                var playerPiecesProperty = so.FindProperty("playerPieces");
                playerPiecesProperty.arraySize = numberOfPlayers;
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    var player = playersRoot.transform.GetChild(i);
                    playerPiecesProperty.GetArrayElementAtIndex(i).objectReferenceValue = player;
                }

                // Set square transforms
                var squaresContainer = boardRoot.transform.Find("Squares");
                if (squaresContainer != null)
                {
                    var squaresProperty = so.FindProperty("squareTransforms");
                    squaresProperty.arraySize = squaresContainer.childCount;
                    for (int i = 0; i < squaresContainer.childCount; i++)
                    {
                        squaresProperty.GetArrayElementAtIndex(i).objectReferenceValue =
                            squaresContainer.GetChild(i);
                    }
                }

                so.FindProperty("boardModel").objectReferenceValue = boardRoot.GetComponent<BoardModel>();
                so.FindProperty("gameConfig").objectReferenceValue = gameConfig;
                so.ApplyModifiedProperties();
            }

            Debug.Log($"[CompleteGameSetup] Created {numberOfPlayers} player pieces");
        }

        private void SetupDiceSystem()
        {
            // Find or create Dice container
            GameObject diceRoot = GameObject.Find("DiceSystem");
            if (diceRoot == null)
            {
                diceRoot = new GameObject("DiceSystem");
            }

            // Add DiceModel
            var diceModel = diceRoot.GetComponent<DiceModel>();
            if (diceModel == null)
            {
                diceModel = diceRoot.AddComponent<DiceModel>();
            }

            // Configure DiceModel
            SerializedObject so = new SerializedObject(diceModel);
            so.FindProperty("config").objectReferenceValue = diceConfig;
            so.FindProperty("dicePrefab").objectReferenceValue = dicePrefab;

            // Add DiceView if prefab not provided
            if (dicePrefab == null)
            {
                var diceView = diceRoot.GetComponent<DiceView>();
                if (diceView == null)
                {
                    diceView = diceRoot.AddComponent<DiceView>();
                }

                SerializedObject soView = new SerializedObject(diceView);
                soView.FindProperty("config").objectReferenceValue = diceConfig;
                soView.ApplyModifiedProperties();

                so.FindProperty("diceView").objectReferenceValue = diceView;
            }

            so.ApplyModifiedProperties();

            diceRoot.transform.position = new Vector3(0, 2, 0);

            Debug.Log("[CompleteGameSetup] Dice system setup complete");
        }

        private void SetupAIController()
        {
            if (gameController == null)
                return;

            var aiController = gameController.GetComponent<AIPlayerController>();
            if (aiController == null)
            {
                aiController = gameController.gameObject.AddComponent<AIPlayerController>();
            }

            SerializedObject so = new SerializedObject(aiController);
            so.FindProperty("aiPlayerIndex").intValue = 1; // Player 2 is AI
            so.FindProperty("thinkDelay").floatValue = 1.5f;
            so.ApplyModifiedProperties();

            Debug.Log("[CompleteGameSetup] AI controller configured");
        }

        private void CreateNewGameScene()
        {
            if (EditorUtility.DisplayDialog("Create New Game Scene",
                "This will create a completely new game scene. Continue?", "Yes", "Cancel"))
            {
                SceneBuilder.BuildGameScene();
                EditorUtility.DisplayDialog("Scene Created",
                    "New Game scene created! Now use 'Setup Complete Game' to configure it.", "OK");
            }
        }

        private void ValidateSetup()
        {
            string report = "=== GAME SETUP VALIDATION ===\n\n";

            FindSceneReferences();

            report += "Core Systems:\n";
            report += gameController != null ? "✓ GameController\n" : "✗ GameController (MISSING)\n";
            report += FindFirstObjectByType<NetworkManager>() != null ? "✓ NetworkManager\n" : "✗ NetworkManager\n";
            report += gameCanvas != null ? "✓ Canvas\n" : "✗ Canvas (MISSING)\n";

            report += "\nBoard:\n";
            var boardRoot = GameObject.Find("BoardRoot");
            report += boardRoot != null ? "✓ BoardRoot\n" : "✗ BoardRoot (MISSING)\n";
            if (boardRoot != null)
            {
                var boardModel = boardRoot.GetComponent<BoardModel>();
                report += boardModel != null ? "✓ BoardModel\n" : "✗ BoardModel\n";
                var movementSystem = boardRoot.GetComponent<MovementSystem>();
                report += movementSystem != null ? "✓ MovementSystem\n" : "✗ MovementSystem\n";
            }

            report += "\nPlayers:\n";
            var players = GameObject.Find("Players");
            if (players != null)
            {
                report += $"✓ {players.transform.childCount} player pieces found\n";
            }
            else
            {
                report += "✗ No players (MISSING)\n";
            }

            report += "\nDice:\n";
            var dice = FindFirstObjectByType<DiceModel>();
            report += dice != null ? "✓ DiceModel\n" : "✗ DiceModel (MISSING)\n";

            report += "\nUI:\n";
            var uiManager = FindFirstObjectByType<GameUIManager>();
            report += uiManager != null ? "✓ GameUIManager\n" : "✗ GameUIManager\n";
            var uiBuilder = FindFirstObjectByType<RuntimeUIBuilder>();
            report += uiBuilder != null ? "✓ RuntimeUIBuilder\n" : "✗ RuntimeUIBuilder\n";

            Debug.Log(report);
            EditorUtility.DisplayDialog("Validation Report", report, "OK");
        }

        private Color GetPlayerColor(int index)
        {
            Color[] colors = new Color[]
            {
                Color.red,
                Color.blue,
                Color.green,
                Color.yellow
            };

            return colors[index % colors.Length];
        }
    }
}
#endif
