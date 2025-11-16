# Ladders and Snakes - Project Architecture

## Overview

This document describes the professional architecture of the Ladders and Snakes Unity project. The codebase follows clean architecture principles with clear separation of concerns, event-driven communication, and modular design.

## Project Philosophy

- **Modularity**: Each system is self-contained and loosely coupled
- **Testability**: Dependencies are injected through ServiceLocator for easy mocking
- **Maintainability**: Clear naming conventions and single responsibility principle
- **Scalability**: Event-driven architecture allows easy feature additions
- **Editor-First**: Comprehensive editor tools for rapid development

## Directory Structure

```
Assets/Scripts/
├── Config/              Configuration ScriptableObjects
│   ├── BoardConfig.cs           Board layout configuration
│   ├── DiceConfig.cs            Dice behavior settings
│   ├── GameConfig.cs            Global game settings
│   ├── GameSetupConfig.cs       Setup parameters
│   ├── DifficultyLevel.cs       Difficulty enum
│   ├── DifficultyPreset.cs      AI difficulty presets
│   └── BoardGeneratorConfig.cs  Board generation settings
│
├── Core/                Core utilities and infrastructure
│   ├── ServiceLocator.cs        Dependency injection container
│   ├── EventBus.cs              Event system for decoupled communication
│   ├── PoolManager.cs           Object pooling system
│   ├── UIReferenceAttribute.cs  Attribute for UI auto-binding
│   ├── UIReferenceBinder.cs     Runtime UI reference binding
│   └── UIReferenceAutoBinding.cs Auto-binding MonoBehaviour
│
├── Editor/              Editor-only tools and utilities
│   ├── LASEditorMenu.cs         Centralized menu structure
│   ├── SceneBuilder.cs          Automated scene creation
│   ├── BoardGeneratorWindow.cs  Board generation editor window
│   ├── BoardConfigGenerator.cs  Board config creation tools
│   ├── UIGeneratorWindow.cs     UI generation window
│   ├── UIReferenceToolWindow.cs UI reference binding window
│   ├── UIElementGenerator.cs    UI element factory
│   ├── QuickUISetup.cs          One-click UI setup
│   ├── DefaultBoardSceneSetup.cs Board scene utilities
│   └── UIReferenceEditorMenu.cs UI binding menu items
│
├── Entities/            Game entities and views
│   ├── BoardModel.cs            Board data model
│   ├── BoardSquare.cs           Individual square entity
│   ├── PlayerPiece.cs           Player token entity
│   ├── DiceModel.cs             Dice data model
│   ├── DiceView.cs              Dice visual representation
│   ├── MovementSystem.cs        Player movement logic
│   └── PathFollower.cs          Path-following behavior
│
├── Events/              Event definitions
│   └── GameEvents.cs            All game events (DiceRolled, TurnEnded, etc.)
│
├── Gameplay/            Game logic and controllers
│   ├── GameController.cs        Main game loop controller
│   ├── GameStateMachine.cs      State machine implementation
│   ├── GameSetupManager.cs      Game initialization
│   ├── AIPlayerController.cs    AI opponent logic
│   └── MultiplayerGameController.cs Extended controller for multiplayer
│
├── Networking/          Multiplayer networking
│   ├── NetworkManager.cs        Network connection management
│   └── MultiplayerGameController.cs Network-aware game controller
│
├── UI/                  UI controllers and builders
│   ├── MainMenuController.cs    Main menu logic
│   ├── GameUIManager.cs         In-game UI management
│   ├── RuntimeUIBuilder.cs      Runtime UI generation
│   └── UIAutoInitializer.cs     Automatic UI initialization
│
├── Util/                Utility functions
│   └── Utils.cs                 Helper functions
│
├── BoardGenerator.cs         Board generation logic
├── BoardGeneratorAlgorithm.cs Algorithm implementation
├── BoardManager.cs           Board management
└── BoardValidator.cs         Board configuration validation
```

## Core Systems

### 1. Service Locator Pattern

