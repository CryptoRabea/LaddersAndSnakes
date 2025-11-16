# UI Setup Guide - Roll Dice Button & Game UI

This guide explains how to set up the game UI (Roll Dice button, turn indicator, etc.) in your Ladders and Snakes game.

## Problem

The game doesn't automatically create the Roll Dice button and other UI elements, making it impossible to play the game.

## Solution

We've created **three different ways** to set up the UI. Use whichever is most convenient:

---

## Option 1: Automatic Runtime Setup (RECOMMENDED)

The **RuntimeUIBuilder** component automatically creates all UI elements when the game starts.

### How to Use:

1. **In Unity Editor:**
   - Go to menu: `LAS > Quick UI Setup (Add RuntimeUIBuilder)`
   - Click it
   - Done! The UI will now be created automatically every time you run the game

2. **Alternative - Manual:**
   - Select the `GameCanvas` GameObject in your scene
   - In the Inspector, click "Add Component"
   - Search for "Runtime UI Builder"
   - Add it to the GameCanvas

3. **Play the game** - The UI will be created automatically!

---

## Option 2: Build UI in Editor

You can build the UI once in the Unity Editor and save it with the scene.

### How to Use:

1. **In Unity Editor:**
   - Go to menu: `LAS > Force Build UI Now (Editor)`
   - Click it
   - The UI will be created and saved in your scene

2. **Check the Hierarchy:**
   - Expand `GameCanvas` in the Hierarchy
   - You should see:
     - `RollDiceButton`
     - `TurnIndicator`
     - `DiceResultText`
     - `GameOverPanel`

3. **Save the scene** (Ctrl+S / Cmd+S)

---

## Option 3: Full Scene Setup (Advanced)

This sets up the entire scene from scratch, including board, dice, players, and UI.

### How to Use:

1. **In Unity Editor:**
   - Go to menu: `LAS > Setup Game Scene`
   - Click it
   - The entire scene will be configured

2. **Save the scene** when prompted

---

## What Gets Created

The UI setup creates the following elements:

### 1. **Roll Dice Button**
- Located at the bottom center of the screen
- Click to roll the dice
- Shows "Rolling..." while animating
- Automatically disabled when not your turn (multiplayer)

### 2. **Turn Indicator**
- Located at the top center
- Shows which player's turn it is
- Updates automatically after each turn

### 3. **Dice Result Text**
- Located above the Roll Dice button
- Shows the result of the last dice roll
- Yellow text for visibility

### 4. **Game Over Panel**
- Full-screen overlay (hidden during gameplay)
- Shows winner when game ends
- Contains:
  - Winner announcement
  - "Play Again" button
  - "Main Menu" button

---

## How It Works

### RuntimeUIBuilder Component

```
GameCanvas (GameObject)
├── Canvas (Component)
├── CanvasScaler (Component)
├── GraphicRaycaster (Component)
├── RuntimeUIBuilder (Component) ← Creates all UI at runtime
└── GameUIManager (Component) ← Handles button clicks and updates
```

The `RuntimeUIBuilder`:
- Runs automatically in `Awake()`
- Checks if UI already exists (won't duplicate)
- Creates all UI elements programmatically
- Wires up references to `GameUIManager`
- Works in both Editor and Build

### GameUIManager Component

The `GameUIManager` handles all UI interactions:
- Listens for button clicks
- Updates text displays
- Shows/hides game over screen
- Checks if it's the player's turn
- Communicates with the game via Event Bus

---

## Troubleshooting

### Issue: Button doesn't appear when I run the game

**Solution:**
1. Make sure `GameCanvas` exists in your scene
2. Run: `LAS > Quick UI Setup (Add RuntimeUIBuilder)`
3. Try running the game again

### Issue: Button appears but doesn't work

**Solution:**
1. Check the Console for errors
2. Make sure `DiceModel` exists in the scene
3. Make sure `GameController` exists in the scene
4. The `GameSetupManager` should create these automatically

### Issue: No EventSystem in scene error

**Solution:**
The `RuntimeUIBuilder` creates an EventSystem automatically. If you still see this error:
1. Go to: `GameObject > UI > Event System` in Unity Editor
2. This will create an EventSystem manually

### Issue: Button is visible but grayed out (not clickable)

**Possible Causes:**
1. **Dice is still rolling** - Wait for the animation to finish
2. **Not your turn (multiplayer)** - Wait for the other player
3. **Game is over** - The button is disabled after someone wins
4. **Missing DiceModel reference** - Check the `GameUIManager` component

---

## Technical Details

### File Locations

```
Assets/
├── Scripts/
│   ├── UI/
│   │   ├── GameUIManager.cs (Handles UI logic)
│   │   ├── RuntimeUIBuilder.cs (Creates UI at runtime)
│   │   └── UIAutoInitializer.cs (Failsafe - adds builder if missing)
│   └── Editor/
│       ├── GameSceneSetup.cs (Full scene setup tool)
│       └── QuickUISetup.cs (Quick menu tools for UI)
└── Scenes/
    └── GameScene.unity
```

### Event Flow

```
User clicks "Roll Dice" button
    ↓
GameUIManager.OnRollDiceClicked()
    ↓
DiceModel.Roll()
    ↓
DiceRolledEvent published via EventBus
    ↓
GameController receives event
    ↓
MovementSystem moves player piece
    ↓
GameController.EndTurn()
    ↓
TurnEndedEvent published
    ↓
GameUIManager updates UI for next player
```

---

## Quick Reference

| Action | Menu Command |
|--------|-------------|
| Add automatic UI builder | `LAS > Quick UI Setup (Add RuntimeUIBuilder)` |
| Build UI in editor | `LAS > Force Build UI Now (Editor)` |
| Setup entire scene | `LAS > Setup Game Scene` |

---

## For Developers

If you want to customize the UI:

1. **Edit the UI layout:** Modify `RuntimeUIBuilder.cs`
   - Change positions, sizes, colors in the `Create...()` methods

2. **Add new UI elements:**
   - Add creation method in `RuntimeUIBuilder.cs`
   - Add field to `GameUIManager.cs`
   - Wire up reference in `WireUIManager()`

3. **Change button behavior:** Edit `GameUIManager.cs`
   - Modify `OnRollDiceClicked()` for button logic
   - Update `UpdateDiceButton()` for button state

---

## Summary

**Easiest Solution:** Use `LAS > Quick UI Setup (Add RuntimeUIBuilder)` from the Unity menu

This will ensure your game always has the UI it needs to function properly!
