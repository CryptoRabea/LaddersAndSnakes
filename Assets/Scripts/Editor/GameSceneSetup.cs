#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LAS.Gameplay;
using LAS.UI;
using LAS.Entities;

namespace LAS.Editor
{
    /// <summary>
    /// Editor utility to automatically set up the GameScene with all required components
    /// </summary>
    public class GameSceneSetup : EditorWindow
    {
        [MenuItem("LAS/Setup Game Scene")]
        public static void SetupScene()
        {
            Debug.Log("[GameSceneSetup] Setting up GameScene...");

            // Create GameSetupManager
            CreateGameSetupManager();

            // Create UI Canvas
            CreateGameUI();

            // Create Lighting
            CreateLighting();

            Debug.Log("[GameSceneSetup] Scene setup complete! Press Play to start the game.");
            EditorUtility.DisplayDialog("Setup Complete",
                "GameScene has been set up successfully!\n\n" +
                "Press Play to start the game. The GameSetupManager will automatically " +
                "create the board, player pieces, and dice at runtime.",
                "OK");
        }

        private static void CreateGameSetupManager()
        {
            var setupObj = GameObject.Find("GameSetupManager");
            if (setupObj == null)
            {
                setupObj = new GameObject("GameSetupManager");
                Undo.RegisterCreatedObjectUndo(setupObj, "Create GameSetupManager");
            }

            var setupManager = setupObj.GetComponent<GameSetupManager>();
            if (setupManager == null)
            {
                setupManager = setupObj.AddComponent<GameSetupManager>();
            }

            Debug.Log("[GameSceneSetup] GameSetupManager created");
        }

        private static void CreateGameUI()
        {
            // Create Canvas
            var canvasObj = GameObject.Find("GameCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("GameCanvas");
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Game Canvas");
            }

            var canvas = canvasObj.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var canvasScaler = canvasObj.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = canvasObj.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
            }

            var graphicRaycaster = canvasObj.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create EventSystem if it doesn't exist
            if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create UI elements
            CreateTurnIndicator(canvasObj.transform);
            CreateDiceButton(canvasObj.transform);
            CreateDiceResultText(canvasObj.transform);
            CreateGameOverPanel(canvasObj.transform);

            // Add GameUIManager
            var uiManager = canvasObj.GetComponent<GameUIManager>();
            if (uiManager == null)
            {
                uiManager = canvasObj.AddComponent<GameUIManager>();
            }

            // Wire up references
            WireUIReferences(canvasObj);

            Debug.Log("[GameSceneSetup] Game UI created");
        }

        private static void CreateTurnIndicator(Transform parent)
        {
            var turnIndicatorObj = parent.Find("TurnIndicator");
            if (turnIndicatorObj == null)
            {
                turnIndicatorObj = new GameObject("TurnIndicator").transform;
                turnIndicatorObj.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(turnIndicatorObj.gameObject, "Create Turn Indicator");
            }

            var rectTransform = turnIndicatorObj.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = turnIndicatorObj.gameObject.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0, -20);
            rectTransform.sizeDelta = new Vector2(400, 60);

            var text = turnIndicatorObj.GetComponent<TextMeshProUGUI>();
            if (text == null)
                text = turnIndicatorObj.gameObject.AddComponent<TextMeshProUGUI>();

