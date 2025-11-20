using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Complete main menu setup tool - creates ALL UI components
/// </summary>
public class RoomListingSetupTool : EditorWindow
{
    private MainMenuController mainMenuController;
    private GameObject canvasRoot;

    private Vector2 scrollPosition;
    private bool showValidation = true;
    private bool showSetup = true;
    private bool showPrefabGenerator = true;

    [MenuItem("Tools/Main Menu Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<RoomListingSetupTool>("Main Menu Setup");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("COMPLETE MAIN MENU SETUP TOOL", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool creates ALL main menu UI components automatically.", MessageType.Info);

        EditorGUILayout.Space(10);

        // Reference section
        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        mainMenuController = (MainMenuController)EditorGUILayout.ObjectField("Main Menu Controller", mainMenuController, typeof(MainMenuController), true);
        canvasRoot = (GameObject)EditorGUILayout.ObjectField("Canvas Root (Optional)", canvasRoot, typeof(GameObject), true);

        EditorGUILayout.Space(10);

        // Setup section
        showSetup = EditorGUILayout.Foldout(showSetup, "Create UI Components", true);
        if (showSetup)
        {
            EditorGUI.indentLevel++;
            DrawSetupSection();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Prefab generator section
        showPrefabGenerator = EditorGUILayout.Foldout(showPrefabGenerator, "Prefab Generator", true);
        if (showPrefabGenerator)
        {
            EditorGUI.indentLevel++;
            DrawPrefabGeneratorSection();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Validation section
        showValidation = EditorGUILayout.Foldout(showValidation, "Validation", true);
        if (showValidation)
        {
            EditorGUI.indentLevel++;
            DrawValidationSection();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSetupSection()
    {
        EditorGUILayout.HelpBox("ONE-CLICK SETUP - Creates EVERYTHING!", MessageType.Info);

        EditorGUILayout.Space(5);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("CREATE COMPLETE MAIN MENU", GUILayout.Height(40)))
        {
            CreateCompleteMainMenu();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Or create individual components:", EditorStyles.miniLabel);

        if (GUILayout.Button("1. Create Main Panel"))
        {
            CreateMainPanel();
        }

        if (GUILayout.Button("2. Create Multiplayer Panel"))
        {
            CreateMultiplayerPanel();
        }

        if (GUILayout.Button("3. Create Settings Panel"))
        {
            CreateSettingsPanel();
        }

        EditorGUILayout.Space(5);

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Auto-Assign All References", GUILayout.Height(30)))
        {
            AutoAssignReferences();
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawPrefabGeneratorSection()
    {
        if (GUILayout.Button("Generate Room List Item Prefab"))
        {
            GenerateRoomListItemPrefab();
        }
    }

    private void DrawValidationSection()
    {
        if (mainMenuController == null)
        {
            EditorGUILayout.HelpBox("Assign Main Menu Controller above.", MessageType.Warning);
            return;
        }

        SerializedObject so = new SerializedObject(mainMenuController);

        EditorGUILayout.LabelField("UI Panels:", EditorStyles.boldLabel);
        ValidateReference(so, "mainPanel", "Main Panel");
        ValidateReference(so, "multiplayerPanel", "Multiplayer Panel");
        ValidateReference(so, "settingsPanel", "Settings Panel");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Main Menu Buttons:", EditorStyles.boldLabel);
        ValidateReference(so, "playLocalButton", "Play Local Button");
        ValidateReference(so, "playAIButton", "Play AI Button");
        ValidateReference(so, "playOnlineButton", "Play Online Button");
        ValidateReference(so, "settingsButton", "Settings Button");
        ValidateReference(so, "quitButton", "Quit Button");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Multiplayer Buttons:", EditorStyles.boldLabel);
        ValidateReference(so, "hostGameButton", "Host Game Button");
        ValidateReference(so, "joinGameButton", "Join Game Button");
        ValidateReference(so, "backButton", "Back Button");
        ValidateReference(so, "refreshRoomListButton", "Refresh Room List Button");
        ValidateReference(so, "showCreateRoomButton", "Show Create Room Button");
        ValidateReference(so, "showRoomListButton", "Show Room List Button");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Room Listing:", EditorStyles.boldLabel);
        ValidateReference(so, "roomListPanel", "Room List Panel");
        ValidateReference(so, "roomListContainer", "Room List Container");
        ValidateReference(so, "roomListItemPrefab", "Room List Item Prefab");
        ValidateReference(so, "roomListStatusText", "Room List Status Text");
        ValidateReference(so, "roomListScrollRect", "Room List Scroll Rect");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Room Creation:", EditorStyles.boldLabel);
        ValidateReference(so, "createRoomPanel", "Create Room Panel");
        ValidateReference(so, "roomNameInput", "Room Name Input");
        ValidateReference(so, "playerCountDropdown", "Player Count Dropdown");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Settings:", EditorStyles.boldLabel);
        ValidateReference(so, "settingsBackButton", "Settings Back Button");
    }

    private bool ValidateReference(SerializedObject so, string propertyName, string displayName)
    {
        SerializedProperty prop = so.FindProperty(propertyName);

        if (prop != null && prop.objectReferenceValue != null)
        {
            EditorGUILayout.LabelField($"✓ {displayName}", EditorStyles.miniLabel);
            return true;
        }
        else
        {
            EditorGUILayout.LabelField($"✗ {displayName} (Missing)", new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.red } });
            return false;
        }
    }

    private void CreateCompleteMainMenu()
    {
        if (canvasRoot == null)
        {
            if (EditorUtility.DisplayDialog("Create Canvas?", "No canvas root specified. Create a new Canvas?", "Yes", "No"))
            {
                GameObject canvasObj = new GameObject("Canvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                canvasRoot = canvasObj;
            }
            else
            {
                return;
            }
        }

        CreateMainPanel();
        CreateMultiplayerPanel();
        CreateSettingsPanel();

        EditorUtility.DisplayDialog("Success!", "Complete main menu created successfully!\n\nNow click 'Auto-Assign All References'", "OK");
    }

    private void CreateMainPanel()
    {
        GameObject root = canvasRoot ?? FindCanvasRoot();
        if (root == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found. Assign Canvas Root first.", "OK");
            return;
        }

        GameObject mainPanel = new GameObject("MainPanel");
        mainPanel.transform.SetParent(root.transform, false);

        RectTransform rect = mainPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = mainPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        // Create vertical layout for buttons
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(mainPanel.transform, false);
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(400, 600);

        VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        // Create buttons
        CreateButton("PlayLocalButton", "Play Local", buttonContainer.transform, 80);
        CreateButton("PlayAIButton", "Play vs AI", buttonContainer.transform, 80);
        CreateButton("PlayOnlineButton", "Play Online", buttonContainer.transform, 80);
        CreateButton("SettingsButton", "Settings", buttonContainer.transform, 80);
        CreateButton("QuitButton", "Quit", buttonContainer.transform, 80);

        Debug.Log("Main Panel created!");
    }

    private void CreateMultiplayerPanel()
    {
        GameObject root = canvasRoot ?? FindCanvasRoot();
        if (root == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found. Assign Canvas Root first.", "OK");
            return;
        }

        GameObject multiplayerPanel = new GameObject("MultiplayerPanel");
        multiplayerPanel.transform.SetParent(root.transform, false);

        RectTransform rect = multiplayerPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = multiplayerPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        multiplayerPanel.SetActive(false);

        // Create room list panel
        CreateRoomListPanel(multiplayerPanel);

        // Create create room panel
        CreateCreateRoomPanel(multiplayerPanel);

        // Create back button
        GameObject backBtn = CreateButton("BackButton", "Back to Main Menu", multiplayerPanel.transform, 60);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 0);
        backRect.anchorMax = new Vector2(0, 0);
        backRect.pivot = new Vector2(0, 0);
        backRect.anchoredPosition = new Vector2(20, 20);
        backRect.sizeDelta = new Vector2(200, 60);

        // Create join button (for room list)
        GameObject joinBtn = CreateButton("JoinGameButton", "Join Selected Room", multiplayerPanel.transform, 60);
        RectTransform joinRect = joinBtn.GetComponent<RectTransform>();
        joinRect.anchorMin = new Vector2(1, 0);
        joinRect.anchorMax = new Vector2(1, 0);
        joinRect.pivot = new Vector2(1, 0);
        joinRect.anchoredPosition = new Vector2(-20, 20);
        joinRect.sizeDelta = new Vector2(250, 60);

        Debug.Log("Multiplayer Panel created!");
    }

    private void CreateRoomListPanel(GameObject parent)
    {
        GameObject roomListPanel = new GameObject("RoomListPanel");
        roomListPanel.transform.SetParent(parent.transform, false);

        RectTransform rect = roomListPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.1f);
        rect.anchorMax = new Vector2(1, 0.95f);
        rect.offsetMin = new Vector2(20, 20);
        rect.offsetMax = new Vector2(-20, -20);

        Image bg = roomListPanel.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // Header
        GameObject header = new GameObject("Header");
        header.transform.SetParent(roomListPanel.transform, false);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 80);

        TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
        headerText.text = "Available Rooms";
        headerText.fontSize = 48;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = Color.white;
        headerText.fontStyle = FontStyles.Bold;

        // Status text
        GameObject statusObj = new GameObject("RoomListStatusText");
        statusObj.transform.SetParent(roomListPanel.transform, false);
        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 1);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.pivot = new Vector2(0.5f, 1);
        statusRect.anchoredPosition = new Vector2(0, -90);
        statusRect.sizeDelta = new Vector2(-40, 40);

        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Connecting...";
        statusText.fontSize = 28;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.yellow;

        // Scroll view
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(roomListPanel.transform, false);
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0.15f);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(20, 20);
        scrollRect.offsetMax = new Vector2(-20, -150);

        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;

        Image scrollBg = scrollView.AddComponent<Image>();
        scrollBg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        viewport.AddComponent<Mask>();
        viewport.AddComponent<Image>().color = Color.clear;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 15;
        contentLayout.padding = new RectOffset(15, 15, 15, 15);
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;

        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;
        scroll.viewport = viewportRect;

        // Buttons panel
        GameObject buttonsPanel = new GameObject("ButtonsPanel");
        buttonsPanel.transform.SetParent(roomListPanel.transform, false);
        RectTransform buttonsRect = buttonsPanel.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0, 0);
        buttonsRect.anchorMax = new Vector2(1, 0.15f);
        buttonsRect.offsetMin = new Vector2(20, 10);
        buttonsRect.offsetMax = new Vector2(-20, -10);

