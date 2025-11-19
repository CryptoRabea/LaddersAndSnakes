using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime helper script for room listing UI setup and validation
/// Can auto-find and assign references at runtime
/// </summary>
[RequireComponent(typeof(MainMenuController))]
public class RoomListingUIHelper : MonoBehaviour
{
    [Header("Auto-Find Settings")]
    [SerializeField] private bool autoFindOnAwake = true;
    [SerializeField] private bool logMissingReferences = true;

    private MainMenuController mainMenuController;

    private void Awake()
    {
        mainMenuController = GetComponent<MainMenuController>();

        if (autoFindOnAwake)
        {
            AutoFindAndAssignReferences();
        }
    }

    [ContextMenu("Auto-Find All References")]
    public void AutoFindAndAssignReferences()
    {
        if (mainMenuController == null)
        {
            Debug.LogError("[RoomListingUIHelper] MainMenuController not found!");
            return;
        }

        Debug.Log("[RoomListingUIHelper] Starting auto-find...");

        int foundCount = 0;
        int missingCount = 0;

        // Use reflection to find and assign references
        var fields = typeof(MainMenuController).GetFields(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public
        );

        foreach (var field in fields)
        {
            var serializeField = field.GetCustomAttributes(typeof(SerializeField), false);
            if (serializeField.Length == 0 && !field.IsPublic) continue;

            var headerAttr = field.GetCustomAttributes(typeof(HeaderAttribute), false);
            if (headerAttr.Length > 0)
            {
                var header = (HeaderAttribute)headerAttr[0];
                if (header.header == "Room Listing" || header.header == "Room Creation")
                {
                    if (TryFindAndAssign(field.Name, field.FieldType))
                    {
                        foundCount++;
                    }
                    else
                    {
                        missingCount++;
                        if (logMissingReferences)
                        {
                            Debug.LogWarning($"[RoomListingUIHelper] Could not find: {field.Name} ({field.FieldType.Name})");
                        }
                    }
                }
            }
        }

        Debug.Log($"[RoomListingUIHelper] Auto-find complete. Found: {foundCount}, Missing: {missingCount}");
    }

