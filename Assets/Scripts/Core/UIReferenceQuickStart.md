# UI Reference Tool - Quick Start Guide

## 5-Minute Setup

### Step 1: Add the Attribute (30 seconds)

Replace your current field declarations:

```csharp
// OLD WAY ❌
[SerializeField] private Button myButton;

// NEW WAY ✅
[UIReference] private Button myButton;
```

### Step 2: Bind References (30 seconds)

Open Unity Editor, then:
1. Go to `Window > Ladders & Snakes > UI Reference Tool`
2. Click **"Bind All"**
3. Done! All references are now assigned.

### Step 3: Save Your Scene (10 seconds)

Save your scene to persist the bindings.

---

## Common Scenarios

### Scenario 1: Simple UI Manager

```csharp
using LaddersAndSnakes.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [UIReference] private Button playButton;
    [UIReference] private TextMeshProUGUI scoreText;
    [UIReference] private GameObject pausePanel;

    // That's it! Just add [UIReference] attribute
    // Then use the UI Reference Tool window to bind
}
```

**How to bind:**
- `Window > Ladders & Snakes > UI Reference Tool > Bind All`

### Scenario 2: Nested UI Elements

```csharp
public class MenuUI : MonoBehaviour
{
    // For nested UI, use Path parameter
    [UIReference(Path = "Canvas/MainPanel/PlayButton")]
    private Button playButton;

    [UIReference(Path = "Canvas/SettingsPanel/VolumeSlider")]
    private Slider volumeSlider;
}
```

### Scenario 3: Optional Elements

```csharp
public class DebugUI : MonoBehaviour
{
    // Required (default) - will error if not found
    [UIReference]
    private Button mainButton;

    // Optional - won't error if not found
    [UIReference(Required = false)]
    private GameObject debugPanel;
}
```

### Scenario 4: Runtime Binding

For UI created at runtime:

```csharp
public class DynamicUI : MonoBehaviour
{
    [UIReference] private Button dynamicButton;

    private void Awake()
    {
        // Bind at runtime
        UIReferenceBinder.BindUIReferences(this);
    }
}
```

**OR** add the `UIReferenceAutoBinding` component:
- Right-click GameObject → `UI > Add UI Reference Auto-Binding`
- No code needed!

---

## Workflow Comparison

### Old Workflow (Manual)

1. Add `[SerializeField]` to field
2. Open Unity Inspector
3. Drag GameObject to field
4. Repeat for every field
5. If GameObject renamed → reference breaks
6. If hierarchy changes → reference breaks

**Time:** ~30 seconds per field

### New Workflow (Automatic)

1. Add `[UIReference]` to field
2. Open UI Reference Tool
3. Click "Bind All"
4. Done!

**Time:** ~5 seconds for ALL fields

---

## Menu Shortcuts

| Action | Menu Location |
|--------|---------------|
| **Open UI Reference Tool** | `Window > Ladders & Snakes > UI Reference Tool` |
| **Bind All References** | `Tools > Ladders & Snakes > Bind All UI References in Scene` |
| **Validate References** | `Tools > Ladders & Snakes > Validate All UI References` |
| **Bind Selected Only** | `Tools > Ladders & Snakes > Bind Selected GameObject UI References` |

---

## Keyboard Shortcuts (Optional Setup)

You can add keyboard shortcuts in Unity:
1. `Edit > Shortcuts`
2. Search for "Ladders & Snakes"
3. Assign shortcuts:
   - Bind All: `Ctrl+Shift+B`
   - Validate: `Ctrl+Shift+V`

---

## Troubleshooting

### "Reference not found" error?

1. Check GameObject name matches field name exactly (case-sensitive)
2. For nested objects, use `Path` parameter:
   ```csharp
   [UIReference(Path = "Canvas/Panel/Button")]
   private Button myButton;
   ```

### Multiple objects with same name?

Use a path to be specific:
```csharp
[UIReference(Path = "MainMenu/PlayButton")]
private Button playButton;

[UIReference(Path = "PauseMenu/PlayButton")]
private Button resumeButton;
```

### Reference lost after scene reload?

Make sure to save the scene after binding:
- `File > Save` or `Ctrl+S`

---

## Best Practices

✅ **DO:**
- Use descriptive field names matching GameObject names
- Use `Path` for nested UI elements
- Mark optional references with `Required = false`
- Bind in Editor for best performance
- Validate before building

❌ **DON'T:**
- Use generic names like `button1`, `text1`
- Forget to save scene after binding
- Skip validation before building

---

## Next Steps

1. ✅ Replace `[SerializeField]` with `[UIReference]` on your existing UI managers
2. ✅ Open UI Reference Tool and click "Bind All"
3. ✅ Save your scene
4. ✅ Read full documentation: `UIReferenceToolREADME.md`
5. ✅ Check example: `ExampleUIManager.cs`

---

## Need Help?

- **Full Documentation:** See `UIReferenceToolREADME.md`
- **Example Code:** See `Assets/Scripts/Examples/ExampleUIManager.cs`
- **UI Tool:** `Window > Ladders & Snakes > UI Reference Tool`

Happy binding! 🎮