            text.text = "Player 1's Turn";
            text.fontSize = 36;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }

        private static void CreateDiceButton(Transform parent)
        {
            var buttonObj = parent.Find("RollDiceButton");
            if (buttonObj == null)
            {
                buttonObj = new GameObject("RollDiceButton").transform;
                buttonObj.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(buttonObj.gameObject, "Create Dice Button");
            }

            var rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = buttonObj.gameObject.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0, 40);
            rectTransform.sizeDelta = new Vector2(200, 60);

            var image = buttonObj.GetComponent<Image>();
            if (image == null)
                image = buttonObj.gameObject.AddComponent<Image>();

            image.color = new Color(0.2f, 0.6f, 1f);

            var button = buttonObj.GetComponent<Button>();
            if (button == null)
                button = buttonObj.gameObject.AddComponent<Button>();

            // Create button text
            var textObj = buttonObj.Find("Text");
            if (textObj == null)
            {
                textObj = new GameObject("Text").transform;
                textObj.SetParent(buttonObj);
            }

            var textRect = textObj.GetComponent<RectTransform>();
            if (textRect == null)
                textRect = textObj.gameObject.AddComponent<RectTransform>();

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var buttonText = textObj.GetComponent<TextMeshProUGUI>();
            if (buttonText == null)
                buttonText = textObj.gameObject.AddComponent<TextMeshProUGUI>();

            buttonText.text = "Roll Dice";
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
        }

        private static void CreateDiceResultText(Transform parent)
        {
            var resultObj = parent.Find("DiceResultText");
            if (resultObj == null)
            {
                resultObj = new GameObject("DiceResultText").transform;
                resultObj.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(resultObj.gameObject, "Create Dice Result Text");
            }

            var rectTransform = resultObj.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = resultObj.gameObject.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0, 120);
            rectTransform.sizeDelta = new Vector2(200, 40);

            var text = resultObj.GetComponent<TextMeshProUGUI>();
            if (text == null)
                text = resultObj.gameObject.AddComponent<TextMeshProUGUI>();

            text.text = "Rolled: -";
            text.fontSize = 28;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.yellow;
        }

        private static void CreateGameOverPanel(Transform parent)
        {
            var panelObj = parent.Find("GameOverPanel");
            if (panelObj == null)
            {
                panelObj = new GameObject("GameOverPanel").transform;
                panelObj.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(panelObj.gameObject, "Create Game Over Panel");
            }

            var rectTransform = panelObj.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = panelObj.gameObject.AddComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            var image = panelObj.GetComponent<Image>();
            if (image == null)
                image = panelObj.gameObject.AddComponent<Image>();

            image.color = new Color(0, 0, 0, 0.8f);

            // Winner text
            var winnerTextObj = panelObj.Find("WinnerText");
            if (winnerTextObj == null)
            {
                winnerTextObj = new GameObject("WinnerText").transform;
                winnerTextObj.SetParent(panelObj);
            }

            var winnerRect = winnerTextObj.GetComponent<RectTransform>();
            if (winnerRect == null)
                winnerRect = winnerTextObj.gameObject.AddComponent<RectTransform>();

            winnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            winnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            winnerRect.pivot = new Vector2(0.5f, 0.5f);
            winnerRect.anchoredPosition = new Vector2(0, 100);
            winnerRect.sizeDelta = new Vector2(600, 100);

            var winnerText = winnerTextObj.GetComponent<TextMeshProUGUI>();
            if (winnerText == null)
                winnerText = winnerTextObj.gameObject.AddComponent<TextMeshProUGUI>();

            winnerText.text = "Player 1 Wins!";
            winnerText.fontSize = 48;
            winnerText.alignment = TextAlignmentOptions.Center;
            winnerText.color = Color.yellow;

            // Play Again button
            CreateButton(panelObj, "PlayAgainButton", "Play Again", new Vector2(0, -50), new Vector2(200, 60));

            // Main Menu button
            CreateButton(panelObj, "MainMenuButton", "Main Menu", new Vector2(0, -130), new Vector2(200, 60));

            panelObj.gameObject.SetActive(false);
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            var buttonObj = parent.Find(name);
            if (buttonObj == null)
            {
                buttonObj = new GameObject(name).transform;
                buttonObj.SetParent(parent);
            }

            var rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = buttonObj.gameObject.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            var image = buttonObj.GetComponent<Image>();
            if (image == null)
                image = buttonObj.gameObject.AddComponent<Image>();

            image.color = new Color(0.2f, 0.6f, 1f);

            var button = buttonObj.GetComponent<Button>();
            if (button == null)
                button = buttonObj.gameObject.AddComponent<Button>();

            // Create button text
            var textObj = buttonObj.Find("Text");
            if (textObj == null)
            {
                textObj = new GameObject("Text").transform;
                textObj.SetParent(buttonObj);
            }

            var textRect = textObj.GetComponent<RectTransform>();
            if (textRect == null)
                textRect = textObj.gameObject.AddComponent<RectTransform>();

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var buttonText = textObj.GetComponent<TextMeshProUGUI>();
            if (buttonText == null)
                buttonText = textObj.gameObject.AddComponent<TextMeshProUGUI>();

            buttonText.text = text;
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
        }

        private static void WireUIReferences(GameObject canvasObj)
        {
            var uiManager = canvasObj.GetComponent<GameUIManager>();
            if (uiManager == null)
                return;

            var canvas = canvasObj.transform;

            // Use SerializedObject to set private fields
            SerializedObject so = new SerializedObject(uiManager);

            // Set button references
            var rollDiceButton = canvas.Find("RollDiceButton")?.GetComponent<Button>();
            if (rollDiceButton != null)
                so.FindProperty("rollDiceButton").objectReferenceValue = rollDiceButton;

            // Set text references
            var turnIndicatorText = canvas.Find("TurnIndicator")?.GetComponent<TextMeshProUGUI>();
            if (turnIndicatorText != null)
                so.FindProperty("turnIndicatorText").objectReferenceValue = turnIndicatorText;

            var diceResultText = canvas.Find("DiceResultText")?.GetComponent<TextMeshProUGUI>();
            if (diceResultText != null)
                so.FindProperty("diceResultText").objectReferenceValue = diceResultText;

            // Set game over panel references
            var gameOverPanel = canvas.Find("GameOverPanel")?.gameObject;
            if (gameOverPanel != null)
            {
                so.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;

                var winnerText = gameOverPanel.transform.Find("WinnerText")?.GetComponent<TextMeshProUGUI>();
                if (winnerText != null)
                    so.FindProperty("winnerText").objectReferenceValue = winnerText;

                var playAgainButton = gameOverPanel.transform.Find("PlayAgainButton")?.GetComponent<Button>();
                if (playAgainButton != null)
                    so.FindProperty("playAgainButton").objectReferenceValue = playAgainButton;

                var mainMenuButton = gameOverPanel.transform.Find("MainMenuButton")?.GetComponent<Button>();
                if (mainMenuButton != null)
                    so.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
            }

            // Find and set DiceModel reference
            var diceModel = GameObject.FindObjectOfType<DiceModel>();
            if (diceModel != null)
                so.FindProperty("diceModel").objectReferenceValue = diceModel;

            // Find and set GameController reference
            var gameController = GameObject.FindObjectOfType<GameController>();
            if (gameController != null)
                so.FindProperty("gameController").objectReferenceValue = gameController;

            so.ApplyModifiedProperties();

            Debug.Log("[GameSceneSetup] UI references wired");
        }

        private static void CreateLighting()
        {
            // Create directional light if it doesn't exist
            var lights = GameObject.FindObjectsOfType<Light>();
            bool hasDirectionalLight = false;

            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    hasDirectionalLight = true;
                    break;
                }
            }

            if (!hasDirectionalLight)
            {
                var lightObj = new GameObject("Directional Light");
                Undo.RegisterCreatedObjectUndo(lightObj, "Create Directional Light");

                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = Color.white;
                light.intensity = 1f;

                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

                Debug.Log("[GameSceneSetup] Directional light created");
            }
        }
    }
}
#endif
