# UI Reference Tool Documentation

## Overview

The UI Reference Tool is a comprehensive system for automatically finding and binding UI element references in Unity. It eliminates the need for manual `FindObjectOfType` calls and reduces Inspector assignment errors.

## Features

- **Automatic Reference Binding**: Automatically find and assign UI elements by name or path
- **Editor Window**: Visual tool for managing all UI references in the scene
- **Validation System**: Validate that all required references are properly assigned
- **Runtime Auto-Binding**: Optional runtime binding for dynamically created UI
- **Custom Inspector**: Quick-access buttons in component inspectors
- **Play Mode Validation**: Warns about missing references before entering play mode

---

## Quick Start

### 1. Mark Fields with Attributes

Instead of using `[SerializeField]` and manually assigning in the Inspector:

```csharp
using LaddersAndSnakes.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyUIManager : MonoBehaviour
{
    // Old way - manual assignment required
    [SerializeField] private Button myButton;

    // NEW way - automatic binding
    [UIReference] private Button rollDiceButton;
    [UIReference] private TextMeshProUGUI turnIndicatorText;
    [UIReference(Path = "Canvas/GameOverPanel")] private GameObject gameOverPanel;
    [UIReference(Name = "WinnerText", Required = false)] private TextMeshProUGUI winnerText;
}
```

### 2. Bind References

**Option A: Use the Editor Window**
1. Open `Window > Ladders & Snakes > UI Reference Tool`
2. Click "Bind All" to bind all references in the scene
3. Click "Validate All" to check for missing references

**Option B: Use Menu Items**
- `Tools > Ladders & Snakes > Bind All UI References in Scene`
- `Tools > Ladders & Snakes > Validate All UI References`

**Option C: Use Inspector Buttons**
- Select any component with `[UIReference]` attributes
- Click "Bind References" button at the bottom of the Inspector

**Option D: Runtime Auto-Binding**
- Add `UIReferenceAutoBinding` component to your UI GameObject
- References will bind automatically at runtime in Awake/Start

---

## Attribute Reference

### `[UIReference]`

Marks a field for automatic binding.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Path` | string | null | Hierarchical path to the UI element (e.g., "Canvas/Panel/Button") |
| `Name` | string | null | GameObject name to search for. If not set, uses field name |
| `SearchInChildren` | bool | true | Search in children of the root transform |
| `Required` | bool | true | Whether this reference is required (errors if not found) |
| `Tag` | string | null | Only bind to GameObjects with this tag |

**Examples:**

```csharp
// Basic usage - searches for GameObject named "RollDiceButton"
[UIReference]
private Button rollDiceButton;

// Custom name - searches for GameObject named "BtnRoll"
[UIReference(Name = "BtnRoll")]
private Button rollButton;

// Path-based - direct hierarchy path
[UIReference(Path = "Canvas/TopPanel/TurnIndicator")]
private TextMeshProUGUI turnText;

// Optional reference - won't error if not found
[UIReference(Required = false)]
private GameObject optionalPanel;

// Search entire scene (not just children)
[UIReference(SearchInChildren = false)]
private Camera mainCamera;

// Tag-based search
[UIReference(Tag = "Player")]
private GameObject playerObject;
```

### `[UIRoot]`

Marks a field as the root transform for searching child UI elements.

```csharp
public class MyUIManager : MonoBehaviour
{
    [UIRoot]
    [SerializeField] private Transform canvasTransform;

    // Will search for this starting from canvasTransform
    [UIReference]
    private Button myButton;
}
```

---

## Naming Conventions

The system automatically converts field names to GameObject names:

| Field Name | Searches For GameObject |
|------------|-------------------------|
| `rollDiceButton` | `RollDiceButton` |
| `turnIndicatorText` | `TurnIndicatorText` |
| `_gameOverPanel` | `GameOverPanel` |
| `m_winnerText` | `WinnerText` |

Common prefixes (`_`, `m_`, `s_`) are automatically stripped.

---

## Real-World Examples

### Example 1: GameUIManager

**Before:**
```csharp
public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;
    [SerializeField] private TextMeshProUGUI diceResultText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    private void Start()
    {
        // Manual FindObjectOfType as fallback
        if (diceModel == null)
            diceModel = FindObjectOfType<DiceModel>();
    }
}
```

**After:**
```csharp
using LaddersAndSnakes.Core;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    [UIReference] private Button rollDiceButton;
    [UIReference] private TextMeshProUGUI turnIndicatorText;
    [UIReference] private TextMeshProUGUI diceResultText;
    [UIReference] private GameObject gameOverPanel;
    [UIReference(Path = "GameOverPanel/WinnerText")] private TextMeshProUGUI winnerText;
    [UIReference(Path = "GameOverPanel/PlayAgainButton")] private Button playAgainButton;
    [UIReference(Path = "GameOverPanel/MainMenuButton")] private Button mainMenuButton;

    // No manual finding needed! Bind via Editor Window or add UIReferenceAutoBinding component
}
```

### Example 2: MainMenuController with UIRoot

```csharp
using LaddersAndSnakes.Core;