**Location**: `Core/ServiceLocator.cs`

A lightweight dependency injection container that manages global services.

**Usage**:
```csharp
// Register a service
ServiceLocator.Register<IEventBus>(new EventBus());

// Retrieve a service
var eventBus = ServiceLocator.Get<IEventBus>();

// Clear all services (usually in cleanup)
ServiceLocator.Clear();
```

**Registered Services**:
- `IEventBus`: Event communication system
- `PoolManager`: Object pooling system
- Additional services can be registered at runtime

### 2. Event Bus System

**Location**: `Core/EventBus.cs`

Decoupled event-driven communication system.

**Usage**:
```csharp
// Define an event
public struct DiceRolledEvent { public int result; }

// Subscribe to events
eventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);

// Publish events
eventBus.Publish(new DiceRolledEvent { result = 6 });

// Unsubscribe (important in OnDestroy!)
eventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
```

**Available Events** (in `Events/GameEvents.cs`):
- `DiceRolledEvent`: Dice roll results
- `TurnEndedEvent`: Turn completion
- `PlayerMovedEvent`: Player movement
- `GameOverEvent`: Game completion

### 3. State Machine Pattern

**Location**: `Gameplay/GameStateMachine.cs`

The game uses a state machine for managing game flow.

**States**:
- `IdleState`: Waiting for player input
- `RollingState`: Dice is rolling
- `MovingState`: Player is moving
- `GameOverState`: Game has ended

**Usage**:
```csharp
public class GameController : MonoBehaviour
{
    protected GameState state;

    public void TransitionTo(GameState newState)
    {
        state?.Exit();
        state = newState;
        state.Enter(this);
    }
}
```

### 4. UI Auto-Binding System

**Location**: `Core/UIReferenceAttribute.cs`, `Core/UIReferenceBinder.cs`

Automatic UI reference binding using reflection and attributes.

**Usage**:
```csharp
public class MyUIController : MonoBehaviour
{
    [UIReference("RollDiceButton")]
    private Button rollButton;

    [UIReference("PlayerNameText")]
    private TextMeshProUGUI playerName;

    void Start()
    {
        UIReferenceBinder.BindUIReferences(this);
        // All references are now automatically populated!
    }
}
```

### 5. Object Pooling

**Location**: `Core/PoolManager.cs`

Efficient object reuse system to minimize garbage collection.

**Usage**:
```csharp
// Get object from pool
var obj = PoolManager.Get<MyPoolableObject>();

// Return to pool
PoolManager.Return(obj);
```

## Configuration System

All game settings use ScriptableObjects for easy tweaking and designer-friendly editing.

### Board Configuration

**File**: `Config/BoardConfig.cs`

Defines board layout including ladders and snakes.

```csharp
[CreateAssetMenu(menuName = "LAS/BoardConfig")]
public class BoardConfig : ScriptableObject
{
    public List<BoardJump> jumps;
}
```

### Game Configuration

**File**: `Config/GameConfig.cs`

Global game settings like player count, turn duration, etc.

### Dice Configuration

**File**: `Config/DiceConfig.cs`

Dice behavior including roll duration, animation curves, etc.

## Editor Tools

The project includes a comprehensive suite of editor tools accessible through the **LAS** menu.

### Menu Structure

```
LAS/
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

### Scene Builder

**Location**: `Editor/SceneBuilder.cs`

Automatically creates complete scenes with all necessary GameObjects and components.

**Features**:
- One-click MainMenu scene creation
- One-click Game scene creation
- Automatic build settings configuration
- Full UI hierarchy generation
- Component auto-wiring

### Board Generator

**Location**: `Editor/BoardGeneratorWindow.cs`

Visual editor window for creating board configurations.

**Features**:
- Random board generation with difficulty presets
- Manual ladder/snake placement
- Board validation
- Visual preview
- Save to ScriptableObject

### UI Generator

**Location**: `Editor/UIGeneratorWindow.cs`

Automated UI creation and binding.

**Features**:
- Generate UI elements from code
- Auto-bind UI references
- Validate existing bindings
- Component generation

## Networking Architecture

The project supports local and online multiplayer.

### Network Manager

**Location**: `Networking/NetworkManager.cs`

Handles connection establishment and management.

**Features**:
- Host/Join functionality
- Player synchronization
- State replication

### Multiplayer Game Controller

**Location**: `Networking/MultiplayerGameController.cs`

Extends `GameController` with network awareness.

## Best Practices

### 1. Event-Driven Communication

Always use the EventBus for cross-system communication:

```csharp
// DON'T: Direct coupling
diceController.OnRollComplete = () => playerController.Move();