        HorizontalLayoutGroup buttonLayout = buttonsPanel.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 20;
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = true;

        CreateButton("RefreshRoomListButton", "Refresh", buttonsPanel.transform, 60);
        CreateButton("ShowCreateRoomButton", "Create Room", buttonsPanel.transform, 60);
    }

    private void CreateCreateRoomPanel(GameObject parent)
    {
        GameObject createPanel = new GameObject("CreateRoomPanel");
        createPanel.transform.SetParent(parent.transform, false);

        RectTransform rect = createPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.1f);
        rect.anchorMax = new Vector2(1, 0.95f);
        rect.offsetMin = new Vector2(20, 20);
        rect.offsetMax = new Vector2(-20, -20);

        Image bg = createPanel.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        createPanel.SetActive(false);

        // Header
        GameObject header = new GameObject("Header");
        header.transform.SetParent(createPanel.transform, false);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 80);

        TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
        headerText.text = "Create New Room";
        headerText.fontSize = 48;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = Color.white;
        headerText.fontStyle = FontStyles.Bold;

        // Content area
        GameObject contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(createPanel.transform, false);
        RectTransform contentRect = contentArea.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.2f, 0.3f);
        contentRect.anchorMax = new Vector2(0.8f, 0.8f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = contentArea.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 40;
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // Room name input
        GameObject roomNameObj = CreateInputField("RoomNameInput", "Enter Room Name");
        roomNameObj.transform.SetParent(contentArea.transform, false);
        roomNameObj.AddComponent<LayoutElement>().preferredHeight = 80;

        // Player count dropdown
        GameObject dropdownObj = CreateDropdown("PlayerCountDropdown", "Max Players");
        dropdownObj.transform.SetParent(contentArea.transform, false);
        dropdownObj.AddComponent<LayoutElement>().preferredHeight = 80;

        // Buttons
        GameObject buttonsPanel = new GameObject("ButtonsPanel");
        buttonsPanel.transform.SetParent(contentArea.transform, false);
        buttonsPanel.AddComponent<LayoutElement>().preferredHeight = 80;

        HorizontalLayoutGroup buttonLayout = buttonsPanel.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 30;
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = true;

        CreateButton("HostGameButton", "Host Game", buttonsPanel.transform, 80);
        CreateButton("ShowRoomListButton", "Back to List", buttonsPanel.transform, 80);
    }

    private void CreateSettingsPanel()
    {
        GameObject root = canvasRoot ?? FindCanvasRoot();
        if (root == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found. Assign Canvas Root first.", "OK");
            return;
        }

        GameObject settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(root.transform, false);

        RectTransform rect = settingsPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = settingsPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        settingsPanel.SetActive(false);

        // Header
        GameObject header = new GameObject("Header");
        header.transform.SetParent(settingsPanel.transform, false);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 100);

        TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
        headerText.text = "Settings";
        headerText.fontSize = 54;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = Color.white;
        headerText.fontStyle = FontStyles.Bold;

        // Back button
        GameObject backBtn = CreateButton("SettingsBackButton", "Back", settingsPanel.transform, 60);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 0);
        backRect.anchorMax = new Vector2(0, 0);
        backRect.pivot = new Vector2(0, 0);
        backRect.anchoredPosition = new Vector2(20, 20);
        backRect.sizeDelta = new Vector2(200, 60);

        Debug.Log("Settings Panel created!");
    }

    private GameObject CreateButton(string name, string text, Transform parent, float height)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, height);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.9f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        // Button color transitions
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.6f, 1f, 1f);
        colors.pressedColor = new Color(0.15f, 0.4f, 0.75f, 1f);
        btn.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = height * 0.4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        return btnObj;
    }

    private GameObject CreateInputField(string name, string placeholder)
    {
        GameObject inputObj = new GameObject(name);

        RectTransform rect = inputObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 60);

        Image img = inputObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        TMP_InputField input = inputObj.AddComponent<TMP_InputField>();

        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 0);
        textAreaRect.offsetMax = new Vector2(-10, 0);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 32;
        tmp.color = Color.white;

        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderTmp = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderTmp.text = placeholder;
        placeholderTmp.fontSize = 32;
        placeholderTmp.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        placeholderTmp.fontStyle = FontStyles.Italic;

        input.textViewport = textAreaRect;
        input.textComponent = tmp;
        input.placeholder = placeholderTmp;

        return inputObj;
    }

    private GameObject CreateDropdown(string name, string label)
    {
        GameObject dropdownObj = new GameObject(name);

        RectTransform rect = dropdownObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 60);

        Image img = dropdownObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

        // Create arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.pivot = new Vector2(1, 0.5f);
        arrowRect.anchoredPosition = new Vector2(-10, 0);
        arrowRect.sizeDelta = new Vector2(20, 20);

        Image arrowImg = arrowObj.AddComponent<Image>();
        arrowImg.color = Color.white;

        // Create label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(15, 0);
        labelRect.offsetMax = new Vector2(-40, 0);

        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 32;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.MidlineLeft;

        // Create template
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);

        Image templateImg = templateObj.AddComponent<Image>();
        templateImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        ScrollRect templateScroll = templateObj.AddComponent<ScrollRect>();
        templateScroll.horizontal = false;
        templateScroll.movementType = ScrollRect.MovementType.Clamped;

        templateObj.SetActive(false);

        // Create viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);

        viewportObj.AddComponent<Mask>();
        viewportObj.AddComponent<Image>().color = Color.clear;

        // Create content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;

        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create item
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 40);

        Toggle itemToggle = itemObj.AddComponent<Toggle>();
        itemToggle.isOn = true;

        // Item background
        GameObject itemBgObj = new GameObject("Item Background");
        itemBgObj.transform.SetParent(itemObj.transform, false);
        RectTransform itemBgRect = itemBgObj.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.offsetMin = Vector2.zero;
        itemBgRect.offsetMax = Vector2.zero;

        Image itemBgImg = itemBgObj.AddComponent<Image>();
        itemBgImg.color = new Color(0.25f, 0.25f, 0.3f, 1f);

        itemToggle.targetGraphic = itemBgImg;

        // Item checkmark
        GameObject checkmarkObj = new GameObject("Item Checkmark");
        checkmarkObj.transform.SetParent(itemObj.transform, false);
        RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0, 0.5f);
        checkmarkRect.pivot = new Vector2(0, 0.5f);
        checkmarkRect.anchoredPosition = new Vector2(10, 0);
        checkmarkRect.sizeDelta = new Vector2(20, 20);

        Image checkmarkImg = checkmarkObj.AddComponent<Image>();
        checkmarkImg.color = Color.green;

        itemToggle.graphic = checkmarkImg;

        // Item label
        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform, false);
        RectTransform itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(40, 0);
        itemLabelRect.offsetMax = new Vector2(-10, 0);

        TextMeshProUGUI itemLabelTmp = itemLabelObj.AddComponent<TextMeshProUGUI>();
        itemLabelTmp.text = "Option";
        itemLabelTmp.fontSize = 28;
        itemLabelTmp.color = Color.white;
        itemLabelTmp.alignment = TextAlignmentOptions.MidlineLeft;

        // Configure dropdown references
        dropdown.targetGraphic = img;
        dropdown.template = templateRect;
        dropdown.captionText = labelTmp;
        dropdown.itemText = itemLabelTmp;

        templateScroll.content = contentRect;
        templateScroll.viewport = viewportRect;

        // Add options
        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("2 Players"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("3 Players"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("4 Players"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("6 Players"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("8 Players"));

        dropdown.value = 1; // Default to 3 players

        return dropdownObj;
    }

    private void AutoAssignReferences()
    {
        if (mainMenuController == null)
        {
            EditorUtility.DisplayDialog("Error", "Assign Main Menu Controller first.", "OK");
            return;
        }

        SerializedObject so = new SerializedObject(mainMenuController);
        int assigned = 0;

        // Panels
        assigned += TryAssign(so, "mainPanel", "MainPanel");
        assigned += TryAssign(so, "multiplayerPanel", "MultiplayerPanel");
        assigned += TryAssign(so, "settingsPanel", "SettingsPanel");

        // Main menu buttons
        assigned += TryAssign(so, "playLocalButton", "PlayLocalButton", typeof(Button));
        assigned += TryAssign(so, "playAIButton", "PlayAIButton", typeof(Button));
        assigned += TryAssign(so, "playOnlineButton", "PlayOnlineButton", typeof(Button));
        assigned += TryAssign(so, "settingsButton", "SettingsButton", typeof(Button));
        assigned += TryAssign(so, "quitButton", "QuitButton", typeof(Button));

        // Multiplayer buttons
        assigned += TryAssign(so, "hostGameButton", "HostGameButton", typeof(Button));
        assigned += TryAssign(so, "joinGameButton", "JoinGameButton", typeof(Button));
        assigned += TryAssign(so, "backButton", "BackButton", typeof(Button));
        assigned += TryAssign(so, "refreshRoomListButton", "RefreshRoomListButton", typeof(Button));
        assigned += TryAssign(so, "showCreateRoomButton", "ShowCreateRoomButton", typeof(Button));
        assigned += TryAssign(so, "showRoomListButton", "ShowRoomListButton", typeof(Button));
        assigned += TryAssign(so, "settingsBackButton", "SettingsBackButton", typeof(Button));

        // Room listing
        assigned += TryAssign(so, "roomListPanel", "RoomListPanel");
        assigned += TryAssign(so, "roomListContainer", "Content", typeof(Transform));
        assigned += TryAssign(so, "roomListStatusText", "RoomListStatusText", typeof(TextMeshProUGUI));
        assigned += TryAssign(so, "roomListScrollRect", "ScrollView", typeof(ScrollRect));
        assigned += TryAssign(so, "createRoomPanel", "CreateRoomPanel");
        assigned += TryAssign(so, "roomNameInput", "RoomNameInput", typeof(TMP_InputField));
        assigned += TryAssign(so, "playerCountDropdown", "PlayerCountDropdown", typeof(TMP_Dropdown));

        so.ApplyModifiedProperties();

        EditorUtility.DisplayDialog("Complete!", $"Assigned {assigned} references successfully!", "OK");
        Debug.Log($"Auto-assigned {assigned} references");
    }

    private int TryAssign(SerializedObject so, string propertyName, string objectName, System.Type componentType = null)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null) return 0;
        if (prop.objectReferenceValue != null) return 0; // Skip if already assigned

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.name == objectName)
            {
                if (componentType == null)
                {
                    prop.objectReferenceValue = go;
                    Debug.Log($"✓ Assigned {objectName} to {propertyName}");
                    return 1;
                }
                else
                {
                    Component comp = go.GetComponent(componentType);
                    if (comp != null)
                    {
                        prop.objectReferenceValue = comp;
                        Debug.Log($"✓ Assigned {objectName} ({componentType.Name}) to {propertyName}");
                        return 1;
                    }
                }
            }
        }
        return 0;
    }

    private void GenerateRoomListItemPrefab()
    {
        GameObject item = new GameObject("RoomListItem");
        RectTransform rect = item.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(700, 100);

        Image bg = item.AddComponent<Image>();
        bg.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);

        HorizontalLayoutGroup layout = item.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 15, 15);
        layout.spacing = 20;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = true;

        // Room name
        GameObject nameObj = new GameObject("RoomNameText");
        nameObj.transform.SetParent(item.transform, false);
        nameObj.AddComponent<LayoutElement>().preferredWidth = 350;
        TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        nameTmp.text = "Room Name";
        nameTmp.fontSize = 36;
        nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
        nameTmp.color = Color.white;
        nameTmp.fontStyle = FontStyles.Bold;

        // Player count
        GameObject countObj = new GameObject("PlayerCountText");
        countObj.transform.SetParent(item.transform, false);
        countObj.AddComponent<LayoutElement>().preferredWidth = 180;
        TextMeshProUGUI countTmp = countObj.AddComponent<TextMeshProUGUI>();
        countTmp.text = "0/4 Players";
        countTmp.fontSize = 32;
        countTmp.alignment = TextAlignmentOptions.Center;
        countTmp.color = Color.green;

        // Select button
        GameObject btnObj = new GameObject("SelectButton");
        btnObj.transform.SetParent(item.transform, false);
        btnObj.AddComponent<LayoutElement>().preferredWidth = 150;

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.7f, 0.2f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "Select";
        btnTmp.fontSize = 32;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.color = Color.white;
        btnTmp.fontStyle = FontStyles.Bold;

        string path = EditorUtility.SaveFilePanelInProject("Save Room List Item", "RoomListItem", "prefab", "Save prefab");
        if (!string.IsNullOrEmpty(path))
        {
            PrefabUtility.SaveAsPrefabAsset(item, path);
            DestroyImmediate(item);
            EditorUtility.DisplayDialog("Success", "Prefab created at:\n" + path, "OK");

            // Auto-assign to controller
            if (mainMenuController != null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                SerializedObject so = new SerializedObject(mainMenuController);
                SerializedProperty prop = so.FindProperty("roomListItemPrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedProperties();
                    Debug.Log("✓ Auto-assigned prefab to controller");
                }
            }
        }
        else
        {
            DestroyImmediate(item);
        }
    }

    private GameObject FindCanvasRoot()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        return canvases.Length > 0 ? canvases[0].gameObject : null;
    }
}
