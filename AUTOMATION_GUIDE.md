# Snakes and Ladders - Automation Guide

## 🚀 Quick Start - One-Click Setup

The fastest way to get a fully working game scene is to use the automated scene builder:

### Option 1: Complete All-in-One Setup (Recommended)

1. Open Unity Editor
2. Go to **Tools → Snakes and Ladders → Quick Setup → Complete Setup (All-in-One)**
3. Click "Yes, Setup Everything!"
4. Wait for the process to complete
5. Open the GameScene (Tools → Snakes and Ladders → Open Scenes → Open GameScene)
6. Select the "Board" GameObject in the hierarchy
7. In the Inspector, click the **"Generate Board"** button
8. Save the scene (Ctrl+S / Cmd+S)
9. Press **Play** to test!

**That's it!** You now have a fully functional Snakes and Ladders game.

---

## 📋 What Gets Created Automatically

The automation system creates:

### 1. **Directory Structure**
```
Assets/
├── Resources/
├── Prefabs/
├── Scenes/
├── ScriptableObjects/
├── Materials/
└── Animations/
```

### 2. **ScriptableObject Configs**
- **GameConfig.asset** - Board size (100), move speed (4.0), animation curves
- **DiceConfig.asset** - Dice sides (6), roll duration (1.2s), spin torque (10)
- **BoardConfig.asset** - Snake/ladder mappings (auto-populated by BoardGenerator)

### 3. **Player Prefabs** (4 colored spheres)
- Player1_Red.prefab (Red sphere with PlayerPiece component)
- Player2_Blue.prefab (Blue sphere)
- Player3_Green.prefab (Green sphere)
- Player4_Yellow.prefab (Yellow sphere)

### 4. **Complete GameScene**
```
GameScene
├── Main Camera (positioned at 5, 15, -10, rotated 45°)
├── Directional Light
├── GameManager
│   ├── ServiceLocator
│   ├── EventBus
│   ├── PoolManager
│   ├── GameController
│   ├── PlayerManager (with player prefabs assigned)
│   ├── MovementSystem (with GameConfig)
│   ├── BoardManager (with BoardConfig)
│   ├── BoardModel
│   └── GameStateMachine
├── Board
│   └── BoardGenerator (ready to generate 10x10 grid)
├── Dice (cube with DiceController, DiceView, DiceModel)
├── Canvas (Screen Space Overlay)
│   ├── GameHUD
│   │   ├── Current Player Text
│   │   ├── Position Text
│   │   ├── Turn Text
│   │   ├── Dice Result Text
│   │   ├── Roll Dice Button
│   │   └── Event Log
│   └── GameOverPanel (initially hidden)
│       ├── Victory Text
│       ├── Restart Button
│       └── Main Menu Button
└── GameInitializer
```

---

## 🛠️ Automation Tools Menu

All automation tools are accessible from the Unity menu bar:

### **Tools → Snakes and Ladders → Quick Setup**

| Menu Item | Description |
|-----------|-------------|
| **Complete Setup (All-in-One)** | Creates everything: directories, configs, prefabs, and scene |
| **1. Create Directories** | Creates all required folder structure |
| **2. Create ScriptableObjects Only** | Creates GameConfig, DiceConfig, BoardConfig |
| **3. Create Player Prefabs Only** | Creates 4 colored player piece prefabs |
| **4. Setup Build Settings** | Adds all scenes to build settings |

### **Tools → Snakes and Ladders → Open Scenes**

| Menu Item | Description |
|-----------|-------------|
| **Open GameScene** | Quickly open GameScene.unity |
| **Open MainMenu** | Quickly open MainMenu.unity (if exists) |

### **Tools → Snakes and Ladders → Utilities**

| Menu Item | Description |
|-----------|-------------|
| **Clear All Generated Assets** | Deletes all ScriptableObjects, Prefabs, and Scenes (⚠️ DANGEROUS) |

### **Tools → Snakes and Ladders → Help**

| Menu Item | Description |
|-----------|-------------|
| **View Documentation** | Opens README.md |
| **View Implementation Guide** | Opens IMPLEMENTATION_GUIDE.md |

---

## 🎮 Board Generation

The **BoardGenerator** component has a custom editor with an automated "Generate Board" button.

### How to Generate the Board:

1. Select the **Board** GameObject in the hierarchy
2. In the Inspector panel, find the **BoardGenerator** component
3. Configure settings (optional):
   - **Rows**: 10 (default)
   - **Columns**: 10 (default)
   - **Tile Size**: 1.5 (default)
   - **Snake Count**: 5 (default)
   - **Ladder Count**: 5 (default)
4. Click the **"Generate Board"** button
5. The board will be created with:
   - 100 tiles (10x10 grid) with checkerboard pattern
   - Tile numbers (0-99) displayed on each tile
   - 5 snakes (red cylinders connecting higher to lower tiles)
   - 5 ladders (green cylinders connecting lower to higher tiles)
   - Snake/ladder endpoint labels for easy identification

### Board Features:

- **Checkerboard Pattern**: Alternating gray/white tiles for visual clarity
- **Snake Layout**: Red cylinders with labels showing "S{head}" and "{tail}"
- **Ladder Layout**: Green cylinders with labels showing "L{bottom}" and "{top}"
- **Tile Numbering**: Each tile shows its index (0-99)
- **Auto-Config Update**: BoardConfig ScriptableObject is automatically updated

### Regenerating the Board:

If you want to regenerate the board with different snakes/ladders:

1. Select the Board GameObject
2. Click **"Clear Existing Board"** button
3. Adjust the snake/ladder counts if desired
4. Click **"Generate Board"** button again

