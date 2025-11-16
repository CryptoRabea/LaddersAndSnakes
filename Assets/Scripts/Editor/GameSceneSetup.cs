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
            GameObject turnIndicatorGO = null;
            var turnIndicatorObj = parent.Find("TurnIndicator");
            if (turnIndicatorObj == null)
            {
                turnIndicatorGO = new GameObject("TurnIndicator");
                turnIndicatorGO.transform.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(turnIndicatorGO, "Create Turn Indicator");
            }
            else
            {
                turnIndicatorGO = turnIndicatorObj.gameObject;
            }

            var rectTransform = turnIndicatorGO.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = turnIndicatorGO.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0, -20);
            rectTransform.sizeDelta = new Vector2(400, 60);

            var text = turnIndicatorGO.GetComponent<TextMeshProUGUI>();
            if (text == null)
                text = turnIndicatorGO.AddComponent<TextMeshProUGUI>();

            text.text = "Player 1's Turn";
            text.fontSize = 36;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }

        private static void CreateDiceButton(Transform parent)
        {
            GameObject buttonGO = null;
            var buttonObj = parent.Find("RollDiceButton");
            if (buttonObj == null)
            {
                buttonGO = new GameObject("RollDiceButton");
                buttonGO.transform.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(buttonGO, "Create Dice Button");
            }
            else
            {
                buttonGO = buttonObj.gameObject;
            }

            var rectTransform = buttonGO.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = buttonGO.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0, 40);
            rectTransform.sizeDelta = new Vector2(200, 60);

            var image = buttonGO.GetComponent<Image>();
            if (image == null)
                image = buttonGO.AddComponent<Image>();

            image.color = new Color(0.2f, 0.6f, 1f);

            var button = buttonGO.GetComponent<Button>();
            if (button == null)
                button = buttonGO.AddComponent<Button>();

            // Create button text
            GameObject textGO = null;
            var textObj = buttonGO.transform.Find("Text");
            if (textObj == null)
            {
                textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform);
            }
            else
            {
                textGO = textObj.gameObject;
            }

            var textRect = textGO.GetComponent<RectTransform>();
            if (textRect == null)
                textRect = textGO.AddComponent<RectTransform>();

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var buttonText = textGO.GetComponent<TextMeshProUGUI>();
            if (buttonText == null)
                buttonText = textGO.AddComponent<TextMeshProUGUI>();

            buttonText.text = "Roll Dice";
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
        }

        private static void CreateDiceResultText(Transform parent)
        {
            GameObject resultGO = null;
            var resultObj = parent.Find("DiceResultText");
            if (resultObj == null)
            {
                resultGO = new GameObject("DiceResultText");
                resultGO.transform.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(resultGO, "Create Dice Result Text");
            }
            else
            {
                resultGO = resultObj.gameObject;
            }

            var rectTransform = resultGO.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = resultGO.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0, 120);
            rectTransform.sizeDelta = new Vector2(200, 40);

            var text = resultGO.GetComponent<TextMeshProUGUI>();
            if (text == null)
                text = resultGO.AddComponent<TextMeshProUGUI>();

            text.text = "Rolled: -";
            text.fontSize = 28;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.yellow;
        }

        private static void CreateGameOverPanel(Transform parent)
        {
            GameObject panelGO = null;
            var panelObj = parent.Find("GameOverPanel");
            if (panelObj == null)
            {
                panelGO = new GameObject("GameOverPanel");
                panelGO.transform.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(panelGO, "Create Game Over Panel");
            }
            else
            {
                panelGO = panelObj.gameObject;
            }

            var rectTransform = panelGO.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = panelGO.AddComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            var image = panelGO.GetComponent<Image>();
            if (image == null)
                image = panelGO.AddComponent<Image>();

            image.color = new Color(0, 0, 0, 0.8f);

            // Winner text
            GameObject winnerTextGO = null;
            var winnerTextObj = panelGO.transform.Find("WinnerText");
            if (winnerTextObj == null)
            {
                winnerTextGO = new GameObject("WinnerText");
                winnerTextGO.transform.SetParent(panelGO.transform);
            }
            else
            {
                winnerTextGO = winnerTextObj.gameObject;
            }

            var winnerRect = winnerTextGO.GetComponent<RectTransform>();
            if (winnerRect == null)
                winnerRect = winnerTextGO.AddComponent<RectTransform>();

            winnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            winnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            winnerRect.pivot = new Vector2(0.5f, 0.5f);
            winnerRect.anchoredPosition = new Vector2(0, 100);
            winnerRect.sizeDelta = new Vector2(600, 100);

            var winnerText = winnerTextGO.GetComponent<TextMeshProUGUI>();
            if (winnerText == null)
                winnerText = winnerTextGO.AddComponent<TextMeshProUGUI>();

            winnerText.text = "Player 1 Wins!";
            winnerText.fontSize = 48;
            winnerText.alignment = TextAlignmentOptions.Center;
            winnerText.color = Color.yellow;

            // Play Again button
            CreateButton(panelGO.transform, "PlayAgainButton", "Play Again", new Vector2(0, -50), new Vector2(200, 60));

            // Main Menu button
            CreateButton(panelGO.transform, "MainMenuButton", "Main Menu", new Vector2(0, -130), new Vector2(200, 60));

            panelGO.SetActive(false);
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            GameObject buttonGO = null;
            var buttonObj = parent.Find(name);
            if (buttonObj == null)
            {
                buttonGO = new GameObject(name);
                buttonGO.transform.SetParent(parent);
            }
            else
            {
                buttonGO = buttonObj.gameObject;
            }

            var rectTransform = buttonGO.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = buttonGO.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            var image = buttonGO.GetComponent<Image>();
            if (image == null)
                image = buttonGO.AddComponent<Image>();

            image.color = new Color(0.2f, 0.6f, 1f);

            var button = buttonGO.GetComponent<Button>();
            if (button == null)
                button = buttonGO.AddComponent<Button>();

            // Create button text
            GameObject textGO = null;
            var textObj = buttonGO.transform.Find("Text");
            if (textObj == null)
            {
                textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform);
            }
            else
            {
                textGO = textObj.gameObject;
            }

            var textRect = textGO.GetComponent<RectTransform>();
            if (textRect == null)
                textRect = textGO.AddComponent<RectTransform>();

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var buttonText = textGO.GetComponent<TextMeshProUGUI>();
            if (buttonText == null)
                buttonText = textGO.AddComponent<TextMeshProUGUI>();

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
