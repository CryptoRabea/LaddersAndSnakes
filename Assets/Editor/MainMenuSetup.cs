using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

/// <summary>
/// One-time setup script to create the complete Main Menu UI hierarchy
/// Run this from: Tools > Setup Main Menu UI (One Time)
/// </summary>
public class MainMenuSetup : EditorWindow
{
    [MenuItem("Tools/Setup Main Menu UI (One Time)")]
    public static void SetupMainMenuUI()
    {
        if (!EditorUtility.DisplayDialog("Setup Main Menu UI",
            "This will create the complete Main Menu UI hierarchy.\n\n" +
            "Make sure you have the MainMenu scene open.\n\n" +
            "Continue?", "Yes", "Cancel"))
        {
            return;
        }

        // Create Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Create EventSystem if it doesn't exist
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // Create MainMenuController GameObject
        GameObject controllerGO = new GameObject("MainMenuController");
        MainMenuController controller = controllerGO.AddComponent<MainMenuController>();

        // Create Main Panel
        GameObject mainPanel = CreatePanel("MainPanel", canvasGO.transform);
        SetPanelColor(mainPanel, new Color(0.1f, 0.1f, 0.15f, 0.95f));

        // Create title for Main Panel
        GameObject mainTitle = CreateText("Title", mainPanel.transform, "LADDERS & SNAKES", 60);
        RectTransform mainTitleRT = mainTitle.GetComponent<RectTransform>();
        mainTitleRT.anchorMin = new Vector2(0.5f, 0.8f);
        mainTitleRT.anchorMax = new Vector2(0.5f, 0.8f);
        mainTitleRT.anchoredPosition = Vector2.zero;

        // Create Main Menu Buttons
        Button playLocalBtn = CreateButton("PlayLocalButton", mainPanel.transform, "Play Local", new Vector2(0, 50));
        Button playAIBtn = CreateButton("PlayAIButton", mainPanel.transform, "Play vs AI", new Vector2(0, -20));
        Button playOnlineBtn = CreateButton("PlayOnlineButton", mainPanel.transform, "Play Online", new Vector2(0, -90));
        Button settingsBtn = CreateButton("SettingsButton", mainPanel.transform, "Settings", new Vector2(0, -160));
        Button quitBtn = CreateButton("QuitButton", mainPanel.transform, "Quit", new Vector2(0, -230));

        // Create Multiplayer Panel
        GameObject multiplayerPanel = CreatePanel("MultiplayerPanel", canvasGO.transform);
        SetPanelColor(multiplayerPanel, new Color(0.1f, 0.15f, 0.1f, 0.95f));
        multiplayerPanel.SetActive(false);

        // Create title for Multiplayer Panel
        GameObject mpTitle = CreateText("Title", multiplayerPanel.transform, "MULTIPLAYER SETUP", 50);
        RectTransform mpTitleRT = mpTitle.GetComponent<RectTransform>();
        mpTitleRT.anchorMin = new Vector2(0.5f, 0.8f);
        mpTitleRT.anchorMax = new Vector2(0.5f, 0.8f);
        mpTitleRT.anchoredPosition = Vector2.zero;

        // Player Count Dropdown
        GameObject dropdownLabel = CreateText("PlayerCountLabel", multiplayerPanel.transform, "Number of Players:", 30);
        RectTransform dropdownLabelRT = dropdownLabel.GetComponent<RectTransform>();
        dropdownLabelRT.anchorMin = new Vector2(0.5f, 0.6f);
        dropdownLabelRT.anchorMax = new Vector2(0.5f, 0.6f);
        dropdownLabelRT.anchoredPosition = new Vector2(0, 50);

        TMP_Dropdown playerCountDropdown = CreateDropdown("PlayerCountDropdown", multiplayerPanel.transform, new Vector2(0, 0));

        // Server Address Input
        GameObject serverLabel = CreateText("ServerAddressLabel", multiplayerPanel.transform, "Server Address:", 30);
        RectTransform serverLabelRT = serverLabel.GetComponent<RectTransform>();
        serverLabelRT.anchorMin = new Vector2(0.5f, 0.4f);
        serverLabelRT.anchorMax = new Vector2(0.5f, 0.4f);
        serverLabelRT.anchoredPosition = new Vector2(0, 30);

        TMP_InputField serverAddressInput = CreateInputField("ServerAddressInput", multiplayerPanel.transform, "127.0.0.1", new Vector2(0, -20));

        // Multiplayer Panel Buttons
        Button hostGameBtn = CreateButton("HostGameButton", multiplayerPanel.transform, "Host Game", new Vector2(-120, -120));
        Button joinGameBtn = CreateButton("JoinGameButton", multiplayerPanel.transform, "Join Game", new Vector2(120, -120));
        Button mpBackBtn = CreateButton("BackButton", multiplayerPanel.transform, "Back", new Vector2(0, -200));

        // Create Settings Panel
        GameObject settingsPanel = CreatePanel("SettingsPanel", canvasGO.transform);
        SetPanelColor(settingsPanel, new Color(0.15f, 0.1f, 0.1f, 0.95f));
        settingsPanel.SetActive(false);

        // Create title for Settings Panel
        GameObject settingsTitle = CreateText("Title", settingsPanel.transform, "SETTINGS", 50);
        RectTransform settingsTitleRT = settingsTitle.GetComponent<RectTransform>();
        settingsTitleRT.anchorMin = new Vector2(0.5f, 0.8f);
        settingsTitleRT.anchorMax = new Vector2(0.5f, 0.8f);
        settingsTitleRT.anchoredPosition = Vector2.zero;

        // Settings content placeholder
        GameObject settingsText = CreateText("SettingsInfo", settingsPanel.transform, "Settings panel - Configure game options here", 25);
        RectTransform settingsTextRT = settingsText.GetComponent<RectTransform>();
        settingsTextRT.anchorMin = new Vector2(0.5f, 0.5f);
        settingsTextRT.anchorMax = new Vector2(0.5f, 0.5f);
        settingsTextRT.anchoredPosition = Vector2.zero;

        Button settingsBackBtn = CreateButton("SettingsBackButton", settingsPanel.transform, "Back", new Vector2(0, -200));

        // Wire up controller references using SerializedObject
        SerializedObject serializedController = new SerializedObject(controller);

        serializedController.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        serializedController.FindProperty("multiplayerPanel").objectReferenceValue = multiplayerPanel;
        serializedController.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;

        serializedController.FindProperty("playLocalButton").objectReferenceValue = playLocalBtn;
        serializedController.FindProperty("playAIButton").objectReferenceValue = playAIBtn;
        serializedController.FindProperty("playOnlineButton").objectReferenceValue = playOnlineBtn;
        serializedController.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
        serializedController.FindProperty("quitButton").objectReferenceValue = quitBtn;

        serializedController.FindProperty("hostGameButton").objectReferenceValue = hostGameBtn;
        serializedController.FindProperty("joinGameButton").objectReferenceValue = joinGameBtn;
        serializedController.FindProperty("backButton").objectReferenceValue = mpBackBtn;
        serializedController.FindProperty("serverAddressInput").objectReferenceValue = serverAddressInput;
        serializedController.FindProperty("playerCountDropdown").objectReferenceValue = playerCountDropdown;

        serializedController.FindProperty("settingsBackButton").objectReferenceValue = settingsBackBtn;

        serializedController.ApplyModifiedProperties();

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Setup Complete!",
            "Main Menu UI has been created successfully!\n\n" +
            "All panels, buttons, and components have been set up.\n" +
            "Save the scene to keep the changes.", "OK");

