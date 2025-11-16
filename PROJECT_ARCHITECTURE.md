# Ladders and Snakes - Professional Architecture

## Quick Reference

### Centralized Menu Structure
```
LAS (Main Menu)
├── Scenes/
│   ├── Build All Scenes
│   ├── Build MainMenu Scene
│   ├── Build Game Scene
│   └── Configure Build Settings
│
├── Board/
│   ├── Setup Board in Scene
│   ├── Clear Board from Scene
│   ├── Board Generator Window
│   ├── Create Default Board Config
│   ├── Select Board Config
│   └── Generate Random Board/
│       ├── Easy
│       ├── Medium
│       ├── Hard
│       └── Extreme
│
├── UI/
│   ├── UI Generator Window
│   ├── UI Reference Tool Window
│   ├── Bind All UI References in Scene
│   ├── Bind Selected GameObject UI References
│   ├── Validate All UI References
│   ├── Add Auto-Binding Component to Selected
│   └── Generate UI from Selected Component
│
├── Setup/
│   └── Quick Scene Setup
│
└── Documentation/
    ├── Open Implementation Guide
    ├── Project Architecture
    └── About
```

## Namespace Architecture

```
LAS/
├── LAS                     Core game scripts
│   ├── BoardGenerator      Generates visual board
│   ├── BoardGeneratorAlgorithm  Procedural generation
│   ├── BoardValidator      Validates board configurations
│   └── BoardManager        Runtime board management
│
├── LAS.Config             ScriptableObject configurations
│   ├── BoardConfig         Board layout (jumps/ladders)
│   ├── BoardGeneratorConfig  Generation parameters
│   ├── DiceConfig          Dice behavior
│   ├── GameConfig          Game rules
│   ├── GameSetupConfig     Initial setup
│   ├── DifficultyLevel     Enum for difficulty
│   └── DifficultyPreset    Preset configurations
│
├── LAS.Core               Core utilities & patterns
│   ├── EventBus            Event system
│   ├── ServiceLocator      Service registry
│   ├── PoolManager         Object pooling
│   ├── UIReferenceAttribute  UI auto-binding attributes
│   ├── UIReferenceBinder   Binding logic
│   └── UIReferenceAutoBinding  Runtime auto-binding
│
├── LAS.Editor             Editor tools (Unity Editor only)
│   ├── LASEditorMenu       CENTRALIZED menu structure
│   ├── SceneBuilder        Automated scene creation
│   ├── BoardGeneratorWindow  Visual board generator
│   ├── BoardConfigGenerator  Config asset creation
│   ├── DefaultBoardSceneSetup  Scene board setup
│   ├── UIGeneratorWindow   Visual UI creator
│   ├── UIElementGenerator  UI element factory
│   ├── UIReferenceToolWindow  UI reference manager
│   ├── UIReferenceEditorMenu  UI menu utilities
│   └── QuickUISetup        Fast UI setup
│
├── LAS.Entities           Game entities (MVC pattern)
│   ├── BoardModel          Board data model
│   ├── BoardSquare         Individual tile
│   ├── DiceModel           Dice logic (Model)
│   ├── DiceView            Dice visualization (View)
│   ├── PlayerPiece         Player game piece
│   ├── MovementSystem      Player movement controller
│   └── PathFollower        Path following behavior
│
├── LAS.Events             Event definitions
│   └── GameEvents          All game event types
│
├── LAS.Gameplay           Game logic & controllers
│   ├── GameController      Main game controller
│   ├── GameStateMachine    State management
│   ├── GameSetupManager    Automated game setup
│   └── AIPlayerController  AI opponent logic
│
├── LAS.Networking         Multiplayer system
│   ├── NetworkManager      Network management
│   └── MultiplayerGameController  Multiplayer game logic
│
├── LAS.UI                 UI controllers
│   ├── MainMenuController  Main menu logic
│   ├── GameUIManager       In-game UI
│   ├── RuntimeUIBuilder    Runtime UI construction
│   └── UIAutoInitializer   UI initialization
│
└── LAS.Examples           Example implementations
    └── ExampleUIManager    UI Reference examples
```

## Design Patterns

### 1. Service Locator Pattern
**Location**: `LAS.Core.ServiceLocator`

**Purpose**: Centralized service registration and retrieval

**Usage**:
```csharp
// Register
ServiceLocator.Register<IEventBus>(eventBus);

// Retrieve
var eventBus = ServiceLocator.Get<IEventBus>();
```

### 2. Event Bus Pattern
**Location**: `LAS.Core.EventBus`

**Purpose**: Decoupled communication between systems

**Usage**:
```csharp
// Subscribe
eventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);

// Publish
eventBus.Publish(new DiceRolledEvent { ... });

// Unsubscribe
eventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
```

### 3. ScriptableObject Architecture
**Location**: `LAS.Config.*`

**Purpose**: Data-driven, designer-friendly configuration

**Files**:
- BoardConfig.asset
- DiceConfig.asset
- GameConfig.asset
- GameSetupConfig.asset

### 4. MVC Pattern
**Location**: `LAS.Entities`

**Purpose**: Separation of Model-View-Controller

**Example**:
- **Model**: `DiceModel` (logic, state)
- **View**: `DiceView` (visualization, animation)
- **Controller**: `GameController` (orchestration)

