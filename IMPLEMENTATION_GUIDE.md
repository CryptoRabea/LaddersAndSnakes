# Ladders and Snakes - Complete Implementation Guide

## Table of Contents
1. [Project Architecture](#project-architecture)
2. [Getting Started](#getting-started)
3. [Editor Tools](#editor-tools)
4. [Core Systems](#core-systems)
5. [Multiplayer Setup](#multiplayer-setup)
6. [AI Implementation](#ai-implementation)
7. [UI System](#ui-system)
8. [Board Generation](#board-generation)
9. [Workflow Guide](#workflow-guide)
10. [Troubleshooting](#troubleshooting)

---

## Project Architecture

### Namespace Structure
All code uses the **LAS.*** namespace hierarchy for clean organization:

```
LAS/
├── LAS                  - Core game scripts (BoardGenerator, BoardManager, etc.)
├── LAS.Config          - Configuration ScriptableObjects
├── LAS.Core            - Core utilities (EventBus, ServiceLocator, UIReference)
├── LAS.Editor          - Editor tools and windows
├── LAS.Entities        - Game entities (BoardModel, DiceModel, PlayerPiece)
├── LAS.Events          - Event definitions
├── LAS.Gameplay        - Game logic (GameController, AIPlayer, GameStateMachine)
├── LAS.Networking      - Multiplayer networking
├── LAS.UI              - UI controllers
└── LAS.Examples        - Example implementations
```

### Design Patterns Used
- **Service Locator Pattern**: Core services registration and retrieval
- **Event Bus Pattern**: Decoupled communication between systems
- **ScriptableObject Architecture**: Data-driven configuration
- **MVC Pattern**: Separation of Model-View-Controller for entities
- **Object Pooling**: Efficient reuse of game objects
- **State Machine**: Game state management

---

## Getting Started

### Quick Setup (Recommended)

**Option 1: Automated Scene Setup**
1. Open Unity Editor
2. Navigate to: **LAS → Scenes → Build All Scenes**
3. This creates:
   - MainMenu scene with all buttons configured
   - GameScene with board, camera, and game controllers
   - Properly configured build settings

**Option 2: Manual Scene Setup**
1. **Create MainMenu**:
   - **LAS → Scenes → Build MainMenu Scene**
   - Adds: Canvas, EventSystem, Menu buttons, MainMenuController

2. **Create GameScene**:
   - **LAS → Scenes → Build Game Scene**
   - Adds: Camera, Lighting, NetworkManager, GameController

3. **Add Board**:
   - Open GameScene
   - **LAS → Board → Setup Board in Scene**
   - This generates the complete 10x10 board with ladders and snakes

4. **Configure Build Settings**:
   - **LAS → Scenes → Configure Build Settings**
   - Adds both scenes to build list

### Project Folder Structure
```
Assets/
├── Config/                    - ScriptableObject assets
│   ├── DefaultBoardConfig.asset
│   ├── DefaultDiceConfig.asset
│   ├── DefaultGameConfig.asset
│   └── GameSetupConfig.asset
├── Prefabs/                   - Reusable prefabs
│   ├── PlayerPiece.prefab
│   └── Dice.prefab
├── Scenes/
│   ├── MainMenu.unity
│   └── GameScene.unity
└── Scripts/
    └── (Organized by namespace as shown above)
```

---

## Editor Tools

All editor tools are accessible via the **LAS** menu in Unity's top menu bar.

### LAS Menu Structure

#### **LAS → Scenes**
- **Build All Scenes**: Creates both MainMenu and GameScene with full setup
- **Build MainMenu Scene**: Creates just the main menu
- **Build Game Scene**: Creates just the game scene
- **Configure Build Settings**: Adds scenes to build configuration

#### **LAS → Board**
- **Setup Board in Scene**: Generates complete board with visual tiles
- **Clear Board from Scene**: Removes board from scene
- **Board Generator Window**: Opens visual board configuration tool
- **Create Default Board Config**: Creates ScriptableObject with default layout
- **Select Board Config**: Highlights active board config in project
- **Generate Random Board**: Create procedurally generated boards
  - Easy (more ladders, fewer snakes)
  - Medium (balanced)
  - Hard (more snakes, fewer ladders)
  - Extreme (very challenging)

#### **LAS → UI**
- **UI Generator Window**: Visual UI element creator
- **UI Reference Tool Window**: Manage [UIReference] attributes
- **Bind All UI References in Scene**: Auto-bind all UI references
- **Bind Selected GameObject UI References**: Bind only selected object
- **Validate All UI References**: Check for missing references
- **Add Auto-Binding Component to Selected**: Add runtime auto-binding
- **Generate UI from Selected Component**: Create UI from [UIReference] attributes

#### **LAS → Setup**
- **Quick Scene Setup**: Fast scene configuration for testing

#### **LAS → Documentation**
- **Open Implementation Guide**: Opens this guide
- **Project Architecture**: Shows namespace structure
- **About**: Project information

---

## Core Systems

### 1. Service Locator
Central registry for core services.

```csharp
using LAS.Core;

// Register a service (usually in Awake)
ServiceLocator.Register<IEventBus>(eventBus);

// Retrieve a service anywhere
var eventBus = ServiceLocator.Get<IEventBus>();
```

### 2. Event Bus
Decoupled event communication.

```csharp
using LAS.Core;
using LAS.Events;

// Subscribe to events
eventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);

// Publish events
eventBus.Publish(new DiceRolledEvent { playerId = 0, result = 6 });

// Unsubscribe (important!)
eventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
```

### 3. Board System

**BoardConfig (ScriptableObject)**
```csharp
[CreateAssetMenu(menuName = "LAS/BoardConfig")]
public class BoardConfig : ScriptableObject
{
    public List<BoardJump> jumps;  // All ladders and snakes
}
```

**BoardManager (Scene Component)**
- Manages tile positions
- Provides ladder/snake lookup
- Handles board visualization

```csharp
// Get tile world position
Vector3 pos = boardManager.GetTilePosition(tileIndex);

// Check for ladder/snake
int finalTile = boardManager.GetTargetTileIndex(currentTile);
```

### 4. Dice System

**DiceModel**: Logic and state
**DiceView**: Visual representation and animation

```csharp
// Roll dice for a player
diceModel.RollDice(playerIndex);

// Listen for result
eventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
```

### 5. Player Movement

**MovementSystem**: Handles player piece movement along the board

```csharp
// Move player to specific tile
movementSystem.MovePlayerToTile(playerIndex, targetTile);
```

---

## Multiplayer Setup

### Network Modes

**Local Multiplayer (Hotseat)**
```csharp
NetworkManager.Instance.StartLocalMultiplayer(playerCount: 2);
```

**Single Player vs AI**
```csharp
NetworkManager.Instance.StartSinglePlayerWithAI();
```

**Online Host**
```csharp
NetworkManager.Instance.StartHost();
```

**Online Client**
```csharp
NetworkManager.Instance.StartClient(serverAddress: "192.168.1.100");
```

### Main Menu Integration
The MainMenuController handles all game modes:
- **Play Local Multiplayer**: 2-4 players on same device
- **Play vs AI**: Single player against AI opponent
- **Play Online**: Host or join networked games

---

## AI Implementation

### AIPlayerController
Automatically plays for AI players when it's their turn.

**Features:**
- Automatic dice rolling with thinking delay
- Event-driven turn detection
- Works with single player and multiplayer modes

**Configuration:**
```csharp
[SerializeField] private float thinkDelay = 1.0f;      // Delay before AI acts
[SerializeField] private int aiPlayerIndex = 1;        // Which player is AI
```

**How it works:**
1. Listens for `TurnEndedEvent`
2. Checks if current player is AI
3. Waits `thinkDelay` seconds
4. Automatically rolls dice via `diceModel.RollDice(aiPlayerIndex)`

---

## UI System

### UIReference Attribute System
Automated UI reference binding without manual drag-and-drop.

**Step 1: Mark fields with [UIReference]**
```csharp
using LAS.Core;

public class MyUI : MonoBehaviour
{
    [UIReference] private Button playButton;
    [UIReference] private TextMeshProUGUI titleText;
    [UIReference(Path = "Canvas/Panel/Score")] private TextMeshProUGUI scoreText;
}
```

**Step 2: Generate or Bind**

**Option A: Auto-Generate UI Elements**
1. Select the GameObject with your script
2. **LAS → UI → Generate UI from Selected Component**
3. UI elements are created automatically!

**Option B: Bind Existing UI**
1. Create UI manually in scene
2. Select GameObject
3. **LAS → UI → Bind Selected GameObject UI References**

**Step 3: Runtime Auto-Binding (Optional)**
Add `UIReferenceAutoBinding` component to enable runtime binding:
- **LAS → UI → Add Auto-Binding Component to Selected**

### UI Generator Window
Visual tool for creating UI elements quickly.

**Access**: **LAS → UI → UI Generator Window**

**Features:**
- Quick Create: Buttons, Text, Panels, Images, Sliders, etc.
- Templates: Pause Menu, HUD, Dialog Box, Settings Panel
- Auto-Generate: Creates UI from [UIReference] attributes

---

## Board Generation

### Using Default Board
The default board has:
- 8 Snakes
- 7 Ladders
- Classic layout

Create it: **LAS → Board → Create Default Board Config**

### Procedural Board Generation

**Via Menu:**
- **LAS → Board → Generate Random Board → Medium**
- Choose difficulty: Easy, Medium, Hard, Extreme

**Via Board Generator Window:**
1. **LAS → Board → Board Generator Window**
2. Configure settings:
   - Board size (default: 100 tiles)
   - Ladder count (min/max)
   - Snake count (min/max)
   - Difficulty preset
3. Click **Generate**
4. Click **Save as New BoardConfig** or **Update DefaultBoardConfig**

### Custom Board Configuration
```csharp
// Create a BoardConfig asset
BoardConfig config = CreateInstance<BoardConfig>();

// Add a ladder
config.jumps.Add(new BoardJump(
    from: 4,        // Start tile
    to: 14,         // End tile
    isLadder: true  // It's a ladder
));

// Add a snake
config.jumps.Add(new BoardJump(
    from: 98,       // Snake head
    to: 78,         // Snake tail
    isLadder: false // It's a snake
));
```

---

## Workflow Guide

### Creating a Complete Game

**1. Initial Setup**
```
LAS → Scenes → Build All Scenes
```
This creates everything you need!

**2. Customize Board (Optional)**
```
LAS → Board → Board Generator Window
```
- Adjust difficulty
- Generate new board
- Save configuration

**3. Test in Editor**
- Open MainMenu scene
- Press Play
- Click "Play vs AI" or "Play Local Multiplayer"

**4. Build Game**
- File → Build Settings
- Click Build
- Scenes are already configured!

### Adding a New Game Mode

**Example: 3-Player Mode**

1. Update `GameSetupConfig`:
```csharp
public int playerCount = 3;
```

2. Update MainMenuController to support 3 players:
```csharp
private void OnPlayLocal()
{
    NetworkManager.Instance.StartLocalMultiplayer(playerCount: 3);
    LoadGameScene();
}
```

3. Done! The system automatically handles multiple players.

### Creating Custom UI

**Method 1: Use UI Generator**
1. **LAS → UI → UI Generator Window**
2. Click template (e.g., "Pause Menu")
3. Customize in scene

**Method 2: Use [UIReference]**
1. Create script with [UIReference] attributes
2. **LAS → UI → Generate UI from Selected Component**
3. UI appears automatically!

---

## Troubleshooting

### "Type or namespace LAS could not be found"
**Solution**: All scripts now use `LAS.*` namespaces. Ensure:
- No old `LaddersAndSnakes.*` imports
- Using directives match: `using LAS.Config;`, `using LAS.Core;`, etc.

### "BoardConfig has no tiles array"
**Solution**: `TileData` was removed. Use `BoardJump` instead:
```csharp
// Old (wrong)
boardConfig.tiles[i]

// New (correct)
boardConfig.jumps[i]
```

### UI References are null
**Solutions**:
1. **Bind References**: **LAS → UI → Bind All UI References in Scene**
2. **Validate**: **LAS → UI → Validate All UI References**
3. Check field names match GameObject names (case-insensitive)

### AI Not Playing
**Check**:
1. Is `NetworkManager.IsSinglePlayerAI` true?
2. Is `AIPlayerController` attached to GameController?
3. Check console for `[AIPlayerController]` logs

### Board Not Appearing
1. **LAS → Board → Setup Board in Scene**
2. Check BoardManager has:
   - `boardConfig` assigned
   - `boardParent` assigned (optional)
3. Call `boardGenerator.GenerateBoard()` in scene

### Missing Menu Items
- All menus consolidated under **LAS** in menu bar
- Old **Tools → Ladders & Snakes** removed
- Old **Window** menu items removed
- Use new **LAS** menu instead

---

## Best Practices

### 1. Use ScriptableObjects for Configuration
All game configuration should use ScriptableObjects:
- BoardConfig
- DiceConfig
- GameConfig
- GameSetupConfig

### 2. Event-Driven Communication
Prefer EventBus over direct references:
```csharp
// Good
eventBus.Publish(new PlayerMovedEvent { ... });

// Avoid
gameController.OnPlayerMoved(player);
```

### 3. Service Locator for Core Services
Register services once, access anywhere:
```csharp
ServiceLocator.Register<IEventBus>(eventBus);
```

### 4. Automated UI References
Use `[UIReference]` instead of manual assignment:
```csharp
[UIReference] private Button myButton;  // Auto-bound!
```

### 5. Use Editor Tools
Don't manually create scenes - use:
- **LAS → Scenes → Build All Scenes**
- **LAS → Board → Setup Board in Scene**
- **LAS → UI → UI Generator Window**

---

## Quick Reference

### Most Common Tasks

| Task | Menu Path |
|------|-----------|
| Create scenes | **LAS → Scenes → Build All Scenes** |
| Generate board | **LAS → Board → Setup Board in Scene** |
| Create UI | **LAS → UI → UI Generator Window** |
| Bind UI refs | **LAS → UI → Bind All UI References** |
| Random board | **LAS → Board → Generate Random Board → Medium** |
| Open this guide | **LAS → Documentation → Open Implementation Guide** |

### Key Classes

| Class | Purpose | Namespace |
|-------|---------|-----------|
| `ServiceLocator` | Service registry | LAS.Core |
| `EventBus` | Event system | LAS.Core |
| `BoardManager` | Board logic | (root) |
| `NetworkManager` | Multiplayer | LAS.Networking |
| `AIPlayerController` | AI opponent | LAS.Gameplay |
| `UIReferenceBinder` | Auto UI binding | LAS.Core |

---

## Support & Contribution

### Reporting Issues
1. Check this guide first
2. Check console for error messages
3. Use **LAS → Documentation → About** for version info

### Extending the System
The architecture is designed for extension:
- Add new events to `LAS.Events`
- Add new game modes to `NetworkManager`
- Add new UI templates to `UIGeneratorWindow`
- Add new board algorithms to `BoardGeneratorAlgorithm`

---

**Last Updated**: 2025
**Version**: 1.0
**Architecture**: Clean, Professional, Production-Ready
