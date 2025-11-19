using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool to set up and validate room listing UI components
/// Helps create the required UI hierarchy and automatically assigns references
/// </summary>
public class RoomListingSetupTool : EditorWindow
{
    private MainMenuController mainMenuController;
    private GameObject multiplayerPanel;

    private Vector2 scrollPosition;
    private bool showValidation = true;
    private bool showSetup = true;
    private bool showPrefabGenerator = true;

    [MenuItem("Tools/Room Listing Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<RoomListingSetupTool>("Room Listing Setup");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Room Listing Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool helps you set up the room listing UI components for the main menu.", MessageType.Info);

        EditorGUILayout.Space(10);

        // Reference section
        EditorGUILayout.LabelField("Main References", EditorStyles.boldLabel);
        mainMenuController = (MainMenuController)EditorGUILayout.ObjectField("Main Menu Controller", mainMenuController, typeof(MainMenuController), true);
        multiplayerPanel = (GameObject)EditorGUILayout.ObjectField("Multiplayer Panel", multiplayerPanel, typeof(GameObject), true);

        EditorGUILayout.Space(10);

        // Setup section
        showSetup = EditorGUILayout.Foldout(showSetup, "Setup UI Components", true);
        if (showSetup)
        {
            EditorGUI.indentLevel++;
            DrawSetupSection();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Prefab generator section
        showPrefabGenerator = EditorGUILayout.Foldout(showPrefabGenerator, "Room List Item Prefab Generator", true);
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
        EditorGUILayout.HelpBox("Click the buttons below to create the required UI hierarchy automatically.", MessageType.Info);

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Create Room List Panel"))
        {
            CreateRoomListPanel();
        }

        if (GUILayout.Button("Create Room Panel"))
        {
            CreateRoomPanel();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Auto-Assign All References"))
        {
            AutoAssignReferences();
        }
    }

    private void DrawPrefabGeneratorSection()
    {
        EditorGUILayout.HelpBox("Generate a room list item prefab with the correct structure.", MessageType.Info);

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Generate Room List Item Prefab"))
        {
            GenerateRoomListItemPrefab();
        }
    }

    private void DrawValidationSection()
    {
        if (mainMenuController == null)
        {
            EditorGUILayout.HelpBox("Please assign the Main Menu Controller reference above.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(5);

        bool allValid = true;

        // Validate references
        SerializedObject so = new SerializedObject(mainMenuController);

        allValid &= ValidateReference(so, "roomListPanel", "Room List Panel");
        allValid &= ValidateReference(so, "roomListContainer", "Room List Container");
        allValid &= ValidateReference(so, "roomListItemPrefab", "Room List Item Prefab");
        allValid &= ValidateReference(so, "roomListStatusText", "Room List Status Text");
        allValid &= ValidateReference(so, "roomListScrollRect", "Room List Scroll Rect");
        allValid &= ValidateReference(so, "createRoomPanel", "Create Room Panel");
        allValid &= ValidateReference(so, "roomNameInput", "Room Name Input");
        allValid &= ValidateReference(so, "playerCountDropdown", "Player Count Dropdown");
        allValid &= ValidateReference(so, "refreshRoomListButton", "Refresh Room List Button");
        allValid &= ValidateReference(so, "showCreateRoomButton", "Show Create Room Button");
        allValid &= ValidateReference(so, "showRoomListButton", "Show Room List Button");

        EditorGUILayout.Space(5);

        if (allValid)
        {
            EditorGUILayout.HelpBox("✓ All required references are assigned!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Some references are missing. Use the Auto-Assign button or manually assign them.", MessageType.Warning);
        }
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
            EditorGUILayout.LabelField($"✗ {displayName} (Missing)", EditorStyles.miniLabel);
            return false;
        }
    }

    private void CreateRoomListPanel()
    {
        if (multiplayerPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign the Multiplayer Panel reference first.", "OK");
            return;
        }

        // Create room list panel
        GameObject roomListPanel = new GameObject("RoomListPanel");
        roomListPanel.transform.SetParent(multiplayerPanel.transform, false);

        RectTransform rect = roomListPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0.8f);
        rect.offsetMin = new Vector2(20, 20);
        rect.offsetMax = new Vector2(-20, -20);

        // Add background
        Image bg = roomListPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Create header
        GameObject header = new GameObject("Header");
        header.transform.SetParent(roomListPanel.transform, false);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.sizeDelta = new Vector2(0, 80);

        TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
        headerText.text = "Available Rooms";
        headerText.fontSize = 36;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = Color.white;

        // Create status text
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(roomListPanel.transform, false);
        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 1);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.pivot = new Vector2(0.5f, 1);
        statusRect.anchoredPosition = new Vector2(0, -80);
        statusRect.sizeDelta = new Vector2(-40, 50);

        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Connecting...";
        statusText.fontSize = 24;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.yellow;

        // Create scroll view
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(roomListPanel.transform, false);
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(20, 100);
        scrollRect.offsetMax = new Vector2(-20, -150);

        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        Image scrollBg = scrollView.AddComponent<Image>();
        scrollBg.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);

        // Create viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        viewport.AddComponent<Image>();

        // Create content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;
        scroll.viewport = viewportRect;

        // Create buttons panel
        GameObject buttonsPanel = new GameObject("ButtonsPanel");
        buttonsPanel.transform.SetParent(roomListPanel.transform, false);
        RectTransform buttonsRect = buttonsPanel.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0, 0);
        buttonsRect.anchorMax = new Vector2(1, 0);
        buttonsRect.pivot = new Vector2(0.5f, 0);
        buttonsRect.anchoredPosition = Vector2.zero;
        buttonsRect.sizeDelta = new Vector2(0, 80);