// DO: Event-driven decoupling
eventBus.Publish(new DiceRolledEvent { result = 6 });
```

### 2. Service Locator Usage

Use ServiceLocator for global dependencies:

```csharp
protected virtual void Start()
{
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus?.Subscribe<DiceRolledEvent>(OnDiceRolled);
}
```

### 3. ScriptableObjects for Configuration

Always use ScriptableObjects for tweakable values:

```csharp
// DON'T: Hard-coded values
private const int MAX_DICE_VALUE = 6;

// DO: ScriptableObject configuration
[SerializeField] private DiceConfig diceConfig;
```

### 4. UI Auto-Binding

Use UIReference attribute instead of manual SerializeField:

```csharp
// OLD WAY: Manual SerializeField (error-prone)
[SerializeField] private Button rollButton;

// NEW WAY: Auto-binding attribute
[UIReference("RollDiceButton")]
private Button rollButton;
```

### 5. State Machine for Game Flow

Use state pattern for complex game logic:

```csharp
public class GameController : MonoBehaviour
{
    public void RollDice()
    {
        if (state is IdleState)
        {
            TransitionTo(new RollingState());
        }
    }
}
```

## Performance Considerations

### Object Pooling

Use PoolManager for frequently spawned objects:
- Particle effects
- Player pieces
- UI elements

### Event Cleanup

Always unsubscribe from events in `OnDestroy`:

```csharp
protected virtual void OnDestroy()
{
    ServiceLocator.Get<IEventBus>()?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
}
```

## Testing Strategy

### Unit Testing

- Core systems (ServiceLocator, EventBus) are testable in isolation
- Mock services can be registered for testing
- State machine logic can be tested without Unity

### Integration Testing

- Use Unity Test Framework for integration tests
- Test scenes can be built programmatically using SceneBuilder

## Future Extensibility

The architecture is designed for easy extension:

1. **New Game Modes**: Add new states to the state machine
2. **New Events**: Define new event structs and publish/subscribe
3. **New Services**: Register additional services in ServiceLocator
4. **New Board Layouts**: Create new BoardConfig ScriptableObjects
5. **Custom Dice**: Create new DiceConfig variations
6. **AI Improvements**: Extend AIPlayerController

## Debugging

### Debug Tools

Use the editor menu for debugging:
- `LAS/Board/Setup Board in Scene`: Quickly setup test board
- `LAS/UI/Validate All UI References`: Check for missing UI bindings
- `LAS/Setup/Quick Scene Setup`: One-click scene setup

### Logging

All systems use Unity's Debug.Log with prefixes:
- `[ServiceLocator]`: Service registration logs
- `[EventBus]`: Event publish/subscribe logs
- `[SceneBuilder]`: Scene creation logs
- `[BoardGenerator]`: Board generation logs

## Version Control

### Important Files

Always commit:
- `.cs` source files
- `.meta` files
- ScriptableObject `.asset` files
- Scene `.unity` files

### Ignored Files

The `.gitignore` includes:
- `Library/`: Unity cache
- `Temp/`: Temporary files
- `obj/`: Build artifacts

## Summary

This architecture provides:
- **Clean separation of concerns**
- **Easy testing and debugging**
- **Designer-friendly workflow**
- **Scalable and maintainable codebase**
- **Comprehensive editor tools**
- **Event-driven flexibility**

For implementation details, see [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md).
