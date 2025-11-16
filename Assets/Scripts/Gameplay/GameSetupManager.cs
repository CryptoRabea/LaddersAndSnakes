using UnityEngine;
using LAS.Config;
using LAS.Entities;
using LAS.Networking;
using LAS.Core;

namespace LAS.Gameplay
{
    /// <summary>
    /// Automatically sets up the complete game scene with all required components
    /// This script should be attached to a GameObject in the GameScene
    /// </summary>
    public class GameSetupManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameSetupConfig setupConfig;

        [Header("Board Settings")]
        [SerializeField] private BoardConfig boardConfig;
        [SerializeField] private Material boardMaterial;
        [SerializeField] private Material ladderMaterial;
        [SerializeField] private Material snakeMaterial;

        [Header("Prefabs")]
        [SerializeField] private GameObject playerPiecePrefab;
        [SerializeField] private GameObject dicePrefab;

        [Header("Auto-Setup")]
        [SerializeField] private bool autoSetupOnStart = true;

        private GameObject boardObject;
        private GameObject[] playerPieces;
        private Transform[] squareTransforms;
        private BoardModel boardModel;
        private DiceModel diceModel;
        private MovementSystem movementSystem;
        private MultiplayerGameController gameController;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupGame();
            }
        }

        /// <summary>
        /// Sets up the entire game with all required components
        /// </summary>
        public void SetupGame()
        {
            Debug.Log("[GameSetupManager] Setting up game...");

            // Use default config if none assigned
            if (setupConfig == null)
            {
                Debug.LogWarning("[GameSetupManager] No setup config assigned, using defaults");
                setupConfig = ScriptableObject.CreateInstance<GameSetupConfig>();
            }

            // Load or create board config
            if (boardConfig == null)
            {
                boardConfig = Resources.Load<BoardConfig>("Config/DefaultBoardConfig");
                if (boardConfig == null)
                {
                    Debug.LogWarning("[GameSetupManager] No board config found, creating default");
                    boardConfig = ScriptableObject.CreateInstance<BoardConfig>();
                }
            }

            // Setup in order
            SetupBoard();
            SetupCamera();
            SetupPlayerPieces();
            SetupDice();
            SetupGameController();
            SetupMovementSystem();

            Debug.Log("[GameSetupManager] Game setup complete!");
        }

        private void SetupBoard()
        {
            // Check if board already exists
            boardObject = GameObject.Find("Board");
            if (boardObject == null)
            {
                boardObject = new GameObject("Board");
            }

            // Add BoardGenerator if needed
            var generator = boardObject.GetComponent<BoardGenerator>();
            if (generator == null)
            {
                generator = boardObject.AddComponent<BoardGenerator>();
            }

            // Assign materials
            if (boardMaterial != null) generator.boardMaterial = boardMaterial;
            if (ladderMaterial != null) generator.ladderMaterial = ladderMaterial;
            if (snakeMaterial != null) generator.snakeMaterial = snakeMaterial;

            // Generate the board
            generator.config = boardConfig;
            generator.GenerateBoard();

            // Get square transforms for movement
            var boardManager = boardObject.GetComponent<BoardManager>();
            if (boardManager == null)
            {
                boardManager = boardObject.AddComponent<BoardManager>();
            }

            // Store square transforms (assuming 100 tiles)
            squareTransforms = new Transform[100];
            for (int i = 0; i < 100; i++)
            {
                var squareObj = new GameObject($"Square_{i + 1}");
                squareObj.transform.parent = boardObject.transform;

                // Calculate position based on 10x10 grid
                int row = i / 10;
                int col = i % 10;

                // Handle snake pattern (alternating left-to-right)
                if (row % 2 == 1)
                    col = 9 - col;

                squareObj.transform.position = new Vector3(col, 0.1f, row);
                squareTransforms[i] = squareObj.transform;
            }

            // Create BoardModel
            boardModel = boardObject.GetComponent<BoardModel>();
            if (boardModel == null)
            {
                boardModel = boardObject.AddComponent<BoardModel>();
            }
            boardModel.config = boardConfig;

            Debug.Log("[GameSetupManager] Board setup complete");
        }

        private void SetupCamera()
        {
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                var camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            // Position camera to view the entire board
            mainCam.transform.position = new Vector3(4.5f, 15f, 4.5f);
            mainCam.transform.rotation = Quaternion.Euler(60, 0, 0);
            mainCam.orthographic = false;

            Debug.Log("[GameSetupManager] Camera setup complete");
        }

        private void SetupPlayerPieces()
        {
            int playerCount = setupConfig != null ? setupConfig.playerCount : 2;
            playerPieces = new GameObject[playerCount];

            var playerColors = new Color[]
            {
                Color.red,
                Color.blue,
                Color.green,
                Color.yellow
            };

            for (int i = 0; i < playerCount; i++)
            {
                GameObject pieceObj;

                if (playerPiecePrefab != null)
                {
                    pieceObj = Instantiate(playerPiecePrefab);
                }
                else
                {
                    // Create simple capsule if no prefab
                    pieceObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    pieceObj.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
                }

                pieceObj.name = $"Player_{i + 1}";

                // Position at start (slightly offset for each player)
                float offset = i * 0.2f;
                pieceObj.transform.position = squareTransforms[0].position + new Vector3(offset, 0, offset);

                // Set color
                var renderer = pieceObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Standard"));
                    mat.color = playerColors[i % playerColors.Length];
                    renderer.material = mat;
                }

                // Add PlayerPiece component
                var pieceComponent = pieceObj.GetComponent<PlayerPiece>();
                if (pieceComponent == null)
                {
                    pieceComponent = pieceObj.AddComponent<PlayerPiece>();
                }
                pieceComponent.playerIndex = i;
                pieceComponent.currentIndex = 1;

                // Add animator if not present
                if (pieceObj.GetComponent<Animator>() == null)
                {
                    pieceObj.AddComponent<Animator>();
                }

                playerPieces[i] = pieceObj;
            }

            Debug.Log($"[GameSetupManager] Created {playerCount} player pieces");
        }

        private void SetupDice()
        {
            var diceObj = GameObject.Find("Dice");
            if (diceObj == null)
            {
                if (dicePrefab != null)
                {
                    diceObj = Instantiate(dicePrefab);
                }
                else
                {
                    // Create simple cube if no prefab
                    diceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    diceObj.transform.localScale = Vector3.one * 0.5f;
                }
                diceObj.name = "Dice";
            }

            // Position dice off to the side
            diceObj.transform.position = new Vector3(-2, 1, 5);

            // Add DiceModel
            diceModel = diceObj.GetComponent<DiceModel>();
            if (diceModel == null)
            {
                diceModel = diceObj.AddComponent<DiceModel>();
            }

            // Load or create dice config
            var diceConfig = Resources.Load<DiceConfig>("Config/DefaultDiceConfig");
            if (diceConfig == null)
            {
                diceConfig = ScriptableObject.CreateInstance<DiceConfig>();
                diceConfig.sides = 6;
                diceConfig.rollDuration = 1.2f;
                diceConfig.spinTorque = new Vector3(400, 400, 400);
            }
            diceModel.config = diceConfig;

            // Add DiceView
            var diceView = diceObj.GetComponent<DiceView>();
            if (diceView == null)
            {
                diceView = diceObj.AddComponent<DiceView>();
            }
            diceView.config = diceConfig;

            // Add animator if not present
            if (diceObj.GetComponent<Animator>() == null)
            {
                diceObj.AddComponent<Animator>();
            }

            diceModel.diceView = diceView;

            Debug.Log("[GameSetupManager] Dice setup complete");
        }

        private void SetupGameController()
        {
            var controllerObj = GameObject.Find("GameController");
            if (controllerObj == null)
            {
                controllerObj = new GameObject("GameController");
            }

            // Add MultiplayerGameController
            gameController = controllerObj.GetComponent<MultiplayerGameController>();
            if (gameController == null)
            {
                gameController = controllerObj.AddComponent<MultiplayerGameController>();
            }

            // Add AIPlayerController if we're in single player AI mode
            var networkManager = NetworkManager.Instance;
            if (networkManager != null && networkManager.IsSinglePlayerAI)
            {
                var aiController = controllerObj.GetComponent<AIPlayerController>();
                if (aiController == null)
                {
                    aiController = controllerObj.AddComponent<AIPlayerController>();
                    Debug.Log("[GameSetupManager] Added AIPlayerController for single player AI mode");
                }
            }

            Debug.Log("[GameSetupManager] Game controller setup complete");
        }

        private void SetupMovementSystem()
        {
            var movementObj = GameObject.Find("MovementSystem");
            if (movementObj == null)
            {
                movementObj = new GameObject("MovementSystem");
            }

            movementSystem = movementObj.GetComponent<MovementSystem>();
            if (movementSystem == null)
            {
                movementSystem = movementObj.AddComponent<MovementSystem>();
            }

            // Assign references
            movementSystem.playerPieces = new Transform[playerPieces.Length];
            for (int i = 0; i < playerPieces.Length; i++)
            {
                movementSystem.playerPieces[i] = playerPieces[i].transform;
            }

            movementSystem.squareTransforms = squareTransforms;
            movementSystem.boardModel = boardModel;

            // Load or create game config
            var gameConfig = Resources.Load<GameConfig>("Config/DefaultGameConfig");
            if (gameConfig == null)
            {
                gameConfig = ScriptableObject.CreateInstance<GameConfig>();
                gameConfig.boardSize = 100;
                gameConfig.moveSpeed = 4f;
                gameConfig.moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
            movementSystem.gameConfig = gameConfig;

            Debug.Log("[GameSetupManager] Movement system setup complete");
        }

        /// <summary>
        /// Public method to start a new game with specific settings
        /// </summary>
        public void StartNewGame(int playerCount, bool enableNetworking)
        {
            if (setupConfig == null)
            {
                setupConfig = ScriptableObject.CreateInstance<GameSetupConfig>();
            }

            setupConfig.playerCount = playerCount;
            setupConfig.enableNetworking = enableNetworking;

            SetupGame();
        }
    }
}