        HorizontalLayoutGroup buttonLayout = buttonsPanel.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 20;
        buttonLayout.padding = new RectOffset(20, 20, 10, 10);
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = true;
        buttonLayout.childForceExpandHeight = true;

        // Create refresh button
        GameObject refreshBtn = CreateButton("RefreshButton", "Refresh");
        refreshBtn.transform.SetParent(buttonsPanel.transform, false);

        // Create show create room button
        GameObject showCreateBtn = CreateButton("ShowCreateRoomButton", "Create Room");
        showCreateBtn.transform.SetParent(buttonsPanel.transform, false);

        EditorUtility.DisplayDialog("Success", "Room List Panel created successfully!", "OK");

        Debug.Log("Room List Panel created at: " + GetGameObjectPath(roomListPanel));
    }

    private void CreateRoomPanel()
    {
        if (multiplayerPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign the Multiplayer Panel reference first.", "OK");
            return;
        }

        // Create create room panel
        GameObject createPanel = new GameObject("CreateRoomPanel");
        createPanel.transform.SetParent(multiplayerPanel.transform, false);

        RectTransform rect = createPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0.8f);
        rect.offsetMin = new Vector2(20, 20);
        rect.offsetMax = new Vector2(-20, -20);

        // Add background
        Image bg = createPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Initially hide it
        createPanel.SetActive(false);

        // Create header
        GameObject header = new GameObject("Header");
        header.transform.SetParent(createPanel.transform, false);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.sizeDelta = new Vector2(0, 80);

        TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
        headerText.text = "Create Room";
        headerText.fontSize = 36;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = Color.white;

        // Create content area
        GameObject contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(createPanel.transform, false);
        RectTransform contentRect = contentArea.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.2f, 0.3f);
        contentRect.anchorMax = new Vector2(0.8f, 0.8f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = contentArea.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 30;
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        // Room name input
        GameObject roomNameObj = CreateInputField("RoomNameInput", "Room Name");
        roomNameObj.transform.SetParent(contentArea.transform, false);
        LayoutElement roomNameLayout = roomNameObj.AddComponent<LayoutElement>();
        roomNameLayout.preferredHeight = 80;

        // Player count dropdown
        GameObject dropdownObj = CreateDropdown("PlayerCountDropdown", "Max Players");
        dropdownObj.transform.SetParent(contentArea.transform, false);
        LayoutElement dropdownLayout = dropdownObj.AddComponent<LayoutElement>();
        dropdownLayout.preferredHeight = 80;

        // Buttons panel
        GameObject buttonsPanel = new GameObject("ButtonsPanel");
        buttonsPanel.transform.SetParent(contentArea.transform, false);
        LayoutElement buttonsLayout = buttonsPanel.AddComponent<LayoutElement>();
        buttonsLayout.preferredHeight = 100;

        HorizontalLayoutGroup buttonLayout = buttonsPanel.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 20;
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = true;

        // Host button
        GameObject hostBtn = CreateButton("HostGameButton", "Host Game");
        hostBtn.transform.SetParent(buttonsPanel.transform, false);

        // Show room list button
        GameObject showListBtn = CreateButton("ShowRoomListButton", "Back to List");
        showListBtn.transform.SetParent(buttonsPanel.transform, false);

        EditorUtility.DisplayDialog("Success", "Create Room Panel created successfully!", "OK");

        Debug.Log("Create Room Panel created at: " + GetGameObjectPath(createPanel));
    }

    private GameObject CreateButton(string name, string text)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 60);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btnObj;
    }

    private GameObject CreateInputField(string name, string placeholder)
    {
        GameObject inputObj = new GameObject(name);
        RectTransform rect = inputObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 60);

        Image img = inputObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 1f);

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
        tmp.fontSize = 28;
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
        placeholderTmp.fontSize = 28;
        placeholderTmp.color = new Color(0.5f, 0.5f, 0.5f, 1f);
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
        img.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 0);
        labelRect.offsetMax = new Vector2(-30, 0);

        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 28;
        labelTmp.color = Color.white;

        dropdown.captionText = labelTmp;

        return dropdownObj;
    }

    private void AutoAssignReferences()
    {
        if (mainMenuController == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign the Main Menu Controller reference first.", "OK");
            return;
        }

        SerializedObject so = new SerializedObject(mainMenuController);

        int assignedCount = 0;

        // Find and assign references
        assignedCount += TryAssignReference(so, "roomListPanel", "RoomListPanel");
        assignedCount += TryAssignReference(so, "roomListContainer", "Content", typeof(Transform));
        assignedCount += TryAssignReference(so, "roomListStatusText", "StatusText", typeof(TextMeshProUGUI));
        assignedCount += TryAssignReference(so, "roomListScrollRect", "ScrollView", typeof(ScrollRect));
        assignedCount += TryAssignReference(so, "createRoomPanel", "CreateRoomPanel");
        assignedCount += TryAssignReference(so, "roomNameInput", "RoomNameInput", typeof(TMP_InputField));
        assignedCount += TryAssignReference(so, "playerCountDropdown", "PlayerCountDropdown", typeof(TMP_Dropdown));
        assignedCount += TryAssignReference(so, "refreshRoomListButton", "RefreshButton", typeof(Button));
        assignedCount += TryAssignReference(so, "showCreateRoomButton", "ShowCreateRoomButton", typeof(Button));
        assignedCount += TryAssignReference(so, "showRoomListButton", "ShowRoomListButton", typeof(Button));
        assignedCount += TryAssignReference(so, "hostGameButton", "HostGameButton", typeof(Button));
        assignedCount += TryAssignReference(so, "joinGameButton", "JoinGameButton", typeof(Button));

        so.ApplyModifiedProperties();

        EditorUtility.DisplayDialog("Auto-Assign Complete", $"Successfully assigned {assignedCount} references.", "OK");
    }

    private int TryAssignReference(SerializedObject so, string propertyName, string objectName, System.Type componentType = null)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null) return 0;

        // Skip if already assigned
        if (prop.objectReferenceValue != null) return 0;

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.name == objectName)
            {
                if (componentType == null)
                {
                    prop.objectReferenceValue = go;
                    Debug.Log($"Assigned {objectName} to {propertyName}");
                    return 1;
                }
                else
                {
                    Component comp = go.GetComponent(componentType);
                    if (comp != null)
                    {
                        prop.objectReferenceValue = comp;
                        Debug.Log($"Assigned {objectName} ({componentType.Name}) to {propertyName}");
                        return 1;
                    }
                }
            }
        }

        return 0;
    }

    private void GenerateRoomListItemPrefab()
    {
        // Create room list item
        GameObject item = new GameObject("RoomListItem");
        RectTransform rect = item.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 100);

        Image bg = item.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        HorizontalLayoutGroup layout = item.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 10, 10);
        layout.spacing = 20;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        // Room name text
        GameObject nameObj = new GameObject("RoomNameText");
        nameObj.transform.SetParent(item.transform, false);
        LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
        nameLayout.preferredWidth = 300;

        TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        nameTmp.text = "Room Name";
        nameTmp.fontSize = 32;
        nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
        nameTmp.color = Color.white;

        // Player count text
        GameObject countObj = new GameObject("PlayerCountText");
        countObj.transform.SetParent(item.transform, false);
        LayoutElement countLayout = countObj.AddComponent<LayoutElement>();
        countLayout.preferredWidth = 150;

        TextMeshProUGUI countTmp = countObj.AddComponent<TextMeshProUGUI>();
        countTmp.text = "0/4 Players";
        countTmp.fontSize = 28;
        countTmp.alignment = TextAlignmentOptions.Center;
        countTmp.color = Color.green;

        // Select button
        GameObject btnObj = new GameObject("SelectButton");
        btnObj.transform.SetParent(item.transform, false);
        LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
        btnLayout.preferredWidth = 150;

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);

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
        btnTmp.fontSize = 28;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.color = Color.white;

        // Save as prefab
        string path = EditorUtility.SaveFilePanelInProject("Save Room List Item Prefab", "RoomListItem", "prefab", "Save the room list item prefab");
        if (!string.IsNullOrEmpty(path))
        {
            PrefabUtility.SaveAsPrefabAsset(item, path);
            DestroyImmediate(item);
            EditorUtility.DisplayDialog("Success", "Room List Item Prefab created successfully at:\n" + path, "OK");
        }
        else
        {
            DestroyImmediate(item);
        }
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
