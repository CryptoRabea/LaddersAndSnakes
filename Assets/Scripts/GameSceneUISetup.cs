using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper script to set up game scene UI elements programmatically
/// This creates the main menu button, settings button, and settings panel
/// </summary>
public class GameSceneUISetup : MonoBehaviour
{
    [Header("Auto-Create UI")]
    [SerializeField] private bool autoCreateUI = true;

    [Header("References (will be auto-assigned if null)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button settingsButton;

    [Header("UI Styling")]
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.7f, 0.2f, 1f);
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

    private SettingsPanelController panelController;

    void Awake()
    {
        if (autoCreateUI)
        {
            SetupUI();
        }
    }

    void SetupUI()
    {
        // Find or create canvas
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[GameSceneUISetup] No Canvas found in scene!");
                return;
            }
        }

        // Create main menu button (top-left)
        if (mainMenuButton == null)
        {
            mainMenuButton = CreateButton("MainMenuButton", "Main Menu",
                new Vector2(10, -10), new Vector2(150, 50), TextAnchor.UpperLeft);
        }

        // Create settings button (top-right)
        if (settingsButton == null)
        {
            settingsButton = CreateButton("SettingsButton", "Settings",
                new Vector2(-10, -10), new Vector2(150, 50), TextAnchor.UpperRight);
        }

        // Create settings panel
        if (settingsPanel == null)
        {
            settingsPanel = CreateSettingsPanel();
        }

        // Add or get SettingsPanelController
        panelController = GetComponent<SettingsPanelController>();
        if (panelController == null)
        {
            panelController = gameObject.AddComponent<SettingsPanelController>();
        }

        // Assign references to the controller using reflection since fields are serialized
        AssignReferencesToController();

        Debug.Log("[GameSceneUISetup] UI setup complete");
    }

    Button CreateButton(string name, string text, Vector2 anchoredPosition, Vector2 sizeDelta, TextAnchor anchor)
    {
        // Create button GameObject
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(canvas.transform, false);

        // Add RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.anchoredPosition = anchoredPosition;

        // Set anchors based on position
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                break;
            case TextAnchor.UpperRight:
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 1);
                break;
        }

        // Add Button component
        Button button = buttonObj.AddComponent<Button>();

        // Add Image for button background
        Image image = buttonObj.AddComponent<Image>();
        image.color = buttonColor;

        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 18;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;

        return button;
    }

    GameObject CreateSettingsPanel()
    {
        // Create panel GameObject
        GameObject panel = new GameObject("SettingsPanel");
        panel.transform.SetParent(canvas.transform, false);

        // RectTransform - centered, covers most of screen
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 300);

        // Panel background
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = panelColor;

        // Panel border
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, 2);

        // Title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 50);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Settings";
        titleText.fontSize = 28;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Surrender button (center)
        GameObject surrenderBtnObj = new GameObject("SurrenderButton");
        surrenderBtnObj.transform.SetParent(panel.transform, false);

        RectTransform surrenderRect = surrenderBtnObj.AddComponent<RectTransform>();
        surrenderRect.anchorMin = new Vector2(0.5f, 0.5f);
        surrenderRect.anchorMax = new Vector2(0.5f, 0.5f);
        surrenderRect.pivot = new Vector2(0.5f, 0.5f);
        surrenderRect.anchoredPosition = Vector2.zero;
        surrenderRect.sizeDelta = new Vector2(200, 60);

        Button surrenderBtn = surrenderBtnObj.AddComponent<Button>();
        Image surrenderImage = surrenderBtnObj.AddComponent<Image>();
        surrenderImage.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red color for surrender

        GameObject surrenderTextObj = new GameObject("Text");
        surrenderTextObj.transform.SetParent(surrenderBtnObj.transform, false);

        RectTransform surrenderTextRect = surrenderTextObj.AddComponent<RectTransform>();
        surrenderTextRect.anchorMin = Vector2.zero;
        surrenderTextRect.anchorMax = Vector2.one;
        surrenderTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI surrenderText = surrenderTextObj.AddComponent<TextMeshProUGUI>();
        surrenderText.text = "Surrender";
        surrenderText.fontSize = 22;
        surrenderText.alignment = TextAlignmentOptions.Center;
        surrenderText.color = Color.white;

        // Close button (bottom)
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(panel.transform, false);

        RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0);
        closeRect.anchorMax = new Vector2(0.5f, 0);
        closeRect.pivot = new Vector2(0.5f, 0);
        closeRect.anchoredPosition = new Vector2(0, 10);
        closeRect.sizeDelta = new Vector2(150, 50);

        Button closeBtn = closeBtnObj.AddComponent<Button>();
        Image closeImage = closeBtnObj.AddComponent<Image>();
        closeImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);

        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "Close";
        closeText.fontSize = 18;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;

        // Store references for later assignment
        panel.SetActive(false); // Start hidden

        return panel;
    }

    void AssignReferencesToController()
    {
        if (panelController == null) return;

        // Use reflection to set private serialized fields
        var type = typeof(SettingsPanelController);

        var settingsPanelField = type.GetField("settingsPanel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (settingsPanelField != null)
            settingsPanelField.SetValue(panelController, settingsPanel);

        var settingsButtonField = type.GetField("settingsButton",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (settingsButtonField != null)
            settingsButtonField.SetValue(panelController, settingsButton);

        var mainMenuButtonField = type.GetField("mainMenuButton",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (mainMenuButtonField != null)
            mainMenuButtonField.SetValue(panelController, mainMenuButton);

        // Find buttons in panel
        if (settingsPanel != null)
        {
            Button[] buttons = settingsPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                if (btn.name == "SurrenderButton")
                {
                    var surrenderButtonField = type.GetField("surrenderButton",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (surrenderButtonField != null)
                        surrenderButtonField.SetValue(panelController, btn);
                }
                else if (btn.name == "CloseButton")
                {
                    var closePanelButtonField = type.GetField("closePanelButton",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (closePanelButtonField != null)
                        closePanelButtonField.SetValue(panelController, btn);
                }
            }
        }

        Debug.Log("[GameSceneUISetup] References assigned to SettingsPanelController");
    }

#if UNITY_EDITOR
    [ContextMenu("Setup UI")]
    public void SetupUIFromEditor()
    {
        SetupUI();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