---

## 🎯 Auto Game Starter (Runtime)

The **AutoGameStarter** component can be added to automatically start the game when the scene loads.

### How to Use:

1. Select the **GameManager** GameObject
2. Add Component → **Auto Game Starter**
3. Configure settings:
   - **Auto Start Game**: ✓ (checked)
   - **Number Of Players**: 2-4
   - **Start Delay**: 0.5 seconds (default)
   - **Auto Generate Board**: ✓ (checked)

### Features:

- **Automatic Game Start**: Game begins automatically after a short delay
- **Configurable Players**: Set number of players (2-4)
- **Board Auto-Generation**: Generates board at runtime (if not already generated)
- **Manual Start**: Public method `ManualStartGame()` for UI button integration

### Public Methods:

```csharp
// Start the game manually
autoGameStarter.ManualStartGame();

// Restart the current scene
autoGameStarter.RestartGame();

// Load a different scene
autoGameStarter.LoadScene("MainMenu");
```

---

## 📝 Step-by-Step Manual Setup (Alternative)

If you prefer to create things step-by-step instead of using the all-in-one setup:

### Step 1: Create Directories
```
Tools → Quick Setup → 1. Create Directories
```

### Step 2: Create ScriptableObjects
```
Tools → Quick Setup → 2. Create ScriptableObjects Only
```

### Step 3: Create Player Prefabs
```
Tools → Quick Setup → 3. Create Player Prefabs Only
```

### Step 4: Build GameScene
```
Tools → Snakes and Ladders → Build Complete Game Scene
```

### Step 5: Generate Board
1. Open GameScene
2. Select Board GameObject
3. Click "Generate Board" in Inspector

### Step 6: Setup Build Settings
```
Tools → Quick Setup → 4. Setup Build Settings
```

### Step 7: Test
1. Press Play
2. The game should start automatically (if AutoGameStarter is configured)

---

## 🔧 Troubleshooting

### Problem: "GameController not found in scene"
**Solution**: Make sure you've built the scene using the SceneBuilder. The GameController should be attached to the GameManager GameObject.

### Problem: "Board already generated, skipping auto-generation"
**Solution**: This is normal. The board has already been generated. If you want to regenerate, select the Board object and click "Clear Existing Board", then "Generate Board".

### Problem: "ScriptableObjects not found"
**Solution**: Run "Tools → Quick Setup → 2. Create ScriptableObjects Only" to create them.

### Problem: "Player prefabs not assigned"
**Solution**: The SceneBuilder should automatically assign them. If not, manually drag the 4 player prefabs from Assets/Prefabs to the PlayerManager component's "Player Piece Prefabs" array.

### Problem: "Build settings empty"
**Solution**: Run "Tools → Quick Setup → 4. Setup Build Settings" to add scenes to build.

### Problem: "Roll Dice button doesn't work"
**Solution**: Make sure the DiceController component is attached to the Dice GameObject and the button's OnClick event is connected.

---

## 🎨 Customization After Automation

Once the scene is auto-generated, you can customize:

### Visual Customization:
- Replace primitive shapes with custom 3D models
- Add materials and textures
- Import player piece sprites/models
- Add particle effects (confetti, dust trails, etc.)
- Add sound effects and music

### Gameplay Customization:
- Adjust movement speed in GameConfig
- Change dice sides in DiceConfig
- Modify board size (rows/columns)
- Add more snakes/ladders
- Implement AI players
- Add power-ups or special tiles

### UI Customization:
- Replace Unity UI with TextMeshPro for better text rendering
- Add animations to UI elements
- Create a proper main menu scene
- Add settings panel (sound, graphics, etc.)
- Implement player name input

---

## 📊 Architecture Overview

The automation creates a clean, modular architecture:

```
Event-Driven System
    ├── EventBus (Pub/Sub pattern)
    ├── ServiceLocator (Dependency injection)
    └── PoolManager (Object pooling)

Game Loop
    ├── GameController (Main state machine)
    ├── PlayerManager (Player data & pieces)
    ├── MovementSystem (Movement logic)
    └── GameStateMachine (Idle → Moving → GameOver)

Board System
    ├── BoardManager (Tile positions)
    ├── BoardModel (Snake/ladder data)
    ├── BoardGenerator (Procedural generation)
    └── BoardSquare (Individual tiles)

Input/Output
    ├── DiceController (Dice rolling logic)
    ├── GameHUD (In-game UI)
    └── GameOverUI (Victory screen)
```

---

## 🚀 Next Steps

After using the automation:

1. **Play Test**: Press Play and roll the dice to test movement
2. **Visual Polish**: Replace primitives with custom art assets
3. **Main Menu**: Create a MainMenu scene with player setup
4. **Networking**: Use the included NetworkGameManager for multiplayer
5. **Mobile**: Add touch controls and responsive UI
6. **Build**: File → Build Settings → Build

---

## 📚 Additional Resources

- **README.md**: Feature overview and quick start
- **IMPLEMENTATION_GUIDE.md**: Detailed manual setup instructions
- **Scripts Documentation**: All scripts have XML documentation comments

---

## 🎯 Summary

The automation system provides:

✅ **Zero manual setup** - Everything created with one click
✅ **Fully wired references** - All components connected automatically
✅ **Production-ready structure** - Clean architecture and best practices
✅ **Easy customization** - Modify configs without touching code
✅ **Quick iteration** - Regenerate board anytime with different layouts
✅ **Time savings** - From hours of setup to minutes

**Enjoy building your Snakes and Ladders game! 🎲🐍🪜**
