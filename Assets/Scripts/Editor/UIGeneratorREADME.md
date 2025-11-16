# UI Element Generator - Documentation

## Overview

The UI Element Generator is a powerful Unity editor tool that automatically creates UI GameObjects in your scene. It works seamlessly with the UI Reference system to provide a complete workflow from code to visual UI.

## Key Features

- **Quick Create**: Click buttons to instantly create UI elements (Button, Text, Panel, Image, Slider, Toggle, InputField, Dropdown)
- **Auto-Generate from Code**: Automatically create UI elements based on `[UIReference]` attributes in your scripts
- **Templates**: One-click creation of complete UI layouts (Pause Menu, Game HUD, Settings Panel, etc.)
- **Smart Placement**: Automatic parent management and positioning
- **Undo Support**: Full undo/redo integration for all operations

---

## Quick Start

### Method 1: Use the UI Generator Window

1. Open `Window > Ladders & Snakes > UI Generator`
2. Click any button to create that UI element
3. Created elements appear in the scene instantly

### Method 2: Auto-Generate from Script

1. Add `[UIReference]` attributes to your script:
```csharp
using LaddersAndSnakes.Core;

public class MyUI : MonoBehaviour
{
    [UIReference] private Button playButton;
    [UIReference] private TextMeshProUGUI scoreText;
    [UIReference] private GameObject pausePanel;
}
```

2. Select the GameObject with this script in the hierarchy
3. Click **"Generate UI"** button in the Inspector
4. All UI elements are created automatically!
5. Click **"Bind References"** to connect them

### Method 3: Use Menu Items

- `Tools > Ladders & Snakes > Generate UI from Selected Component`

---

## Detailed Usage

### UI Generator Window

Access via: `Window > Ladders & Snakes > UI Generator`

#### Settings Section

- **Parent Transform**: Where to create UI elements (leave empty for Canvas)
- **Create in Canvas**: Automatically create/use Canvas as parent
- **Auto-Select Created**: Select newly created elements automatically
- **Custom Position**: Specify exact position for created elements

#### Quick Create Section

Click any button to create that UI element:

| Element | Description |
|---------|-------------|
| **Button** | Button with TextMeshPro text child |
| **Text** | TextMeshProUGUI text element |
| **Panel** | Panel with Image component |
| **Image** | Image component |
| **Slider** | Fully configured Slider with fill and handle |
| **Toggle** | Toggle with checkmark and label |
| **Input Field** | TMP_InputField with placeholder |
| **Dropdown** | TMP_Dropdown with template |
| **Canvas** | Creates Canvas with EventSystem |

#### Auto-Generate Section

1. Drag a MonoBehaviour component into the "Component" field
2. The window shows how many UI elements will be created
3. Click "Generate X UI Elements"
4. Optionally bind references immediately

#### Templates Section

One-click creation of complete UI layouts:

| Template | Contents |
|----------|----------|
| **Pause Menu** | Panel with Title, Resume/Settings/MainMenu buttons |
| **Game HUD** | Transparent panel with Score, Timer, Pause button |
| **Dialog Box** | Panel with Title, Message, OK/Cancel buttons |
| **Main Menu** | Panel with Title, Play/Settings/Credits/Quit buttons |
| **Settings Panel** | Panel with Volume slider, toggles, quality dropdown |
| **Game Over Panel** | Panel with Game Over text, score, Play Again/Main Menu buttons |

---

## Auto-Generation from UIReference Attributes

### Basic Example

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LaddersAndSnakes.Core;

public class GameUI : MonoBehaviour
{
    // These will be auto-generated!
    [UIReference] private Button rollDiceButton;
    [UIReference] private TextMeshProUGUI turnText;
    [UIReference] private GameObject gameOverPanel;
}
```

**Steps:**
1. Select GameObject with this script
2. In Inspector, click **"Generate UI"**
3. Creates: RollDiceButton, TurnText, GameOverPanel
4. Click **"Bind References"** to connect them
5. Done!

### Advanced Example with Paths

```csharp
public class ComplexUI : MonoBehaviour
{
    // Basic elements (created at root)
    [UIReference] private Button mainButton;