public class MainMenuController : MonoBehaviour
{
    [UIRoot]
    [SerializeField] private Transform canvasRoot;

    [Header("Main Panel")]
    [UIReference(Path = "MainPanel/PlayLocalButton")] private Button playLocalButton;
    [UIReference(Path = "MainPanel/PlayAIButton")] private Button playAIButton;
    [UIReference(Path = "MainPanel/PlayOnlineButton")] private Button playOnlineButton;

    [Header("Multiplayer Panel")]
    [UIReference(Path = "MultiplayerPanel")] private GameObject multiplayerPanel;
    [UIReference(Path = "MultiplayerPanel/HostButton")] private Button hostButton;
    [UIReference(Path = "MultiplayerPanel/JoinButton")] private Button joinButton;

    [Header("Settings")]
    [UIReference(Path = "SettingsPanel", Required = false)] private GameObject settingsPanel;
}
```

### Example 3: Runtime Auto-Binding

For UI that's created at runtime:

```csharp
using LaddersAndSnakes.Core;

public class DynamicUIManager : MonoBehaviour
{
    [UIReference] private Button dynamicButton;
    [UIReference] private TextMeshProUGUI dynamicText;

    private void Awake()
    {
        // Bind references automatically at runtime
        UIReferenceBinder.BindUIReferences(this);

        // Now references are ready to use
        dynamicButton.onClick.AddListener(OnButtonClick);
    }
}
```

Or add the `UIReferenceAutoBinding` component:

```csharp
// No code needed! Just add UIReferenceAutoBinding component in Inspector
// or via Tools > Ladders & Snakes > Add Auto-Binding Component
```

---

## API Reference

### UIReferenceBinder

Static utility class for binding operations.

```csharp
// Bind references on a single component
int boundCount = UIReferenceBinder.BindUIReferences(myComponent, logResults: true);

// Get all components with UIReference attributes in scene
List<MonoBehaviour> components = UIReferenceBinder.GetAllUIReferenceBehaviours();

// Validate all references
UIReferenceBinder.ValidationResult result = UIReferenceBinder.ValidateAllReferences();
if (result.HasErrors)
{
    Debug.LogError($"Found {result.Errors.Count} missing references!");
}

// Validate a single component
UIReferenceBinder.ValidationResult result = new UIReferenceBinder.ValidationResult();
UIReferenceBinder.ValidateComponent(myComponent, result);
```

### UIReferenceAutoBinding Component

Add this component to enable runtime binding.

**Inspector Properties:**
- **Bind In Awake**: Bind in Awake (earlier) or Start (later)
- **Log Results**: Log binding results to console
- **Bind All Components**: Automatically bind all components on GameObject
- **Specific Components**: If not binding all, specify which components

**Methods:**
```csharp
// Manually trigger rebinding
autoBinding.RebindReferences();
```

---

## Editor Tools

### UI Reference Tool Window

Access via: `Window > Ladders & Snakes > UI Reference Tool`

**Features:**
- **Refresh**: Scan scene for components with UI references
- **Bind All**: Automatically bind all references in the scene
- **Validate All**: Check all references are assigned
- **Clear All**: Remove all assigned references (with confirmation)
- **Auto-bind on Load**: Automatically bind when window opens
- **Show Successful**: Toggle display of successfully bound references

Each component shows:
- ✓ Green checkmark: Reference is assigned
- ✗ Red X: Required reference is missing
- ✗ Yellow X: Optional reference is missing

**Per-Component Actions:**
- **Bind**: Bind only this component's references
- **Select**: Select the GameObject in hierarchy

### Menu Items

Located under `Tools > Ladders & Snakes`:

1. **Bind All UI References in Scene**: Bind all references in current scene
2. **Validate All UI References**: Check all references are valid
3. **Bind Selected GameObject UI References**: Bind only selected GameObject
4. **Add Auto-Binding Component**: Add UIReferenceAutoBinding to selection

### Inspector Integration

Any component with `[UIReference]` attributes will show:
- **Bind References** button: Bind this component's references
- **Validate** button: Check this component's references

### Play Mode Validation

Automatically validates references when entering play mode. Warnings appear in console if required references are missing.

To disable play mode validation, comment out the `[InitializeOnLoad]` in `UIReferenceEditorMenu.cs`.

---

## Best Practices

### 1. Use Descriptive Names

```csharp
// Good - clear and descriptive
[UIReference] private Button rollDiceButton;
[UIReference] private TextMeshProUGUI playerNameText;

// Avoid - too generic
[UIReference] private Button btn1;
[UIReference] private TextMeshProUGUI text;
```

### 2. Use Paths for Nested UI

```csharp
// Good - explicit path prevents ambiguity
[UIReference(Path = "Canvas/GameOverPanel/WinnerText")]
private TextMeshProUGUI winnerText;

