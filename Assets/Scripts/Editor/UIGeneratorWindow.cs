using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LaddersAndSnakes.Core;

namespace LaddersAndSnakes.Editor
{
    /// <summary>
    /// Editor window for generating UI elements
    /// Access via Window > Ladders & Snakes > UI Generator
    /// </summary>
    public class UIGeneratorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private MonoBehaviour selectedComponent;
        private Transform targetParent;
        private bool autoSelectCreated = true;
        private bool createInCanvas = true;

        private Vector2 customPosition = Vector2.zero;
        private bool useCustomPosition = false;

        [MenuItem("Window/Ladders & Snakes/UI Generator")]
        public static void ShowWindow()
        {
            UIGeneratorWindow window = GetWindow<UIGeneratorWindow>();
            window.titleContent = new GUIContent("UI Generator");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            DrawHeader();
            DrawSettings();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawQuickCreate();
            EditorGUILayout.Space(10);
            DrawAutoGenerate();
            EditorGUILayout.Space(10);
            DrawTemplates();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("UI Element Generator", headerStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Create UI elements quickly:\n" +
                "• Quick Create: Click buttons to create individual elements\n" +
                "• Auto-Generate: Generate UI from [UIReference] attributes\n" +
                "• Templates: Create complete UI layouts from templates",
                MessageType.Info);

            EditorGUILayout.Space(10);
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            targetParent = (Transform)EditorGUILayout.ObjectField("Parent Transform", targetParent, typeof(Transform), true);

            createInCanvas = EditorGUILayout.Toggle("Create in Canvas", createInCanvas);
            autoSelectCreated = EditorGUILayout.Toggle("Auto-Select Created", autoSelectCreated);

            EditorGUILayout.BeginHorizontal();
            useCustomPosition = EditorGUILayout.Toggle("Custom Position", useCustomPosition);
            if (useCustomPosition)
            {
                customPosition = EditorGUILayout.Vector2Field("", customPosition);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawQuickCreate()
        {
            EditorGUILayout.LabelField("Quick Create UI Elements", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Row 1: Basic Elements
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Button", GUILayout.Height(30)))
                CreateElement(() => UIElementGenerator.CreateButton("NewButton", GetParent(), GetPosition()));

            if (GUILayout.Button("Text", GUILayout.Height(30)))
                CreateElement(() => UIElementGenerator.CreateText("NewText", GetParent(), GetPosition()));

            if (GUILayout.Button("Panel", GUILayout.Height(30)))
                CreateElement(() => UIElementGenerator.CreatePanel("NewPanel", GetParent(), GetPosition()));
            EditorGUILayout.EndHorizontal();

            // Row 2: Interactive Elements
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Image", GUILayout.Height(30)))
                CreateElement(() => UIElementGenerator.CreateImage("NewImage", GetParent(), GetPosition()));

            if (GUILayout.Button("Slider", GUILayout.Height(30)))
                CreateElement(() => UIElementGenerator.CreateSlider("NewSlider", GetParent(), GetPosition()));

            if (GUILayout.Button("Toggle", GUILayout.Height(30)))
                CreateElement(() => UIElementGenerator.CreateToggle("NewToggle", GetParent(), GetPosition()));
            EditorGUILayout.EndHorizontal();

            // Row 3: Input Elements
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Input Field", GUILayout.Height(30)))
                CreateElement(() => UIElementGenerator.CreateInputField("NewInputField", GetParent(), GetPosition()));

            if (GUILayout.Button("Dropdown", GUILayout.Height(30)))
                CreateElement(() => UIElementGenerator.CreateDropdown("NewDropdown", GetParent(), GetPosition()));

            if (GUILayout.Button("Canvas", GUILayout.Height(30)))
                CreateCanvas();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawAutoGenerate()
        {
            EditorGUILayout.LabelField("Auto-Generate from Component", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.HelpBox(
                "Select a MonoBehaviour with [UIReference] attributes, and this tool will automatically create all the UI elements for you!",
                MessageType.Info);

            selectedComponent = (MonoBehaviour)EditorGUILayout.ObjectField("Component", selectedComponent, typeof(MonoBehaviour), true);

            if (selectedComponent != null)
            {
                // Show preview of what will be created
                var fields = selectedComponent.GetType().GetFields(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                int count = 0;
                foreach (var field in fields)
                {
                    var attrs = field.GetCustomAttributes(typeof(UIReferenceAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                        count++;
                }

                if (count > 0)
                {
                    EditorGUILayout.LabelField($"Will create {count} UI element(s)", EditorStyles.miniLabel);

                    if (GUILayout.Button($"Generate {count} UI Elements", GUILayout.Height(40)))
                    {
                        GenerateFromComponent();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Selected component has no [UIReference] attributes.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a component to auto-generate UI elements.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTemplates()
        {
            EditorGUILayout.LabelField("UI Templates", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Common UI Patterns", EditorStyles.miniLabel);

            // Game UI Templates
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause Menu", GUILayout.Height(30)))
                CreatePauseMenuTemplate();

            if (GUILayout.Button("Game HUD", GUILayout.Height(30)))
                CreateGameHUDTemplate();

            if (GUILayout.Button("Dialog Box", GUILayout.Height(30)))
                CreateDialogBoxTemplate();
            EditorGUILayout.EndHorizontal();

            // Menu Templates
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Main Menu", GUILayout.Height(30)))
                CreateMainMenuTemplate();

            if (GUILayout.Button("Settings Panel", GUILayout.Height(30)))
                CreateSettingsPanelTemplate();

            if (GUILayout.Button("Game Over Panel", GUILayout.Height(30)))
                CreateGameOverPanelTemplate();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        #region Helper Methods

        private Transform GetParent()
        {
            if (targetParent != null)
                return targetParent;

            if (createInCanvas)
                return UIElementGenerator.GetOrCreateCanvas().transform;

            return null;
        }

        private Vector2? GetPosition()
        {
            return useCustomPosition ? (Vector2?)customPosition : null;
        }

        private void CreateElement(System.Func<GameObject> createFunc)
        {
            GameObject created = createFunc();
            if (created != null)
            {
                if (autoSelectCreated)
                    Selection.activeGameObject = created;

                EditorUtility.SetDirty(created);
                Debug.Log($"Created UI element: {created.name}");
            }
        }

        private void CreateCanvas()
        {
            GameObject canvas = UIElementGenerator.GetOrCreateCanvas();
            if (autoSelectCreated)
                Selection.activeGameObject = canvas;
            Debug.Log("Created Canvas");
        }

        private void GenerateFromComponent()
        {
            if (selectedComponent == null)
            {
                EditorUtility.DisplayDialog("No Component", "Please select a component first.", "OK");
                return;
            }

            List<GameObject> created = UIElementGenerator.GenerateUIFromComponent(selectedComponent, GetParent());

            if (created.Count > 0)
            {
                if (autoSelectCreated)
                    Selection.objects = created.ToArray();

                EditorUtility.DisplayDialog("Generation Complete",
                    $"Successfully created {created.Count} UI element(s)!", "OK");

                // Optionally bind references immediately
                if (EditorUtility.DisplayDialog("Bind References?",
                    "Would you like to bind the references now?", "Yes", "No"))
                {
                    UIReferenceBinder.BindUIReferences(selectedComponent);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("No Elements Created",
                    "No UI elements were created. Make sure the component has [UIReference] attributes.", "OK");
            }
        }

        #endregion

        #region Templates

        private void CreatePauseMenuTemplate()
        {
            Transform parent = GetParent();
            GameObject panel = UIElementGenerator.CreatePanel("PauseMenuPanel", parent, Vector2.zero, new Vector2(300, 400));

            GameObject title = UIElementGenerator.CreateText("Title", panel.transform, new Vector2(0, 150));
            title.GetComponent<TMPro.TextMeshProUGUI>().text = "PAUSED";
            title.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 36;

            UIElementGenerator.CreateButton("ResumeButton", panel.transform, new Vector2(0, 50));
            UIElementGenerator.CreateButton("SettingsButton", panel.transform, new Vector2(0, 0));
            UIElementGenerator.CreateButton("MainMenuButton", panel.transform, new Vector2(0, -50));

            if (autoSelectCreated)
                Selection.activeGameObject = panel;

            Debug.Log("Created Pause Menu template");
        }

        private void CreateGameHUDTemplate()
        {
            Transform parent = GetParent();
            GameObject hud = UIElementGenerator.CreatePanel("GameHUD", parent, Vector2.zero, new Vector2(1920, 1080));
            hud.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0); // Transparent

            // Top UI
            GameObject scoreText = UIElementGenerator.CreateText("ScoreText", hud.transform, new Vector2(-800, 500));
            scoreText.GetComponent<TMPro.TextMeshProUGUI>().text = "Score: 0";

            GameObject timerText = UIElementGenerator.CreateText("TimerText", hud.transform, new Vector2(800, 500));
            timerText.GetComponent<TMPro.TextMeshProUGUI>().text = "Time: 0:00";

            // Bottom UI
            UIElementGenerator.CreateButton("PauseButton", hud.transform, new Vector2(800, -500));

            if (autoSelectCreated)
                Selection.activeGameObject = hud;

            Debug.Log("Created Game HUD template");
        }

        private void CreateDialogBoxTemplate()
        {
            Transform parent = GetParent();
            GameObject panel = UIElementGenerator.CreatePanel("DialogBox", parent, Vector2.zero, new Vector2(500, 200));

            GameObject title = UIElementGenerator.CreateText("TitleText", panel.transform, new Vector2(0, 60));
            title.GetComponent<TMPro.TextMeshProUGUI>().text = "Dialog Title";
            title.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 28;

            GameObject message = UIElementGenerator.CreateText("MessageText", panel.transform, new Vector2(0, 0));
            message.GetComponent<TMPro.TextMeshProUGUI>().text = "Dialog message goes here...";
            message.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 18;

            UIElementGenerator.CreateButton("OkButton", panel.transform, new Vector2(-60, -60));
            UIElementGenerator.CreateButton("CancelButton", panel.transform, new Vector2(60, -60));

            if (autoSelectCreated)
                Selection.activeGameObject = panel;

            Debug.Log("Created Dialog Box template");
        }

        private void CreateMainMenuTemplate()
        {
            Transform parent = GetParent();
            GameObject panel = UIElementGenerator.CreatePanel("MainMenuPanel", parent, Vector2.zero, new Vector2(400, 500));

            GameObject title = UIElementGenerator.CreateText("GameTitle", panel.transform, new Vector2(0, 180));
            title.GetComponent<TMPro.TextMeshProUGUI>().text = "GAME TITLE";
            title.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 48;

            UIElementGenerator.CreateButton("PlayButton", panel.transform, new Vector2(0, 60));
            UIElementGenerator.CreateButton("SettingsButton", panel.transform, new Vector2(0, 10));
            UIElementGenerator.CreateButton("CreditsButton", panel.transform, new Vector2(0, -40));
            UIElementGenerator.CreateButton("QuitButton", panel.transform, new Vector2(0, -90));

            if (autoSelectCreated)
                Selection.activeGameObject = panel;

            Debug.Log("Created Main Menu template");
        }

        private void CreateSettingsPanelTemplate()
        {
            Transform parent = GetParent();
            GameObject panel = UIElementGenerator.CreatePanel("SettingsPanel", parent, Vector2.zero, new Vector2(400, 450));

            GameObject title = UIElementGenerator.CreateText("Title", panel.transform, new Vector2(0, 180));
            title.GetComponent<TMPro.TextMeshProUGUI>().text = "SETTINGS";
            title.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 36;

            // Volume
            GameObject volumeLabel = UIElementGenerator.CreateText("VolumeLabel", panel.transform, new Vector2(-100, 100));
            volumeLabel.GetComponent<TMPro.TextMeshProUGUI>().text = "Volume";
            volumeLabel.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
            UIElementGenerator.CreateSlider("VolumeSlider", panel.transform, new Vector2(50, 100));

            // Music Toggle
            UIElementGenerator.CreateToggle("MusicToggle", panel.transform, new Vector2(0, 50));

            // SFX Toggle
            UIElementGenerator.CreateToggle("SFXToggle", panel.transform, new Vector2(0, 10));

            // Quality Dropdown
            GameObject qualityLabel = UIElementGenerator.CreateText("QualityLabel", panel.transform, new Vector2(-100, -40));
            qualityLabel.GetComponent<TMPro.TextMeshProUGUI>().text = "Quality";
            qualityLabel.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
            UIElementGenerator.CreateDropdown("QualityDropdown", panel.transform, new Vector2(50, -40));

            // Back Button
            UIElementGenerator.CreateButton("BackButton", panel.transform, new Vector2(0, -150));

            if (autoSelectCreated)
                Selection.activeGameObject = panel;

            Debug.Log("Created Settings Panel template");
        }

        private void CreateGameOverPanelTemplate()
        {
            Transform parent = GetParent();
            GameObject panel = UIElementGenerator.CreatePanel("GameOverPanel", parent, Vector2.zero, new Vector2(400, 350));

            GameObject title = UIElementGenerator.CreateText("GameOverText", panel.transform, new Vector2(0, 120));
            title.GetComponent<TMPro.TextMeshProUGUI>().text = "GAME OVER";
            title.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 48;

            GameObject score = UIElementGenerator.CreateText("FinalScoreText", panel.transform, new Vector2(0, 60));
            score.GetComponent<TMPro.TextMeshProUGUI>().text = "Final Score: 0";
            score.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 24;

            UIElementGenerator.CreateButton("PlayAgainButton", panel.transform, new Vector2(0, 0));
            UIElementGenerator.CreateButton("MainMenuButton", panel.transform, new Vector2(0, -50));

            if (autoSelectCreated)
                Selection.activeGameObject = panel;

            Debug.Log("Created Game Over Panel template");
        }

        #endregion
    }
}