        Debug.Log("[MainMenuSetup] UI setup completed successfully!");
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

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

    private static Button CreateButton(string name, Transform parent, string text, Vector2 position)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);

        RectTransform rt = buttonGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300, 60);
        rt.anchoredPosition = position;

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.3f, 0.4f, 1f);

        Button button = buttonGO.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.3f, 0.4f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.4f, 0.5f, 1f);
        colors.pressedColor = new Color(0.15f, 0.25f, 0.35f, 1f);
        colors.selectedColor = new Color(0.25f, 0.35f, 0.45f, 1f);
        button.colors = colors;

        // Create text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return button;
    }

    private static GameObject CreateText(string name, Transform parent, string text, float fontSize)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 100);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        return textGO;
    }

    private static TMP_InputField CreateInputField(string name, Transform parent, string placeholder, Vector2 position)
    {
        GameObject inputGO = new GameObject(name);
        inputGO.transform.SetParent(parent, false);

        RectTransform rt = inputGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 50);
        rt.anchoredPosition = position;

        Image image = inputGO.AddComponent<Image>();
        image.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();

        // Create Text Area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputGO.transform, false);
        RectTransform textAreaRT = textArea.AddComponent<RectTransform>();
        textAreaRT.anchorMin = Vector2.zero;
        textAreaRT.anchorMax = Vector2.one;
        textAreaRT.offsetMin = new Vector2(10, 5);
        textAreaRT.offsetMax = new Vector2(-10, -5);
        textArea.AddComponent<RectMask2D>();

        // Create Placeholder
        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(textArea.transform, false);
        RectTransform placeholderRT = placeholderGO.AddComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero;
        placeholderRT.anchorMax = Vector2.one;
        placeholderRT.offsetMin = Vector2.zero;
        placeholderRT.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 24;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.alignment = TextAlignmentOptions.Left;

        // Create Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(textArea.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = "";
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Left;

        inputField.textViewport = textAreaRT;
        inputField.textComponent = textComponent;
        inputField.placeholder = placeholderText;

        return inputField;
    }

    private static TMP_Dropdown CreateDropdown(string name, Transform parent, Vector2 position)
    {
        GameObject dropdownGO = new GameObject(name);
        dropdownGO.transform.SetParent(parent, false);

        RectTransform rt = dropdownGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300, 50);
        rt.anchoredPosition = position;

        Image image = dropdownGO.AddComponent<Image>();
        image.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        TMP_Dropdown dropdown = dropdownGO.AddComponent<TMP_Dropdown>();

        // Create Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 0);
        labelRT.anchorMax = new Vector2(1, 1);
        labelRT.offsetMin = new Vector2(10, 5);
        labelRT.offsetMax = new Vector2(-30, -5);

        TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = "2 Players";
        labelText.fontSize = 24;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Left;

        // Create Arrow
        GameObject arrowGO = new GameObject("Arrow");
        arrowGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform arrowRT = arrowGO.AddComponent<RectTransform>();
        arrowRT.anchorMin = new Vector2(1, 0.5f);
        arrowRT.anchorMax = new Vector2(1, 0.5f);
        arrowRT.sizeDelta = new Vector2(20, 20);
        arrowRT.anchoredPosition = new Vector2(-15, 0);

        Image arrowImage = arrowGO.AddComponent<Image>();
        arrowImage.color = Color.white;

        // Create Template
        GameObject templateGO = new GameObject("Template");
        templateGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform templateRT = templateGO.AddComponent<RectTransform>();
        templateRT.anchorMin = new Vector2(0, 0);
        templateRT.anchorMax = new Vector2(1, 0);
        templateRT.pivot = new Vector2(0.5f, 1);
        templateRT.sizeDelta = new Vector2(0, 150);
        templateRT.anchoredPosition = new Vector2(0, 0);

        Image templateImage = templateGO.AddComponent<Image>();
        templateImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        ScrollRect scrollRect = templateGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        // Create Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(templateGO.transform, false);
        RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        viewportGO.AddComponent<RectMask2D>();

        // Create Content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 100);

        // Create Item
        GameObject itemGO = new GameObject("Item");
        itemGO.transform.SetParent(contentGO.transform, false);
        RectTransform itemRT = itemGO.AddComponent<RectTransform>();
        itemRT.anchorMin = new Vector2(0, 0.5f);
        itemRT.anchorMax = new Vector2(1, 0.5f);
        itemRT.sizeDelta = new Vector2(0, 30);

        Toggle itemToggle = itemGO.AddComponent<Toggle>();

        GameObject itemBgGO = new GameObject("Item Background");
        itemBgGO.transform.SetParent(itemGO.transform, false);
        RectTransform itemBgRT = itemBgGO.AddComponent<RectTransform>();
        itemBgRT.anchorMin = Vector2.zero;
        itemBgRT.anchorMax = Vector2.one;
        itemBgRT.offsetMin = Vector2.zero;
        itemBgRT.offsetMax = Vector2.zero;

        Image itemBgImage = itemBgGO.AddComponent<Image>();
        itemBgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        itemToggle.targetGraphic = itemBgImage;

        GameObject itemLabelGO = new GameObject("Item Label");
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        RectTransform itemLabelRT = itemLabelGO.AddComponent<RectTransform>();
        itemLabelRT.anchorMin = Vector2.zero;
        itemLabelRT.anchorMax = Vector2.one;
        itemLabelRT.offsetMin = new Vector2(10, 1);
        itemLabelRT.offsetMax = new Vector2(-10, -2);

        TextMeshProUGUI itemLabelText = itemLabelGO.AddComponent<TextMeshProUGUI>();
        itemLabelText.text = "Option";
        itemLabelText.fontSize = 20;
        itemLabelText.color = Color.white;
        itemLabelText.alignment = TextAlignmentOptions.Left;

        scrollRect.content = contentRT;
        scrollRect.viewport = viewportRT;

        dropdown.targetGraphic = image;
        dropdown.template = templateRT;
        dropdown.captionText = labelText;
        dropdown.itemText = itemLabelText;

        templateGO.SetActive(false);

        return dropdown;
    }
}
