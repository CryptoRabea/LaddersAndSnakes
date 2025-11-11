# 🎲 Snakes and Ladders 3D - Complete Implementation Guide

## 📋 Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Setup Instructions](#setup-instructions)
4. [Creating Game Scenes](#creating-game-scenes)
5. [Configuring Components](#configuring-components)
6. [Local Multiplayer Setup](#local-multiplayer-setup)
7. [Online Multiplayer Setup](#online-multiplayer-setup)
8. [Testing & Debugging](#testing--debugging)
9. [Customization](#customization)

---

## 🎯 Overview

This is a complete 3D Snakes and Ladders board game system with support for:
- ✅ **Local Multiplayer** (2-4 players on same device)
- ✅ **Online Multiplayer** (using Unity Netcode)
- ✅ **3D Board Generation** (procedural or manual)
- ✅ **Event-Driven Architecture** (EventBus pattern)
- ✅ **Complete UI System** (Main Menu, HUD, Game Over)
- ✅ **AI Players Support**
- ✅ **Snake & Ladder Detection**
- ✅ **Smooth Movement Animations**

---

## 🏗️ Architecture

### Core Systems

```
ServiceLocator (Core)
├── EventBus - Event system for decoupled communication
└── PoolManager - Object pooling for performance

GameController (Gameplay)
├── PlayerManager - Manages all players
├── GameStateMachine - Handles game states (Idle, Moving, GameOver)
└── MovementSystem - Handles piece movement

BoardManager
├── BoardConfig (ScriptableObject) - Board data
├── BoardGenerator - Procedural board generation
└── TileData - Individual tile information

UI System
├── MainMenuUI - Main menu and player setup
├── GameHUD - In-game interface
└── GameOverUI - Victory screen

Networking (Optional)
├── NetworkGameManager - Multiplayer game logic
├── LobbyManager - Lobby creation/joining
└── NetworkPlayerPiece - Synced player pieces
```

### Event Flow

```
Player Input → DiceController → DiceRolledEvent
                                      ↓
                            GameController (State Machine)
                                      ↓
                            MoveRequestedEvent
                                      ↓
                            MovementSystem
                                      ↓
                    (Check Snakes/Ladders)
                                      ↓
                            PieceMovedEvent
                                      ↓
                    (Check Win Condition)
                                      ↓
                    GameOverEvent OR TurnEndedEvent
```

---

## 🚀 Setup Instructions

### Step 1: Unity Project Setup

1. **Unity Version**: 2021.3 or newer recommended
2. **Required Packages**:
   ```
   Window → Package Manager → Install:
   - TextMeshPro (Essential)
   - Unity Netcode for GameObjects (For online multiplayer)
   - Unity Transport (For online multiplayer)
   ```

### Step 2: Create Folder Structure

Your `Assets/Scripts` folder is already organized:
```
Assets/Scripts/
├── Config/          (ScriptableObjects)
├── Core/            (ServiceLocator, EventBus, PoolManager)
├── Entities/        (PlayerPiece, DiceController, MovementSystem)
├── Events/          (All game events)
├── Gameplay/        (GameController, PlayerManager, StateMachine)
├── Networking/      (Network components)
└── UI/              (All UI scripts)
```

### Step 3: Create ScriptableObject Assets

1. **Create GameConfig**:
   - Right-click in Project → Create → LAS → GameConfig
   - Set values:
     - Board Size: 100
     - Move Speed: 4
     - Move Curve: EaseInOut

2. **Create BoardConfig**:
   - Right-click in Project → Create → LAS → BoardConfig
   - This will be auto-populated by BoardGenerator

3. **Create DiceConfig**:
   - Right-click in Project → Create → LAS → DiceConfig
   - Set values:
     - Sides: 6
     - Roll Duration: 1.2
     - Spin Torque: (400, 400, 400)

---

## 🎮 Creating Game Scenes

### Scene 1: Main Menu

1. **Create New Scene**: `MainMenu`
   - File → New Scene → Basic (Built-in)

2. **Add UI Canvas**:
   ```
   Hierarchy → Right-click → UI → Canvas
   - Canvas Scaler: Scale with Screen Size
   - Reference Resolution: 1920x1080
   ```

3. **Add MainMenuUI Component**:
   - Create empty GameObject: `MainMenuManager`
   - Add Component: `MainMenuUI`
   - Create UI elements:
     ```
     Canvas
     ├── MainPanel
     │   ├── Title (TextMeshPro)
     │   ├── PlayButton
     │   ├── SettingsButton
     │   └── QuitButton
     ├── PlayerSetupPanel
     │   ├── PlayerCountDropdown
     │   ├── StartGameButton
     │   └── BackButton
     └── SettingsPanel
         ├── MusicToggle
         ├── SFXToggle
         └── BackButton
     ```

4. **Assign References** in MainMenuUI Inspector:
   - Drag UI elements to corresponding fields
   - Set Game Scene Name: "GameScene"

### Scene 2: Game Scene

1. **Create New Scene**: `GameScene`

2. **Setup Camera**:
   - Position: (5, 15, -10)
   - Rotation: (45, 0, 0)
   - FOV: 60

3. **Add Lighting**:
   - Directional Light
   - Rotation: (50, -30, 0)

4. **Create Game Manager**:
   ```
   Create Empty GameObject: "GameManager"
   Add Components:
   - GameController
   - PlayerManager
   - MovementSystem
   - BoardManager
   - BoardModel
   ```

5. **Create Board**:
   ```
   Create Empty GameObject: "Board"
   Add Component: BoardGenerator

   Configure BoardGenerator:
   - Rows: 10
   - Columns: 10
   - Tile Size: 1.5
   - Number of Snakes: 5
   - Number of Ladders: 5
   - Assign BoardConfig asset
   ```

6. **Generate Board**:
   - Select Board GameObject
   - In Inspector → BoardGenerator
   - Click "Generate Board" button
   - This creates all tiles, snakes, and ladders automatically

7. **Create Dice**:
   ```
   Create 3D Object: Cube
   Name: "Dice"
   Position: (15, 1, 5)
   Scale: (0.5, 0.5, 0.5)

   Add Components:
   - DiceView
   - DiceController
   - Animator (optional, for animations)
   ```

8. **Create Player Prefabs**:
   ```
   Create 4 player prefabs:

   Player1 (Sphere - Red)
   - Create 3D Object → Sphere
   - Scale: (0.5, 0.5, 0.5)
   - Material: Red
   - Add Component: PlayerPiece
   - Drag to Project folder to create prefab
   - Delete from scene

   Player2 (Sphere - Blue)
   - Same as above but Blue color

   Player3 (Sphere - Green)
   Player4 (Sphere - Yellow)
   ```

9. **Create UI Canvas**:
   ```
   Canvas
   ├── GameHUD (Add GameHUD component)
   │   ├── TopPanel
   │   │   ├── CurrentPlayerText
   │   │   ├── PlayerPositionText
   │   │   ├── TurnNumberText
   │   │   └── ColorIndicator (Image)
   │   ├── CenterPanel
   │   │   ├── GameStatusText
   │   │   └── DiceResultText
   │   ├── BottomPanel
   │   │   ├── RollDiceButton
   │   │   └── EventLogText
   │   └── PausePanel (Initially inactive)
   │       ├── ResumeButton
   │       └── MainMenuButton
   └── GameOverPanel (Add GameOverUI component, initially inactive)
       ├── WinnerText
       ├── StatsText
       ├── PlayAgainButton
       └── MainMenuButton
   ```

---

## ⚙️ Configuring Components

### GameController Configuration

Select `GameManager` → `GameController` component:

```
References:
- Player Manager: Drag PlayerManager component
- Game Config: Drag GameConfig asset

Game Settings:
- Winning Position: 100
- Exact Roll To Win: ✓ (checked)
```

### PlayerManager Configuration

Select `GameManager` → `PlayerManager` component:

```
Players (Array size: 4)

Element 0:
- Player Name: "Red Player"
- Player Color: Red (255, 0, 0)
- Piece Prefab: Player1Prefab
- Is AI: ☐ (unchecked)

Element 1:
- Player Name: "Blue Player"
- Player Color: Blue (0, 0, 255)
- Piece Prefab: Player2Prefab
- Is AI: ☐

Element 2:
- Player Name: "Green Player"
- Player Color: Green (0, 255, 0)
- Piece Prefab: Player3Prefab
- Is AI: ☐

Element 3:
- Player Name: "Yellow Player"
- Player Color: Yellow (255, 255, 0)
- Piece Prefab: Player4Prefab
- Is AI: ☐

Spawn Point: Create empty GameObject at (0, 0.5, 0)
```

### MovementSystem Configuration

Select `GameManager` → `MovementSystem` component:

```
References:
- Board Model: Drag BoardModel component
- Board Manager: Drag BoardManager component
- Game Config: Drag GameConfig asset
- Game Controller: Drag GameController component
```

### BoardManager Configuration

Select `GameManager` → `BoardManager` component:

```
Board Data:
- Board Config: Drag BoardConfig asset
- Board Parent: Drag "Board/Board" GameObject (auto-created by generator)
```

### DiceController Configuration

Select `Dice` GameObject:

```
References:
- Config: Drag DiceConfig asset
- Dice View: Drag DiceView component (same GameObject)
- Roll Button: Drag RollDiceButton from Canvas
- Result Text: Drag DiceResultText from Canvas

Settings:
- Auto Roll Enabled: ☐ (for manual rolling)
- Auto Roll Delay: 0.5
```

### UI Configuration

**GameHUD** (Canvas → GameHUD):
```
Player Info:
- Current Player Text: Drag TextMeshPro element
- Player Position Text: Drag TextMeshPro element
- Current Player Color Indicator: Drag Image element

Dice Info:
- Dice Result Text: Drag TextMeshPro element
- Roll Dice Button: Drag Button element

Game Info:
- Turn Number Text: Drag TextMeshPro element
- Game Status Text: Drag TextMeshPro element

Action Buttons:
- Pause Button: Drag Button element
- Pause Panel: Drag Panel GameObject
- Resume Button: Drag Button in pause panel
- Main Menu Button: Drag Button in pause panel

Event Log:
- Event Log Text: Drag TextMeshPro element
- Max Log Lines: 5
```

**GameOverUI** (Canvas → GameOverPanel):
```
UI Elements:
- Game Over Panel: (This GameObject)
- Winner Text: Drag TextMeshPro element
- Stats Text: Drag TextMeshPro element
- Play Again Button: Drag Button element
- Main Menu Button: Drag Button element

Animation:
- Confetti Effect: (Optional particle system)
```

---

## 🎮 Local Multiplayer Setup

Local multiplayer is **already configured** with the above setup!

### How to Play Locally

1. **Start Game**:
   - Run MainMenu scene
   - Click "Play"
   - Select number of players (2-4)
   - Click "Start Game"

2. **Game Flow**:
   - Players take turns on the same device
   - Current player clicks "Roll Dice"
   - Piece moves automatically
   - Turn ends, next player's turn begins

### Adding AI Players

To add AI opponents:

1. Open `GameScene`
2. Select `GameManager` → `PlayerManager`
3. For any player, check `Is AI` checkbox
4. AI players will automatically roll dice on their turn

---

## 🌐 Online Multiplayer Setup

### Prerequisites

1. **Install Unity Netcode**:
   ```
   Window → Package Manager
   - Unity Registry
   - Search: "Netcode for GameObjects"
   - Install
   - Also install: "Unity Transport"
   ```

### Setup Networking Scene

1. **Create Network Manager**:
   ```
   In GameScene:
   Create Empty GameObject: "NetworkManager"

   Add Component: NetworkManager (Unity's)
   Configure:
   - Transport: Unity Transport

   Add Component: NetworkGameManager (Your script)
   - Game Controller: Drag GameController
   - Player Manager: Drag PlayerManager
   - Max Players: 4
   ```

2. **Create Lobby Scene**:
   ```
   Create New Scene: "LobbyScene"

   Canvas
   └── LobbyPanel (Add LobbyManager component)
       ├── PlayerNameInput (TMP_InputField)
       ├── HostButton
       ├── JoinButton
       ├── JoinCodeInput (TMP_InputField)
       ├── LobbyCodeText (TextMeshPro)
       ├── StatusText (TextMeshPro)
       ├── StartGameButton
       ├── LeaveLobbyButton
       └── PlayerListContent (Scroll View)
   ```

3. **Configure LobbyManager**:
   ```
   UI References:
   - Lobby Panel: Drag panel GameObject
   - Player Name Input: Drag input field
   - Join Code Input: Drag input field
   - Lobby Code Text: Drag text element
   - Status Text: Drag text element

   Buttons:
   - Host Button: Drag button
   - Join Button: Drag button
   - Start Game Button: Drag button
   - Leave Lobby Button: Drag button

   Player List:
   - Player List Content: Drag ScrollView content
   - Player List Item Prefab: Create prefab with TextMeshPro
   ```

4. **Setup Network Prefabs**:
   ```
   For each player prefab:
   - Open prefab
   - Add Component: NetworkObject
   - Add Component: NetworkPlayerPiece
     - Player Piece: Drag PlayerPiece component
   - Save prefab
   ```

5. **Register Network Prefabs**:
   ```
   Select NetworkManager GameObject
   - Network Prefabs List:
     - Add all player prefabs
     - Add any other networked objects
   ```

### Build Settings for Multiplayer

```
File → Build Settings
Add Scenes in order:
1. MainMenu
2. LobbyScene
3. GameScene

Platform: PC, Mac & Linux Standalone
(Or your target platform)
```

### Testing Online Multiplayer

1. **Local Network Test**:
   ```
   - Build the game
   - Run 2 instances

   Instance 1:
   - Enter name
   - Click "Host"
   - Copy lobby code
   - Click "Start Game" when ready

   Instance 2:
   - Enter name
   - Paste lobby code
   - Click "Join"
   ```

2. **Using Unity Relay** (Recommended for production):
   ```
   - Sign up for Unity Gaming Services
   - Enable Relay service
   - Update LobbyManager to use Relay
   - Reference: https://docs.unity.com/relay/
   ```

---

## 🧪 Testing & Debugging

### Testing Checklist

- [ ] **Board Generation**:
  - Board generates correctly with all 100 tiles
  - Snakes and ladders are created
  - Tile numbers display correctly

- [ ] **Player Movement**:
  - Dice rolls generate numbers 1-6
  - Players move correct number of spaces
  - Movement animation is smooth
  - Players follow snaking board pattern

- [ ] **Snakes & Ladders**:
  - Landing on ladder moves player up
  - Landing on snake moves player down
  - Events are logged in HUD

- [ ] **Win Condition**:
  - Game ends when player reaches 100
  - Game Over screen appears
  - Correct winner is displayed

- [ ] **UI**:
  - All buttons work
  - Current player is highlighted
  - Turn counter updates
  - Event log updates

- [ ] **Multiplayer** (if implemented):
  - Players can host/join
  - Turns alternate correctly
  - Moves sync across clients
  - Game ends for all clients

### Common Issues & Fixes

**Issue**: Board doesn't generate
```
Fix:
- Check BoardConfig is assigned
- Ensure Tile prefab or materials are assigned
- Check console for errors
```

**Issue**: Players don't move
```
Fix:
- Verify MovementSystem references are assigned
- Check EventBus is initialized in GameController
- Ensure PlayerManager initialized players
```

**Issue**: Dice doesn't roll
```
Fix:
- Check DiceController is on Dice GameObject
- Verify Roll Button is assigned
- Check EventBus subscription in GameController
```

**Issue**: Multiplayer doesn't connect
```
Fix:
- Ensure Netcode package is installed
- Check NetworkManager is properly configured
- Verify network prefabs are registered
- Check firewall settings for local testing
```

### Debug Tools

Add to any script for debugging:
```csharp
// Log events
ServiceLocator.Get<IEventBus>()?.Subscribe<DiceRolledEvent>(evt =>
    Debug.Log($"Dice: {evt.result}"));

// Log player positions
Debug.Log($"Player {playerIndex} at position {currentPosition}");

// Draw gizmos for tile positions
void OnDrawGizmos()
{
    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(transform.position, 0.2f);
}
```

---

## 🎨 Customization

### Custom Board Design

#### Option 1: Manual Board Creation

1. Delete auto-generated board
2. Create tiles manually in scene
3. Parent them under `Board/Board`
4. BoardManager will use their positions

#### Option 2: Custom Board Generator Settings

```csharp
// In BoardGenerator Inspector:
- Rows: Change for different board shapes
- Columns: Change for different board shapes
- Tile Size: Adjust spacing
- Number of Snakes: More/fewer obstacles
- Number of Ladders: More/fewer shortcuts

// Use custom prefabs:
- Tile Prefab: Your custom tile model
- Snake Prefab: Your snake 3D model
- Ladder Prefab: Your ladder 3D model
```

### Custom Player Pieces

1. Create your 3D model
2. Add `PlayerPiece` component
3. Add `Animator` component (optional)
4. Assign to PlayerManager

Recommended animations:
- Idle
- Move
- Jump (for ladders)
- Fall (for snakes)

### Custom Rules

Edit `GameController.cs`:

```csharp
// Example: Multiple dice
int dice1 = Random.Range(1, 7);
int dice2 = Random.Range(1, 7);
int total = dice1 + dice2;

// Example: Extra turn on 6
if (diceResult == 6)
{
    // Don't end turn, let player roll again
    return;
}

// Example: Power-ups
if (landed on PowerUpTile)
{
    ApplyPowerUp(playerIndex);
}
```

### Visual Polish

**Add Particle Effects**:
```
- Dice roll sparkles
- Snake bite effect
- Ladder climb trail
- Win celebration confetti
```

**Add Sound Effects**:
```csharp
// Create AudioManager:
public class AudioManager : MonoBehaviour
{
    public AudioClip diceRoll;
    public AudioClip move;
    public AudioClip snakeHit;
    public AudioClip ladderClimb;
    public AudioClip victory;

    AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();

        // Subscribe to events
        eventBus.Subscribe<DiceRolledEvent>(evt =>
            source.PlayOneShot(diceRoll));
    }
}
```

**Camera Control**:
```csharp
// Add to camera for better view:
public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(target);
    }
}
```

---

## 📚 Script Reference Quick Guide

### Core Scripts

| Script | Purpose | Key Methods |
|--------|---------|-------------|
| `GameController` | Main game logic | `StartGame()`, `EndTurn()`, `RestartGame()` |
| `PlayerManager` | Manages players | `AddPlayer()`, `GetPlayer()`, `InitializePlayers()` |
| `BoardManager` | Board data | `GetTilePosition()`, `GetTargetTileIndex()` |
| `BoardGenerator` | Creates 3D board | `GenerateBoard()` |
| `DiceController` | Dice rolling | `RollDice()`, `ForceRoll()` |
| `MovementSystem` | Movement logic | `HandleMove()` |

### Events

All events in `GameEvents.cs`:
- `DiceRolledEvent` - Dice was rolled
- `MoveRequestedEvent` - Player should move
- `PieceMovedEvent` - Player finished moving
- `TurnEndedEvent` - Turn ended
- `GameStartedEvent` - Game started
- `GameOverEvent` - Game ended
- `SnakeHitEvent` - Hit a snake
- `LadderHitEvent` - Climbed a ladder
- `PlayerJoinedEvent` - Player joined

### Subscribing to Events

```csharp
IEventBus eventBus = ServiceLocator.Get<IEventBus>();

eventBus?.Subscribe<DiceRolledEvent>(OnDiceRolled);

void OnDiceRolled(DiceRolledEvent evt)
{
    Debug.Log($"Rolled: {evt.result}");
}

// Don't forget to unsubscribe!
void OnDestroy()
{
    eventBus?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
}
```

---

## 🎯 Quick Start Summary

**For Local Multiplayer Only**:
1. Complete Steps 1-3 (Setup)
2. Create MainMenu scene with MainMenuUI
3. Create GameScene with:
   - GameManager (all components)
   - Board (BoardGenerator)
   - Dice (DiceController)
   - UI Canvas (GameHUD, GameOverUI)
4. Configure all component references
5. Add to Build Settings
6. Test!

**To Add Online Multiplayer**:
1. Install Netcode package
2. Create LobbyScene with LobbyManager
3. Add NetworkManager to GameScene
4. Add NetworkObject to player prefabs
5. Add NetworkGameManager component
6. Build and test with 2 instances

---

## 📞 Support & Resources

- **Unity Documentation**: https://docs.unity3d.com/
- **Netcode Documentation**: https://docs-multiplayer.unity3d.com/
- **TextMeshPro Guide**: https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest

---

## ✅ Final Checklist

Before releasing your game:

- [ ] All scenes in Build Settings
- [ ] All references assigned in Inspector
- [ ] Board generates correctly
- [ ] Local multiplayer works
- [ ] UI is responsive
- [ ] Win condition works
- [ ] Snakes and ladders function
- [ ] (Optional) Online multiplayer tested
- [ ] Sound effects added
- [ ] Visual effects polished
- [ ] Performance optimized
- [ ] Tested on target platform

---

## 🎉 Congratulations!

You now have a complete, fully functional 3D Snakes and Ladders game with both local and online multiplayer support!

**Next Steps**:
- Add your own 3D models
- Create custom animations
- Add power-ups and special tiles
- Implement achievements
- Add leaderboards
- Publish to your target platform!

Happy Game Development! 🎮