    private bool TryFindAndAssign(string fieldName, System.Type fieldType)
    {
        // Convert field name to likely GameObject name
        // e.g., "roomListPanel" -> "RoomListPanel"
        string searchName = ConvertFieldNameToObjectName(fieldName);

        // Search for the object
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name.Equals(searchName, System.StringComparison.OrdinalIgnoreCase))
            {
                // Found the GameObject
                if (fieldType == typeof(GameObject))
                {
                    SetFieldValue(fieldName, go);
                    Debug.Log($"[RoomListingUIHelper] ✓ Found and assigned: {searchName} (GameObject)");
                    return true;
                }
                else if (fieldType == typeof(Transform))
                {
                    SetFieldValue(fieldName, go.transform);
                    Debug.Log($"[RoomListingUIHelper] ✓ Found and assigned: {searchName} (Transform)");
                    return true;
                }
                else
                {
                    // Try to get component
                    Component comp = go.GetComponent(fieldType);
                    if (comp != null)
                    {
                        SetFieldValue(fieldName, comp);
                        Debug.Log($"[RoomListingUIHelper] ✓ Found and assigned: {searchName} ({fieldType.Name})");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private string ConvertFieldNameToObjectName(string fieldName)
    {
        // Remove common prefixes
        fieldName = fieldName.TrimStart('_');

        // Convert camelCase to PascalCase
        if (!string.IsNullOrEmpty(fieldName))
        {
            fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
        }

        return fieldName;
    }

    private void SetFieldValue(string fieldName, object value)
    {
        var field = typeof(MainMenuController).GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public
        );

        if (field != null)
        {
            field.SetValue(mainMenuController, value);
        }
    }

    [ContextMenu("Validate All References")]
    public void ValidateReferences()
    {
        if (mainMenuController == null)
        {
            Debug.LogError("[RoomListingUIHelper] MainMenuController not found!");
            return;
        }

        Debug.Log("[RoomListingUIHelper] Validating references...");

        int validCount = 0;
        int missingCount = 0;

        // Check room listing references
        missingCount += ValidateReference("roomListPanel", "Room List Panel");
        missingCount += ValidateReference("roomListContainer", "Room List Container");
        missingCount += ValidateReference("roomListItemPrefab", "Room List Item Prefab");
        missingCount += ValidateReference("roomListStatusText", "Room List Status Text");
        missingCount += ValidateReference("roomListScrollRect", "Room List Scroll Rect");
        missingCount += ValidateReference("createRoomPanel", "Create Room Panel");
        missingCount += ValidateReference("roomNameInput", "Room Name Input");
        missingCount += ValidateReference("playerCountDropdown", "Player Count Dropdown");
        missingCount += ValidateReference("refreshRoomListButton", "Refresh Room List Button");
        missingCount += ValidateReference("showCreateRoomButton", "Show Create Room Button");
        missingCount += ValidateReference("showRoomListButton", "Show Room List Button");

        validCount = 11 - missingCount;

        if (missingCount == 0)
        {
            Debug.Log($"[RoomListingUIHelper] ✓ All {validCount} references are valid!");
        }
        else
        {
            Debug.LogWarning($"[RoomListingUIHelper] Validation complete. Valid: {validCount}, Missing: {missingCount}");
        }
    }

    private int ValidateReference(string fieldName, string displayName)
    {
        var field = typeof(MainMenuController).GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public
        );

        if (field != null)
        {
            object value = field.GetValue(mainMenuController);
            if (value != null && !value.Equals(null))
            {
                Debug.Log($"[RoomListingUIHelper] ✓ {displayName}: Valid");
                return 0;
            }
            else
            {
                Debug.LogWarning($"[RoomListingUIHelper] ✗ {displayName}: Missing");
                return 1;
            }
        }

        return 1;
    }

    [ContextMenu("Print Required UI Structure")]
    public void PrintRequiredUIStructure()
    {
        string structure = @"
=== REQUIRED UI STRUCTURE ===

MultiplayerPanel/
├── RoomListPanel (GameObject with Image)
│   ├── Header (TextMeshProUGUI) - ""Available Rooms""
│   ├── StatusText (TextMeshProUGUI) - Connection status
│   ├── ScrollView (ScrollRect)
│   │   └── Viewport (Mask + Image)
│   │       └── Content (Transform) - Parent for room items
│   │           └── [Room items spawn here]
│   └── ButtonsPanel (HorizontalLayoutGroup)
│       ├── RefreshButton (Button)
│       └── ShowCreateRoomButton (Button)
│
└── CreateRoomPanel (GameObject with Image)
    ├── Header (TextMeshProUGUI) - ""Create Room""
    ├── ContentArea (VerticalLayoutGroup)
    │   ├── RoomNameInput (TMP_InputField)
    │   ├── PlayerCountDropdown (TMP_Dropdown)
    │   └── ButtonsPanel (HorizontalLayoutGroup)
    │       ├── HostGameButton (Button)
    │       └── ShowRoomListButton (Button)

=== ROOM LIST ITEM PREFAB ===

RoomListItem (Prefab with Image + HorizontalLayoutGroup)
├── RoomNameText (TextMeshProUGUI)
├── PlayerCountText (TextMeshProUGUI)
└── SelectButton (Button)
    └── Text (TextMeshProUGUI)

=== REFERENCES TO ASSIGN ===

[Header(""Room Listing"")]
- roomListPanel: GameObject
- roomListContainer: Transform (Content)
- roomListItemPrefab: GameObject (Prefab)
- roomListStatusText: TextMeshProUGUI
- roomListScrollRect: ScrollRect

[Header(""Room Creation"")]
- createRoomPanel: GameObject
- roomNameInput: TMP_InputField
- playerCountDropdown: TMP_Dropdown

[Header(""Multiplayer Options"")]
- refreshRoomListButton: Button
- showCreateRoomButton: Button
- showRoomListButton: Button
- hostGameButton: Button
- joinGameButton: Button

================================
";

        Debug.Log(structure);
    }
}
