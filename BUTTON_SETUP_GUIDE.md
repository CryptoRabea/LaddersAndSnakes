# Roll Dice Button Setup Guide

## The Issue
You ran the setup tool but don't see the "Roll Dice" button.

## How It's Supposed to Work

When you run `LAS > Setup Game Scene`, the setup tool **automatically creates**:

```
GameCanvas
├── RollDiceButton (Button component)
│   └── Text (TextMeshProUGUI: "Roll Dice")
├── TurnIndicator (TextMeshProUGUI: "Player 1's Turn")
├── DiceResultText (TextMeshProUGUI: "Rolled: -")
└── GameOverPanel (initially hidden)
    ├── WinnerText
    ├── PlayAgainButton
    └── MainMenuButton
```

The setup tool also **automatically wires** these references to the `GameUIManager` component.

---

## Troubleshooting: Button Not Visible

### Step 1: Check if the Button Exists

1. In Unity, look at the **Hierarchy** window
2. Expand `GameCanvas`
3. Look for `RollDiceButton`

**If you see it:**
- Click on it and check the **Inspector**
- Verify it has:
  - `RectTransform` component
  - `Image` component (blue color)
  - `Button` component
- Check if it's positioned correctly:
  - Should be at bottom-center of screen
  - Position: `anchoredPosition = (0, 40)`

**If you DON'T see it:**
- The setup tool didn't run properly
- Go to Step 2

### Step 2: Re-run the Setup Tool

1. In Unity top menu: `LAS > Setup Game Scene`
2. Wait for "Setup Complete!" dialog
3. Click OK
4. Check the **Console** (Window > General > Console) for any errors

### Step 3: Check Canvas Settings

1. Select `GameCanvas` in Hierarchy
2. In Inspector, verify:
   - **Canvas** component: `Render Mode = Screen Space - Overlay`
   - **Canvas Scaler**: `UI Scale Mode = Scale With Screen Size`
   - **Reference Resolution**: `1920 x 1080`

3. Make sure `EventSystem` exists in the Hierarchy (required for button clicks)

### Step 4: Check Camera

If the button exists but you can't see it in Game view:

1. Click the **Game** tab (not Scene tab)
2. Make sure you're looking at the Game view when testing
3. The button should be at the bottom center

---

## Manual Setup: Create Button Yourself

If the automatic setup isn't working, you can create the button manually:

### 1. Create the Button GameObject

1. Right-click on `GameCanvas` in Hierarchy
2. Select `UI > Button - TextMeshPro`
3. Rename it to `RollDiceButton`

### 2. Configure the Button

Select `RollDiceButton` and set these properties in **Rect Transform**:

- **Anchors**:
  - Min: `(0.5, 0)`
  - Max: `(0.5, 0)`
- **Pivot**: `(0.5, 0)`
- **Pos X**: `0`
- **Pos Y**: `40`
- **Width**: `200`
- **Height**: `60`

### 3. Set Button Color

In the **Image** component:
- **Color**: Set to blue `RGB(51, 153, 255)` or any color you like

### 4. Edit Button Text

1. Expand `RollDiceButton` in Hierarchy
2. Click on the **Text (TMP)** child object
3. In **TextMeshPro** component:
   - **Text**: `Roll Dice`
   - **Font Size**: `24`
   - **Alignment**: Center (horizontally and vertically)
   - **Color**: White

### 5. Wire Up to GameUIManager

**This is the critical step!**

1. Select `GameCanvas` in Hierarchy
2. Look at the **GameUIManager** component in Inspector
3. You'll see these fields:
   ```
   [UI References]
   - Roll Dice Button: [empty]
   - Turn Indicator Text: [empty]
   - Dice Result Text: [empty]
   - Game Over Panel: [empty]
   - Winner Text: [empty]
   - Play Again Button: [empty]
   - Main Menu Button: [empty]
   ```

4. **Drag `RollDiceButton`** from Hierarchy into the **Roll Dice Button** field

5. Do the same for other UI elements if they exist:
   - `TurnIndicator` → **Turn Indicator Text**
   - `DiceResultText` → **Dice Result Text**
   - `GameOverPanel` → **Game Over Panel**
   - etc.

---

## Verify It Works

1. Press **Play**
2. Look at the **Game** view (not Scene view)
3. You should see:
   - "Player 1's Turn" at the top
   - "Roll Dice" button at the bottom
4. Click the button
5. Check the **Console** for:
   ```
   [DiceModel] Rolling dice...
   [DiceModel] Result: 5
   ```

---

## Common Issues

### "Button exists but doesn't do anything when clicked"

**Check:**
1. `EventSystem` exists in Hierarchy
2. `GameUIManager` has the button reference assigned
3. `DiceModel` exists in the scene (created at runtime by GameSetupManager)
4. Check Console for errors

**Fix:**
- Run `LAS > Setup Game Scene` again
- Manually wire up the button reference (see Manual Setup step 5)

### "Button is tiny or in wrong position"

**Check:**
- Canvas is set to "Screen Space - Overlay"
- Canvas Scaler reference resolution is 1920x1080
- RectTransform anchors are set correctly (see Manual Setup step 2)

### "Multiple errors in Console"

**Most common errors:**
- `NullReferenceException: Object reference not set`
  - This means a reference is missing
  - Check GameUIManager has all references assigned

- `DiceModel not found`
  - Press Play first (GameSetupManager creates it at runtime)
  - Or run the setup tool again

---

## Quick Fix Script

If you want to quickly check what's wrong, run this in the Console while in Play mode:

1. Window > General > Console
2. Look for errors
3. Common fixes:
   - **No DiceModel**: Let the game run for 1-2 seconds (GameSetupManager creates it)
   - **Button not wired**: Stop Play, wire up manually (Manual Setup step 5)
   - **No EventSystem**: Run setup tool again

---

## The Code Reference

For developers: The automatic button creation happens in:
- **File**: `Assets/Scripts/Editor/GameSceneSetup.cs`
- **Method**: `CreateDiceButton()` (line 237)
- **Wiring**: `WireUIReferences()` (line 514)

The button functionality is in:
- **File**: `Assets/Scripts/UI/GameUIManager.cs`
- **Method**: `OnRollDiceClicked()` (line 87)

---

## Summary

**Automatic Way** (Recommended):
1. `LAS > Setup Game Scene`
2. Press Play
3. Done!

**Manual Way** (If automatic fails):
1. Create Button via `UI > Button - TextMeshPro`
2. Set position and size
3. Drag to `GameUIManager` reference
4. Press Play

The button **calls** `GameUIManager.OnRollDiceClicked()` which **calls** `DiceModel.Roll()`.

If you're still stuck, check the Console for specific error messages!
