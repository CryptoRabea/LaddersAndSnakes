using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using LAS.Entities;
using LAS.Config;

namespace LAS.UI
{
    /// <summary>
    /// Runtime UI builder that creates game UI elements automatically when the scene starts.
    /// This ensures the UI is always present, even without using the editor setup tool.
    /// Attach this to the GameCanvas GameObject.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class RuntimeUIBuilder : MonoBehaviour
    {
        [Header("Auto-Build Settings")]
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool skipIfElementsExist = true;

        private void Awake()
        {
            if (buildOnStart)
            {
                BuildUI();
            }
        }

        /// <summary>
        /// Builds all UI elements programmatically
        /// </summary>
        public void BuildUI()
        {
            // Skip if UI already exists
            if (skipIfElementsExist && transform.Find("RollDiceButton") != null)
            {
                Debug.Log("[RuntimeUIBuilder] UI elements already exist, skipping build");
                return;
            }

            Debug.Log("[RuntimeUIBuilder] Building game UI...");

            // Ensure Canvas is configured correctly
            SetupCanvas();

            // Ensure EventSystem exists
            EnsureEventSystem();

            // Ensure DiceModel exists
            EnsureDiceModel();

            // Create UI elements
            CreateTurnIndicator();
            CreateRollDiceButton();
            CreateDiceResultText();
            CreateGameOverPanel();

            // Wire up GameUIManager references
            WireUIManager();

            Debug.Log("[RuntimeUIBuilder] UI build complete!");
        }

        private void SetupCanvas()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
                Debug.Log("[RuntimeUIBuilder] Created EventSystem");
            }
        }

        private void EnsureDiceModel()
        {
            // Check if DiceModel already exists
            var existingDiceModel = FindObjectOfType<DiceModel>();
            if (existingDiceModel != null)
            {
                Debug.Log("[RuntimeUIBuilder] DiceModel already exists");
                return;
            }

            // Create DiceModel GameObject
            var diceModelObj = new GameObject("DiceModel");
            var diceModel = diceModelObj.AddComponent<DiceModel>();

            // Try to load the DiceConfig asset
            var diceConfig = Resources.Load<DiceConfig>("New Dice Config");
            if (diceConfig == null)
            {
                // Try to find any DiceConfig in the project
                #if UNITY_EDITOR
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DiceConfig");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    diceConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<DiceConfig>(path);
                    Debug.Log($"[RuntimeUIBuilder] Loaded DiceConfig from {path}");
                }
                #endif
            }

            // If still no config, create a default one at runtime
            if (diceConfig == null)
            {
                diceConfig = ScriptableObject.CreateInstance<DiceConfig>();
                diceConfig.sides = 6;
                diceConfig.rollDuration = 1.2f;
                Debug.Log("[RuntimeUIBuilder] Created runtime DiceConfig with default values");
            }

            diceModel.config = diceConfig;
            Debug.Log("[RuntimeUIBuilder] Created DiceModel and assigned config");
        }

        private void CreateTurnIndicator()
        {
            var turnIndicatorObj = transform.Find("TurnIndicator");
            if (turnIndicatorObj != null)
            {
                Debug.Log("[RuntimeUIBuilder] TurnIndicator already exists");
                return;
            }

            var indicator = new GameObject("TurnIndicator");
            indicator.transform.SetParent(transform, false);

            var rectTransform = indicator.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0, -20);
            rectTransform.sizeDelta = new Vector2(400, 60);

            var text = indicator.AddComponent<TextMeshProUGUI>();
            text.text = "Player 1's Turn";
            text.fontSize = 36;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            Debug.Log("[RuntimeUIBuilder] Created TurnIndicator");
        }

        private void CreateRollDiceButton()
        {
            var buttonObj = transform.Find("RollDiceButton");
            if (buttonObj != null)
            {
                Debug.Log("[RuntimeUIBuilder] RollDiceButton already exists");
                return;
            }

            // Create button GameObject
            var button = new GameObject("RollDiceButton");
            button.transform.SetParent(transform, false);

            // Setup RectTransform
            var rectTransform = button.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0, 40);
            rectTransform.sizeDelta = new Vector2(200, 60);

            // Add Image component
            var image = button.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 1f);

            // Add Button component
            var buttonComponent = button.AddComponent<Button>();

            // Create button text child
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Roll Dice";
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            Debug.Log("[RuntimeUIBuilder] Created RollDiceButton");
        }

        private void CreateDiceResultText()
        {
            var resultObj = transform.Find("DiceResultText");
            if (resultObj != null)
            {
                Debug.Log("[RuntimeUIBuilder] DiceResultText already exists");
                return;
            }

            var result = new GameObject("DiceResultText");
            result.transform.SetParent(transform, false);

            var rectTransform = result.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0, 120);
            rectTransform.sizeDelta = new Vector2(200, 40);

            var text = result.AddComponent<TextMeshProUGUI>();
            text.text = "Rolled: -";
            text.fontSize = 28;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.yellow;

            Debug.Log("[RuntimeUIBuilder] Created DiceResultText");
        }

        private void CreateGameOverPanel()
        {
            var panelObj = transform.Find("GameOverPanel");
            if (panelObj != null)
            {
                Debug.Log("[RuntimeUIBuilder] GameOverPanel already exists");
                return;
            }

            // Create panel
            var panel = new GameObject("GameOverPanel");
            panel.transform.SetParent(transform, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // Create winner text
            var winnerText = new GameObject("WinnerText");
            winnerText.transform.SetParent(panel.transform, false);

            var winnerRect = winnerText.AddComponent<RectTransform>();
            winnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            winnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            winnerRect.pivot = new Vector2(0.5f, 0.5f);
            winnerRect.anchoredPosition = new Vector2(0, 100);
            winnerRect.sizeDelta = new Vector2(600, 100);

            var winnerTextComponent = winnerText.AddComponent<TextMeshProUGUI>();
            winnerTextComponent.text = "Player 1 Wins!";
            winnerTextComponent.fontSize = 48;
            winnerTextComponent.alignment = TextAlignmentOptions.Center;
            winnerTextComponent.color = Color.yellow;

            // Create Play Again button
            CreateButton(panel.transform, "PlayAgainButton", "Play Again", new Vector2(0, -50), new Vector2(200, 60));

            // Create Main Menu button
            CreateButton(panel.transform, "MainMenuButton", "Main Menu", new Vector2(0, -130), new Vector2(200, 60));

            // Hide panel initially
            panel.SetActive(false);

            Debug.Log("[RuntimeUIBuilder] Created GameOverPanel");
        }

        private void CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            var button = new GameObject(name);
            button.transform.SetParent(parent, false);

            var rectTransform = button.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            var image = button.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 1f);

            var buttonComponent = button.AddComponent<Button>();

            // Create button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
        }

        private void WireUIManager()
        {
            // Get or add GameUIManager component
            var uiManager = GetComponent<GameUIManager>();
            if (uiManager == null)
            {
                uiManager = gameObject.AddComponent<GameUIManager>();
                Debug.Log("[RuntimeUIBuilder] Added GameUIManager component");
            }

            // Use reflection to set private fields
            var managerType = typeof(GameUIManager);

            // Set button reference
            var rollDiceButton = transform.Find("RollDiceButton")?.GetComponent<Button>();
            if (rollDiceButton != null)
            {
                var buttonField = managerType.GetField("rollDiceButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                buttonField?.SetValue(uiManager, rollDiceButton);
            }

            // Set turn indicator text
            var turnIndicatorText = transform.Find("TurnIndicator")?.GetComponent<TextMeshProUGUI>();
            if (turnIndicatorText != null)
            {
                var textField = managerType.GetField("turnIndicatorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                textField?.SetValue(uiManager, turnIndicatorText);
            }

            // Set dice result text
            var diceResultText = transform.Find("DiceResultText")?.GetComponent<TextMeshProUGUI>();
            if (diceResultText != null)
            {
                var textField = managerType.GetField("diceResultText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                textField?.SetValue(uiManager, diceResultText);
            }

            // Set game over panel
            var gameOverPanel = transform.Find("GameOverPanel")?.gameObject;
            if (gameOverPanel != null)
            {
                var panelField = managerType.GetField("gameOverPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                panelField?.SetValue(uiManager, gameOverPanel);

                // Set winner text
                var winnerText = gameOverPanel.transform.Find("WinnerText")?.GetComponent<TextMeshProUGUI>();
                if (winnerText != null)
                {
                    var winnerField = managerType.GetField("winnerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    winnerField?.SetValue(uiManager, winnerText);
                }

                // Set play again button
                var playAgainButton = gameOverPanel.transform.Find("PlayAgainButton")?.GetComponent<Button>();
                if (playAgainButton != null)
                {
                    var playField = managerType.GetField("playAgainButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    playField?.SetValue(uiManager, playAgainButton);
                }

                // Set main menu button
                var mainMenuButton = gameOverPanel.transform.Find("MainMenuButton")?.GetComponent<Button>();
                if (mainMenuButton != null)
                {
                    var menuField = managerType.GetField("mainMenuButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    menuField?.SetValue(uiManager, mainMenuButton);
                }
            }

            Debug.Log("[RuntimeUIBuilder] Wired up GameUIManager references");
        }
    }
}