### 5. Object Pooling
**Location**: `LAS.Core.PoolManager`

**Purpose**: Efficient object reuse

**Usage**:
```csharp
var obj = PoolManager.Get<GameObject>(prefab);
PoolManager.Return(obj);
```

### 6. State Machine
**Location**: `LAS.Gameplay.GameStateMachine`

**Purpose**: Clean state transitions

**States**:
- Menu
- Setup
- Playing
- GameOver

## Data Flow

```
MainMenu
   ↓
NetworkManager.StartSinglePlayerWithAI()
   ↓
SceneManager.LoadScene("GameScene")
   ↓
GameSetupManager.SetupGame()
   ↓
   ├→ SetupBoard()
   ├→ SetupPlayerPieces()
   ├→ SetupDice()
   ├→ SetupGameController()
   └→ SetupMovementSystem()
   ↓
GameController.StartGame()
   ↓
   ├→ Player clicks dice
   ├→ DiceModel.RollDice()
   ├→ EventBus publishes DiceRolledEvent
   ├→ GameController handles event
   ├→ MovementSystem.MovePlayer()
   ├→ BoardManager.GetTargetTile() (check for ladders/snakes)
   ├→ EventBus publishes PlayerMovedEvent
   └→ GameController checks win condition
```

## AI System Flow

```
GameController.OnTurnEnded()
   ↓
EventBus.Publish(TurnEndedEvent)
   ↓
AIPlayerController.OnTurnEnded()
   ↓
Check: Is current player AI?
   ├─ No → Do nothing
   └─ Yes ↓
       Wait thinkDelay seconds
       ↓
       DiceModel.RollDice(aiPlayerIndex)
       ↓
       (Normal game flow continues)
```

## UI Reference System Flow

```
Option 1: Auto-Generate
   ↓
Mark fields with [UIReference]
   ↓
LAS → UI → Generate UI from Selected Component
   ↓
UI elements created automatically!
   ↓
References auto-bound

Option 2: Manual Bind
   ↓
Create UI manually in scene
   ↓
Mark fields with [UIReference]
   ↓
LAS → UI → Bind All UI References
   ↓
References auto-bound

Option 3: Runtime Binding
   ↓
Add UIReferenceAutoBinding component
   ↓
Binds automatically at runtime (Awake or Start)
```

## Build Process

```
LAS → Scenes → Build All Scenes
   ↓
   ├→ BuildMainMenuScene()
   │    ├─ Create Canvas
   │    ├─ Create EventSystem
   │    ├─ Create Main Panel
   │    ├─ Create Buttons (Play Local, Play AI, Play Online, Settings, Quit)
   │    ├─ Create Multiplayer Panel
   │    ├─ Create Settings Panel
   │    └─ Add MainMenuController (with all references bound)
   │
   ├→ BuildGameScene()
   │    ├─ Create Camera Rig
   │    ├─ Setup Lighting
   │    ├─ Add NetworkManager
   │    └─ Add GameController
   │
   └→ ConfigureBuildSettings()
        └─ Add scenes to build list
```

## File Organization

```
Assets/
├── Config/                    ← ScriptableObject assets
│   ├── DefaultBoardConfig.asset
│   ├── DefaultDiceConfig.asset
│   └── DefaultGameConfig.asset
│
├── Prefabs/                   ← Reusable prefabs
│   ├── PlayerPiece.prefab
│   └── Dice.prefab
│
├── Scenes/                    ← Unity scenes
│   ├── MainMenu.unity
│   └── GameScene.unity
│
└── Scripts/                   ← All C# code
    ├── BoardGenerator.cs          [LAS]
    ├── BoardManager.cs            [LAS]
    ├── Config/                    [LAS.Config]
    ├── Core/                      [LAS.Core]
    ├── Editor/                    [LAS.Editor]
    ├── Entities/                  [LAS.Entities]
    ├── Events/                    [LAS.Events]
    ├── Examples/                  [LAS.Examples]
    ├── Gameplay/                  [LAS.Gameplay]
    ├── Networking/                [LAS.Networking]
    └── UI/                        [LAS.UI]
```

## Key Improvements from Original

### ✅ Fixed Issues
1. **Namespace Conflicts**: All `LaddersAndSnakes.*` → `LAS.*`
2. **Duplicate Code**: Removed `TileData`, now only `BoardJump`
3. **Scattered Menus**: Consolidated into single `LAS` menu
4. **Missing Functionality**: Added "Play vs AI" button to SceneBuilder
5. **Compilation Errors**: Fixed duplicate `DifficultyLevel` enum

### ✅ Professional Architecture
1. **Centralized Menu**: All tools under `LAS` menu
2. **Clear Namespaces**: Logical organization
3. **Design Patterns**: Service Locator, Event Bus, MVC, etc.
4. **Documentation**: Complete implementation guide
5. **Automated Workflows**: One-click scene building

### ✅ Developer Experience
1. **No Manual Setup**: Automated scene generation
2. **UI Auto-Binding**: `[UIReference]` attribute system
3. **Visual Tools**: UI Generator, Board Generator windows
4. **Clear Structure**: Easy to find and extend functionality

---

**For detailed usage instructions, see**: `IMPLEMENTATION_GUIDE.md`

**Access all tools via**: Unity Menu Bar → **LAS**
