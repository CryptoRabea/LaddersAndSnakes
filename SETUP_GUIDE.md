# Ladders and Snakes - Complete Setup Guide

**Welcome!** This guide will take you from zero to playing the game in just a few minutes.

---

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start---getting-the-game-running)
3. [First-Time Setup](#first-time-setup-detailed-steps)
4. [How to Play](#how-to-play)
5. [Configuration](#configuration)
6. [Troubleshooting](#troubleshooting)
7. [Advanced Features](#advanced-configuration)

---

## Prerequisites

### System Requirements
- **Operating System**: Windows 10/11, macOS 10.15+, or Linux (Ubuntu 20.04+)
- **RAM**: Minimum 8GB (16GB recommended)
- **Disk Space**: ~10GB for Unity + project
- **Graphics**: DirectX 11/12 compatible GPU

### Software Required

#### 1. Unity Hub (Required)
Download and install Unity Hub:
- Go to: https://unity.com/download
- Download **Unity Hub** for your operating system
- Install Unity Hub following the installer instructions

#### 2. Unity Editor 6 (2023.2+)
Once Unity Hub is installed:
1. Open **Unity Hub**
2. Go to **Installs** tab
3. Click **Install Editor**
4. Select **Unity 6 (6000.0.23f1)** or later (2023.2+ compatible versions work)
5. During installation, include these modules:
   - ✅ **Microsoft Visual Studio Community** (or use existing IDE)
   - ✅ **Android Build Support** (optional - for mobile builds)
   - ✅ **iOS Build Support** (optional - for iOS builds, macOS only)

#### 3. Git (Required for cloning)
If you don't have Git:
- **Windows**: Download from https://git-scm.com/download/win
- **macOS**: Install via `brew install git` or from https://git-scm.com
- **Linux**: `sudo apt-get install git` (Ubuntu/Debian)

#### 4. Code Editor (Optional but Recommended)
- **Visual Studio** (included with Unity Hub)
- **Visual Studio Code** (https://code.visualstudio.com/)
- **JetBrains Rider** (https://www.jetbrains.com/rider/)

---

## First-Time Setup: Detailed Steps

### Step 1: Clone or Download the Project

**Option A: Using Git (Recommended)**
```bash
# Open terminal/command prompt and run:
git clone <repository-url>
cd LaddersAndSnakes
```

**Option B: Download ZIP**
1. Download the project ZIP from GitHub
2. Extract to a folder (e.g., `C:\Projects\LaddersAndSnakes` or `~/Projects/LaddersAndSnakes`)
3. Remember this location!

### Step 2: Add Project to Unity Hub

1. Open **Unity Hub**
2. Click **"Open"** or **"Add"** button (top-right)
3. Navigate to the `LaddersAndSnakes` folder (the one containing `Assets`, `Packages`, `ProjectSettings`)
4. Click **"Select Folder"** / **"Open"**

**Important**: Unity Hub will show:
- Project name: **LaddersAndSnakes**
- Unity version: **6000.0.23f1** (or similar)
- If version shows in yellow/red, click it to install the correct version

### Step 3: Open the Project

1. In Unity Hub, click on the **LaddersAndSnakes** project
2. Unity Editor will launch (this may take 1-5 minutes on first open)
3. Unity will import all assets and packages

**What's happening?**
- Unity is downloading required packages (Netcode, URP, etc.)
- Asset database is being built
- Scripts are being compiled

**First-time loading may take 3-10 minutes depending on your system!**

### Step 4: Wait for Package Installation

You'll see a progress bar at the bottom of Unity Editor:
- "Importing..."
- "Compiling scripts..."
- Wait until it shows **"Ready"** or the progress bar disappears

**Check Console for Errors:**
- Go to **Window > General > Console** (Ctrl+Shift+C / Cmd+Shift+C)
- Ideally, you should see no red errors
- Warnings (yellow) are usually okay

### Step 5: Open the Game Scene

1. In the **Project** window (bottom panel), navigate to:
   ```
   Assets/Scenes/GameScene.unity
   ```
2. **Double-click** `GameScene.unity` to open it
3. The scene will load in the **Hierarchy** window

### Step 6: Run the Automatic Setup

**This is the magic step that sets everything up!**

1. In Unity's top menu, click: **`LAS > Setup Game Scene`**
2. A dialog will appear showing setup progress
3. Click **"OK"** when you see "Setup Complete!"

**What just happened?**
The setup tool created:
- ✅ GameSetupManager (automatic runtime setup)
- ✅ GameCanvas (UI system)
- ✅ EventSystem (input handling)
- ✅ Main Camera (properly positioned)
- ✅ Directional Light (scene lighting)

### Step 7: Press Play! 🎮

1. Click the **Play** button at the top center of Unity Editor (or press Ctrl+P / Cmd+P)
2. The game will start in the **Game** view
3. You should see:
   - A 10x10 game board
   - Ladders and snakes
   - Player pieces at the start
   - A "Roll Dice" button at the bottom

**Congratulations! The game is now running!** 🎉

---

## Quick Start - Getting the Game Running

### Option 1: Automatic Setup (Recommended)

1. **Open Unity** and load the project
2. **Open the GameScene**: `Assets/Scenes/GameScene.unity`
3. **Run the Setup Tool**:
   - Go to the top menu: `LAS > Setup Game Scene`
   - Click "OK" when the setup completion dialog appears
4. **Press Play!** The game will automatically:
   - Generate the board
   - Create player pieces
   - Setup the dice
   - Initialize all game systems

That's it! You can now play the game by clicking "Roll Dice" button.

### Option 2: Manual Scene Verification

If the automatic setup is already done, just verify these objects exist in the Hierarchy:
- ✅ GameSetupManager
- ✅ GameCanvas (with UI elements)
- ✅ EventSystem
- ✅ Main Camera
- ✅ Directional Light

If any are missing, run the setup tool from `LAS > Setup Game Scene`.

---

## How to Play

1. **Press Play** in Unity
2. The board will automatically generate
3. Click the **"Roll Dice"** button at the bottom of the screen
4. Watch your piece move!
5. When you land on:
   - 🪜 **Ladder** - You climb up!
   - 🐍 **Snake** - You slide down!
6. First player to reach square 100 wins!

---

## Configuration

### Game Settings

You can configure the game by creating a `GameSetupConfig` asset:

1. Right-click in Project window
2. Select `Create > LAS > Game Setup Config`
3. Configure settings:
   - **Player Count**: 2-4 players
   - **Enable AI**: Add AI opponents
   - **Enable Networking**: For multiplayer (experimental)
   - **Use Procedural Board**: Generate random boards
   - **Difficulty Level**: Easy, Medium, Hard, Extreme

### Board Customization

Create custom boards:

1. Right-click in Project window
2. Select `Create > LAS > Board Config`
3. Use the Board Generator Window:
   - Go to `LAS > Board Generator Window`
   - Adjust ladder/snake counts
   - Set difficulty
   - Click "Generate Board"

---

## Project Structure

```
Assets/
├── Scenes/
│   ├── MainMenu.unity        # Main menu (player selection)
│   └── GameScene.unity        # Main game scene
│
├── Scripts/
│   ├── Config/                # All configuration ScriptableObjects
│   │   ├── GameSetupConfig.cs
│   │   ├── BoardConfig.cs
│   │   └── DiceConfig.cs
│   │
│   ├── Gameplay/              # Core game logic
│   │   ├── GameController.cs
│   │   ├── GameSetupManager.cs ⭐ (Auto-setup system)
│   │   └── GameStateMachine.cs
│   │
│   ├── Entities/              # Game entities
│   │   ├── DiceModel.cs
│   │   ├── PlayerPiece.cs
│   │   └── MovementSystem.cs
│   │
│   ├── UI/                    # User interface
│   │   ├── GameUIManager.cs   ⭐ (Handles all UI)
│   │   └── MainMenuController.cs
│   │
│   ├── Editor/                # Unity Editor tools
│   │   ├── GameSceneSetup.cs  ⭐ (Setup automation)
│   │   └── BoardGeneratorWindow.cs
│   │
│   └── Networking/            # Multiplayer support
│       ├── NetworkManager.cs
│       └── MultiplayerGameController.cs
```

---

## Key Features Implemented

### ✅ Complete Game Loop
- Dice rolling with visual feedback
- Player movement with smooth animations
- Ladder climbing and snake sliding
- Win condition detection
- Turn-based gameplay

### ✅ User Interface
- Roll Dice button
- Turn indicator
- Dice result display
- Game over screen with winner announcement
- Play Again / Main Menu options

### ✅ Automatic Setup
- **GameSetupManager**: Automatically creates all game objects at runtime
- **Editor Tool**: One-click scene setup from `LAS > Setup Game Scene`
- **No manual dragging required**: Everything wires up automatically!

### ✅ Multiplayer Support
- Local multiplayer (2-4 players on same device)
- AI opponents
- Network infrastructure (ready for online multiplayer)

### ✅ Configurable Gameplay
- Adjustable player count
- Procedural board generation
- Multiple difficulty levels
- Customizable animations and speeds

### ✅ Board Generation
- Static pre-designed boards
- Procedural generation algorithm
- Difficulty presets (Easy, Medium, Hard, Extreme)
- Board validation system

---

## Gameplay Controls

- **Mouse Click**: Roll Dice button
- **Turn-based**: Only the current player can roll
- **Automatic Movement**: Pieces move automatically after dice roll

---

## Troubleshooting

### Common Unity Issues

#### "Unity Hub doesn't show the correct Unity version"

**Problem**: Project requires Unity 6 (6000.0.23f1+) but you have a different version

**Solution**:
1. In Unity Hub, go to **Installs** tab
2. Click **Install Editor**
3. Select Unity 6 (6000.0.23f1 or later)
4. Complete installation
5. Go back to **Projects** tab
6. Click the version number next to your project
7. Select the newly installed Unity 6 version

#### "Package Manager errors on first load"

**Problem**: Packages fail to download or import

**Solution**:
1. Go to **Window > Package Manager**
2. Click the refresh icon (⟳) in the top-right
3. If errors persist, close Unity
4. Delete the `Library` folder in your project directory
5. Reopen the project in Unity (will re-import everything)

#### "Unity crashes on project open"

**Possible causes**: Corrupted cache, insufficient RAM, graphics drivers

**Solution**:
1. Close Unity completely
2. Delete these folders in the project:
   - `Library/`
   - `Temp/`
   - `obj/`
3. Update graphics drivers
4. Restart your computer
5. Open project again

#### "Script compilation errors"

**Problem**: Red errors in Console about missing namespaces or types

**Solution**:
1. Check **Console** window for specific errors
2. Common fixes:
   - Go to **Assets > Reimport All** (wait for completion)
   - Check if all packages installed: **Window > Package Manager**
   - Ensure these packages are installed:
     - TextMeshPro
     - Unity UI (UGUI)
     - Netcode for GameObjects
     - Universal Render Pipeline
3. If errors about "Netcode" or "Unity.Multiplayer":
   - Open **Package Manager**
   - Search for "Netcode for GameObjects"
   - Install version 2.7.0+

#### "Scenes are empty or pink materials"

**Problem**: Assets not imported correctly

**Solution**:
1. Go to **Edit > Project Settings > Graphics**
2. Check if a Render Pipeline Asset is assigned
3. If not, search for "UniversalRP" in Project window
4. Drag it to the **Scriptable Render Pipeline Settings** field
5. Go to **Assets > Reimport All**

### Game-Specific Issues

### "Nothing happens when I press Play"

**Solution**: Run the setup tool
1. Stop Play mode if running
2. Go to `LAS > Setup Game Scene`
3. Wait for the "Setup Complete" dialog
4. Press Play again

### "No UI visible"

**Check**:
- GameCanvas exists in Hierarchy
- EventSystem exists in Hierarchy
- Canvas is set to "Screen Space - Overlay"

**Fix**: Run `LAS > Setup Game Scene` again

### "Roll Dice button doesn't appear or doesn't work"

**This is a common issue!** See the detailed guide: **[BUTTON_SETUP_GUIDE.md](BUTTON_SETUP_GUIDE.md)**

**Quick Check**:
1. Look in Hierarchy for: `GameCanvas > RollDiceButton`
2. If missing: Re-run `LAS > Setup Game Scene`
3. If exists but not working: Check `GameCanvas` → `GameUIManager` component → verify "Roll Dice Button" field is assigned

**Manual Fix**:
1. Right-click `GameCanvas` → `UI > Button - TextMeshPro`
2. Rename to `RollDiceButton`
3. Drag it to `GameUIManager` → `Roll Dice Button` field in Inspector

See [BUTTON_SETUP_GUIDE.md](BUTTON_SETUP_GUIDE.md) for complete step-by-step instructions.

### "Players don't move"

**Check**:
- MovementSystem exists (created automatically)
- Board squares are generated
- Check Console for errors

**Fix**: Ensure GameSetupManager completed setup (check console logs)

---

## Advanced Configuration

### Creating Custom Boards

1. Open `LAS > Board Generator Window`
2. Configure:
   - Ladder count (min/max)
   - Snake count (min/max)
   - Jump length ranges
   - Balance ratio
3. Click "Generate Board"
4. Save the configuration

### Network Multiplayer Setup

1. Create GameSetupConfig asset
2. Enable "Enable Networking"
3. Set Network Mode:
   - **Host**: Start a server
   - **Client**: Connect to server
4. Set server address (for clients)
5. Start game from MainMenu

**Note**: Current networking is simulated. For real multiplayer, integrate with Netcode for GameObjects or Photon.

---

## Performance Tips

- **Object Pooling**: Dice use pooling system (already implemented)
- **Animation Speed**: Adjust in GameConfig to speed up gameplay
- **Camera Distance**: Adjust for better board view

---

## Next Steps / Future Enhancements

- [ ] Add sound effects and music
- [ ] Create dice 3D models with textures
- [ ] Add player piece models (characters)
- [ ] Implement real network multiplayer backend
- [ ] Add power-ups and special tiles
- [ ] Create multiple board themes
- [ ] Add particle effects for ladders/snakes
- [ ] Implement save/load system
- [ ] Add achievements and leaderboards

---

## Technical Architecture

### Event-Driven Design
The game uses an EventBus for communication:
- `DiceRolledEvent`: Published when dice roll completes
- `MoveRequestedEvent`: Triggers player movement
- `PieceMovedEvent`: When movement completes
- `TurnEndedEvent`: End of player's turn
- `GameOverEvent`: Victory condition met

### State Machine
Game states:
1. **IdleState**: Waiting for dice roll
2. **MovingState**: Player piece moving

### Service Locator
Core services registered:
- EventBus (event publishing)
- PoolManager (object pooling)

### Dependency Injection
Components find dependencies automatically via:
- ServiceLocator
- FindObjectOfType (for Unity components)

---

## Credits

Built with:
- Unity Engine
- C# .NET
- TextMeshPro for UI
- Custom EventBus system
- Procedural generation algorithms

---

## Support

For issues or questions:
1. Check console for error messages
2. Verify scene setup via `LAS > Setup Game Scene`
3. Check this documentation
4. Review code comments in key files

---

---

## Quick Reference Checklist

Use this checklist to verify your setup:

### Before Opening Unity
- [ ] Unity Hub installed
- [ ] Unity 6 (6000.0.23f1+) installed in Unity Hub
- [ ] Project downloaded/cloned to local folder
- [ ] Project added to Unity Hub

### First-Time Project Open
- [ ] Project opened in Unity Editor
- [ ] All packages downloaded (check Package Manager)
- [ ] No red errors in Console (warnings are okay)
- [ ] GameScene.unity opened

### Running the Setup
- [ ] Ran `LAS > Setup Game Scene` from menu
- [ ] Setup completed successfully
- [ ] GameSetupManager exists in Hierarchy
- [ ] GameCanvas exists in Hierarchy
- [ ] EventSystem exists in Hierarchy

### Testing the Game
- [ ] Pressed Play button
- [ ] Game board visible
- [ ] Player pieces visible
- [ ] "Roll Dice" button visible and clickable
- [ ] Dice rolls and players move

---

## Video Tutorial (If Available)

Looking for a video walkthrough? Check the project repository for any video guides or screen recordings that demonstrate the setup process.

---

## Getting Help

If you're still stuck after following this guide:

1. **Check the Console**: Most issues show error messages in Unity Console
2. **Review Recent Commits**: Check if recent changes broke something
3. **Clean and Rebuild**:
   ```bash
   # Close Unity, then delete:
   - Library/
   - Temp/
   - obj/
   # Reopen project
   ```
4. **Check Dependencies**: Verify all packages in `Packages/manifest.json` are installed
5. **Unity Forums**: Search Unity forums for specific error messages
6. **Project Issues**: Report bugs in the GitHub repository

---

## Summary: From Zero to Playing in 10 Minutes

**The absolute essentials:**

1. **Install Unity Hub** → Download Unity 6
2. **Clone project** → Add to Unity Hub → Open
3. **Wait for import** → Open GameScene.unity
4. **Run `LAS > Setup Game Scene`** → Press Play
5. **Click "Roll Dice"** → Have fun!

That's it! Everything else is automatic. The game handles all runtime setup, board generation, and UI creation for you.

---

**Enjoy the game! 🎲🪜🐍**