    // Nested elements (creates path if needed)
    [UIReference(Path = "MenuPanel/Header")] private TextMeshProUGUI titleText;
    [UIReference(Path = "MenuPanel/Content")] private Button playButton;
    [UIReference(Path = "MenuPanel/Content")] private Button quitButton;

    // Optional elements
    [UIReference(Required = false)] private GameObject debugPanel;

    // Custom names
    [UIReference(Name = "BtnStart")] private Button startButton;
}
```

This creates:
```
Canvas/
├── MainButton
├── BtnStart
└── MenuPanel/
    ├── Header/
    │   └── TitleText
    └── Content/
        ├── PlayButton
        └── QuitButton
```

---

## Type Detection

The generator creates appropriate UI elements based on field type:

| Field Type | Generated Element |
|------------|-------------------|
| `Button` | Button with text |
| `TextMeshProUGUI` | Text element |
| `Image` | Image |
| `Slider` | Slider with fill and handle |
| `Toggle` | Toggle with checkmark |
| `TMP_InputField` | Input field with placeholder |
| `TMP_Dropdown` | Dropdown with template |
| `GameObject` | Empty panel |

---

## Workflow Examples

### Workflow 1: Code-First Approach

1. Write your UI manager script with `[UIReference]` attributes
2. Attach script to GameObject
3. Generate UI from script (one click)
4. Bind references (one click)
5. Customize visual appearance in scene
6. Write UI logic in script

### Workflow 2: Template-Based Approach

1. Create UI from template (e.g., Pause Menu)
2. Customize the generated UI
3. Write script with matching `[UIReference]` attributes
4. Bind references

### Workflow 3: Manual + Auto Hybrid

1. Manually create some UI elements
2. Write script with `[UIReference]` for remaining elements
3. Generate missing elements
4. Bind all references

---

## Integration with UI Reference System

The UI Generator works perfectly with the UI Reference Tool:

```csharp
[UIReference] private Button myButton;
```

**Complete Workflow:**
1. **Generate**: Click "Generate UI" → Creates MyButton GameObject
2. **Bind**: Click "Bind References" → Connects myButton field to MyButton GameObject
3. **Use**: Access myButton in code
4. **Validate**: Click "Validate" → Ensures everything is connected

---

## Menu Items

### Tools Menu

- `Tools > Ladders & Snakes > Generate UI from Selected Component`
  - Generates UI for the selected GameObject's first component with UIReference attributes

### Inspector Context Menu

When you select a MonoBehaviour with `[UIReference]` attributes, the Inspector shows:

- **Generate UI** - Create all UI elements
- **Bind References** - Connect generated elements to fields
- **Validate References** - Check everything is connected

---

## Tips & Best Practices

### 1. Name Your Fields Descriptively

```csharp
// Good - clear names
[UIReference] private Button rollDiceButton;
[UIReference] private TextMeshProUGUI playerNameText;

// Avoid - generic names
[UIReference] private Button button1;
[UIReference] private TextMeshProUGUI text1;
```

The generator uses field names to create GameObject names:
- `rollDiceButton` → Creates "RollDiceButton"
- `playerNameText` → Creates "PlayerNameText"

### 2. Use Paths for Organization

```csharp
[UIReference(Path = "MenuPanel/Header")] private TextMeshProUGUI title;
[UIReference(Path = "MenuPanel/Content")] private Button playButton;
[UIReference(Path = "MenuPanel/Footer")] private TextMeshProUGUI version;
```

This creates organized hierarchy automatically!

### 3. Start with Templates

For common UI patterns, use templates:
- Don't recreate Pause Menus manually every time
- Use the template and customize

### 4. Generate → Bind Workflow

Always generate first, then bind:
1. Click "Generate UI"
2. Check created elements in scene
3. Customize if needed (colors, positions, etc.)
4. Click "Bind References"

### 5. Use Custom Names When Needed

```csharp
// If your GameObject is named differently than the field
[UIReference(Name = "BtnPlay")] private Button playButton;
```

---

## Customization

### Modify Created Elements

After generation, you can customize:
- Colors and sprites (in Inspector)
- Positions and sizes (in RectTransform)
- Fonts and text (in TextMeshPro)

Changes are preserved - regenerating won't override existing elements!

### Default Sizes

Default sizes (can be changed in code):
- Button: 160x40
- Text: 160x30
- Panel: 400x300
- Image: 100x100
- Slider: 160x20

---

## Troubleshooting

### "No UI elements were generated"

**Cause**: No `[UIReference]` attributes found

**Solution**: Add `[UIReference]` to your fields:
```csharp
[UIReference] private Button myButton;
```

### Elements Created in Wrong Place

**Cause**: Parent Transform setting

**Solution**:
- Check "Parent Transform" in UI Generator window
- Enable "Create in Canvas" for UI elements
- Clear "Parent Transform" field to use Canvas

### Wrong Element Type Created

**Cause**: Field type doesn't match desired element

**Solution**:
- For Button: Use `Button` type
- For Text: Use `TextMeshProUGUI` type
- For Panel: Use `GameObject` type

### Elements Already Exist

The generator checks if elements exist before creating. If an element with the same name exists, it won't be recreated.

---

## API Reference

### UIElementGenerator

Static utility class for creating UI elements programmatically.

```csharp
using LaddersAndSnakes.Editor;