// Less reliable - might find wrong object if multiple exist
[UIReference]
private TextMeshProUGUI winnerText;
```

### 3. Mark Optional References

```csharp
// Good - won't error if missing
[UIReference(Required = false)]
private GameObject optionalDebugPanel;

// Avoid - will error if not found
[UIReference]
private GameObject optionalDebugPanel;
```

### 4. Use UIRoot for Complex Hierarchies

```csharp
public class ComplexUIManager : MonoBehaviour
{
    [UIRoot]
    [SerializeField] private Transform uiCanvas;

    // All searches will start from uiCanvas
    [UIReference] private Button button1;
    [UIReference] private Button button2;
}
```

### 5. Validate Before Build

Always run validation before building:
1. Open UI Reference Tool window
2. Click "Validate All"
3. Fix any errors before building

### 6. Combine with SerializeField for Flexibility

```csharp
// Keep important references serialized for Inspector visibility
[UIReference]
[SerializeField] private Button criticalButton;

// Use UIReference alone for auto-managed references
[UIReference] private TextMeshProUGUI tempText;
```

---

## Troubleshooting

### "Required reference is null" Error

**Causes:**
1. GameObject name doesn't match field name
2. GameObject is in a different hierarchy
3. GameObject doesn't exist in scene

**Solutions:**
- Verify GameObject name matches field name (case-sensitive)
- Use `Path` parameter for nested objects
- Use `Name` parameter if GameObject has different name
- Check `SearchInChildren = false` if object is outside hierarchy

### Reference Not Found

**Check:**
1. GameObject is active in scene
2. GameObject name matches exactly (case-sensitive)
3. Path is correct if using Path parameter
4. Component type matches field type

### Multiple Matches Found

The binder will use the first match found. To be more specific:
- Use the `Path` parameter for exact hierarchy
- Use the `Tag` parameter to filter by tag
- Make GameObject names unique

### References Lost After Scene Reload

This happens if using runtime binding without persistence. Solutions:
- Use `[SerializeField]` along with `[UIReference]`
- Bind references in Editor and save scene
- Use `UIReferenceAutoBinding` component

---

## Migration Guide

### From SerializeField to UIReference

**Step 1:** Add UIReference attribute
```csharp
// Before
[SerializeField] private Button myButton;

// After
[UIReference]
[SerializeField] private Button myButton;  // Keep SerializeField for Inspector visibility
```

**Step 2:** Bind references
- Open UI Reference Tool window
- Click "Bind All"

**Step 3:** Verify
- Click "Validate All"
- Check all references are assigned

**Step 4:** (Optional) Remove SerializeField
```csharp
// If you don't need Inspector visibility
[UIReference] private Button myButton;
```

### From FindObjectOfType to UIReference

**Before:**
```csharp
private DiceModel diceModel;

private void Start()
{
    diceModel = FindObjectOfType<DiceModel>();
}
```

**After:**
```csharp
[UIReference(SearchInChildren = false)]
private DiceModel diceModel;

// No Start method needed!
// Or add UIReferenceAutoBinding component
```

---

## Performance Considerations

- **Editor Binding**: Zero runtime cost - references assigned in Editor
- **Runtime Binding**: Slight cost during Awake/Start, then zero cost
- **Validation**: Editor-only, no runtime cost
- **FindObjectOfType**: Replaced by more efficient Transform.Find when using paths

**Recommendation:** Bind in Editor for best performance, use runtime binding only for dynamic UI.

---

## Advanced Usage

### Custom Binding Logic

```csharp
using LaddersAndSnakes.Core;

public class CustomUIManager : MonoBehaviour
{
    [UIReference] private Button myButton;

    private void Awake()
    {
        // Bind with custom settings
        UIReferenceBinder.BindUIReferences(this, logResults: true);

        // Custom post-binding logic
        if (myButton != null)
        {
            ConfigureButton(myButton);
        }
    }

    private void ConfigureButton(Button button)
    {
        // Custom configuration
        button.interactable = true;
        button.onClick.AddListener(OnButtonClick);
    }
}
```

### Programmatic Validation

```csharp
using LaddersAndSnakes.Core;

public class BuildValidator
{
    [MenuItem("Build/Validate UI Before Build")]
    public static void ValidateBeforeBuild()
    {
        var result = UIReferenceBinder.ValidateAllReferences();

        if (result.HasErrors)
        {
            EditorUtility.DisplayDialog(
                "Build Blocked",
                $"Cannot build: {result.Errors.Count} missing UI references!",
                "OK");
            return;
        }

        // Proceed with build
        BuildPipeline.BuildPlayer(...);
    }
}
```

---

## Support

For issues or questions:
1. Check this documentation
2. Review the example usage in GameUIManager and MainMenuController
3. Open UI Reference Tool window for visual debugging
4. Check console for detailed error messages

---

## Credits

Created for the Ladders & Snakes Unity project.