// Create individual elements
GameObject button = UIElementGenerator.CreateButton("MyButton", parentTransform);
GameObject text = UIElementGenerator.CreateText("MyText", parentTransform);
GameObject panel = UIElementGenerator.CreatePanel("MyPanel", parentTransform);

// Generate from component
List<GameObject> created = UIElementGenerator.GenerateUIFromComponent(myComponent);

// Get or create Canvas
GameObject canvas = UIElementGenerator.GetOrCreateCanvas();
```

### Available Creation Methods

- `CreateButton(name, parent, position)`
- `CreateText(name, parent, position)`
- `CreatePanel(name, parent, position, size)`
- `CreateImage(name, parent, position)`
- `CreateSlider(name, parent, position)`
- `CreateToggle(name, parent, position)`
- `CreateInputField(name, parent, position)`
- `CreateDropdown(name, parent, position)`

---

## Examples

### Example 1: Simple Game HUD

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LaddersAndSnakes.Core;

public class GameHUD : MonoBehaviour
{
    [UIReference] private TextMeshProUGUI scoreText;
    [UIReference] private TextMeshProUGUI livesText;
    [UIReference] private Button pauseButton;

    private void Start()
    {
        // References already connected via "Generate UI" + "Bind References"
        pauseButton.onClick.AddListener(OnPauseClicked);
        UpdateScore(0);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    private void OnPauseClicked()
    {
        Debug.Log("Pause clicked!");
    }
}
```

**Steps:**
1. Attach script to GameObject
2. Click "Generate UI" in Inspector
3. Click "Bind References"
4. Done! All UI elements created and connected

### Example 2: Complex Menu System

```csharp
public class MenuSystem : MonoBehaviour
{
    [Header("Main Menu")]
    [UIReference(Path = "MainPanel")] private GameObject mainPanel;
    [UIReference(Path = "MainPanel/Title")] private TextMeshProUGUI titleText;
    [UIReference(Path = "MainPanel")] private Button playButton;
    [UIReference(Path = "MainPanel")] private Button settingsButton;
    [UIReference(Path = "MainPanel")] private Button quitButton;

    [Header("Settings Panel")]
    [UIReference(Path = "SettingsPanel")] private GameObject settingsPanel;
    [UIReference(Path = "SettingsPanel")] private Slider volumeSlider;
    [UIReference(Path = "SettingsPanel")] private Toggle musicToggle;
    [UIReference(Path = "SettingsPanel")] private Button backButton;

    private void Start()
    {
        // Setup main menu
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // Setup settings
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        backButton.onClick.AddListener(OnBackClicked);

        // Initial state
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    private void OnSettingsClicked()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // ... more methods
}
```

---

## See Also

- **UI Reference Tool**: For binding references (Window > Ladders & Snakes > UI Reference Tool)
- **UIReferenceToolREADME.md**: Complete UI Reference documentation
- **UIReferenceQuickStart.md**: Quick start guide
- **ExampleUIManager.cs**: Example usage

---

## Support

Created for Ladders & Snakes Unity project.

For help:
1. Check this documentation
2. See example scripts in `Assets/Scripts/Examples/`
3. Use the UI Generator window for visual workflow
