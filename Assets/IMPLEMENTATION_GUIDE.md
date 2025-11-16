# Ladders and Snakes - Implementation Guide

## Table of Contents

1. [Getting Started](#getting-started)
2. [Quick Setup](#quick-setup)
3. [Creating a New Scene](#creating-a-new-scene)
4. [Board Configuration](#board-configuration)
5. [UI Development](#ui-development)
6. [Gameplay Features](#gameplay-features)
7. [AI Implementation](#ai-implementation)
8. [Multiplayer Setup](#multiplayer-setup)
9. [Common Tasks](#common-tasks)
10. [Troubleshooting](#troubleshooting)

## Getting Started

### Prerequisites

- Unity 2021.3 LTS or later
- TextMeshPro package installed
- Basic understanding of Unity and C#

### Project Setup

1. Clone the repository
2. Open in Unity
3. Access editor tools via the **LAS** menu (top menu bar)

### First Time Setup

**Option 1: Use the All-in-One Tool (Recommended)**

1. `LAS > Board > Create Default Board Config` - Creates default board configuration
2. `LAS > Setup > Complete Game Setup (All-in-One)` - Opens the unified setup tool
3. Click "SETUP COMPLETE GAME NOW" - Creates a fully playable game!

**Option 2: Manual Step-by-Step**

1. `LAS > Scenes > Build All Scenes` - Creates MainMenu and Game scenes
2. `LAS > Board > Create Default Board Config` - Creates default board configuration
3. `LAS > Board > Setup Board in Scene` - Places board in the scene

## Quick Setup

### ⭐ COMPLETE GAME SETUP (RECOMMENDED - All-in-One Tool)

**The fastest way to get a playable game:**

```
LAS > Setup > Complete Game Setup (All-in-One)
```

This opens a comprehensive setup window where you can:
- **Configure all prefabs** (board, player pieces, dice)
- **Assign configuration assets** (BoardConfig, GameConfig, DiceConfig)
- **Choose setup options** (enable AI, number of players, auto-bind UI)
- **One-click setup** that creates a fully playable game

**What it does:**
1. Sets up GameController and NetworkManager
2. Creates board with 100 squares
3. Spawns player pieces with correct colors
4. Configures dice system
5. Builds complete UI with buttons and indicators
6. Sets up MovementSystem and event handling
7. Optionally enables AI opponent
8. Auto-binds all UI references

**Result:** A complete, playable game ready to test immediately!

---

### Alternative: Complete Scene Setup (Creates Empty Scenes)

```
LAS > Scenes > Build All Scenes
```

This single command:
- Creates MainMenu scene with full UI
- Creates Game scene with controllers
- Configures build settings
- Sets up all required GameObjects

### Quick UI Setup

If you have a GameCanvas in your scene:

```
LAS > Setup > Quick Scene Setup
```

This adds RuntimeUIBuilder component for automatic UI generation.

## Creating a New Scene

### Method 1: Automated Scene Builder

**For Main Menu:**
```
LAS > Scenes > Build MainMenu Scene
```

**For Game Scene:**
```
LAS > Scenes > Build Game Scene
```

### Method 2: Manual Setup

1. Create new scene in Unity
2. Add required systems:
   - EventSystem
   - Canvas
   - GameController
3. Use editor tools to populate:
   ```
   LAS > UI > Bind All UI References in Scene
   ```

### What Gets Created

**MainMenu Scene:**
- Canvas with UI hierarchy
- Main panel (play options, settings, quit)
- Multiplayer panel (host, join, server input)
- Settings panel
- MainMenuController with all references wired

**Game Scene:**
- GameController
- NetworkManager
- CameraRig
- BoardRoot placeholder

## Board Configuration

### Creating a Board Config

#### Option 1: Default Board

```
LAS > Board > Create Default Board Config
```

Creates a standard 10x10 board with balanced ladders and snakes.

#### Option 2: Random Board Generator

```
LAS > Board > Generate Random Board > [Difficulty]
```

Difficulty options:
- **Easy**: 8x8, few obstacles
- **Medium**: 10x10, balanced
- **Hard**: 12x12, many obstacles
- **Extreme**: 15x15, maximum complexity

#### Option 3: Board Generator Window

```
LAS > Board > Board Generator Window
```

Visual interface for:
- Setting board dimensions
- Manual ladder/snake placement
- Real-time validation
- Preview generation

### Board Config Structure

```csharp
[CreateAssetMenu(menuName = "LAS/BoardConfig")]
public class BoardConfig : ScriptableObject
{
    public List<BoardJump> jumps; // Ladders and snakes
}
```

**BoardJump Definition:**
```csharp
public struct BoardJump
{
    public int from;      // Starting tile
    public int to;        // Destination tile
    public bool isLadder; // true = ladder, false = snake
}
```

### Setting Up Board in Scene

```
LAS > Board > Setup Board in Scene
```

This:
1. Finds or creates BoardRoot GameObject
2. Instantiates board prefab
3. Applies selected BoardConfig
4. Positions camera
5. Adds necessary components

### Selecting Active Board Config

```
LAS > Board > Select Board Config
```

Opens file picker to choose which BoardConfig to use.

## UI Development

### UI Auto-Binding System

The project uses attribute-based UI binding for cleaner code.

#### Step 1: Add UIReference Attributes

```csharp
using LAS.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyUIController : MonoBehaviour
{
    [UIReference("RollDiceButton")]
    private Button rollButton;

    [UIReference("TurnIndicator")]
    private TextMeshProUGUI turnText;

    [UIReference("DiceResultText")]
    private TextMeshProUGUI resultText;

    [UIReference("GameOverPanel")]
    private GameObject gameOverPanel;

    void Start()
    {
        UIReferenceBinder.BindUIReferences(this);

        // Now all references are populated!
        rollButton.onClick.AddListener(OnRollDice);
    }

    private void OnRollDice()
    {
        // Your logic here
    }
}
```

#### Step 2: Bind References

**Option A: Automatic Binding (Recommended)**

Add `UIReferenceAutoBinding` component to your UI root:

```
LAS > UI > Add Auto-Binding Component to Selected
```

**Option B: Manual Binding**

```
LAS > UI > Bind All UI References in Scene
```

**Option C: Selective Binding**

1. Select GameObject with UI controller
2. Run: `LAS > UI > Bind Selected GameObject UI References`

### UI Generation

#### Runtime UI Builder

For procedural UI generation:

1. Add `RuntimeUIBuilder` to your Canvas:
   ```
   LAS > Setup > Quick Scene Setup
   ```

2. Or manually force build UI:
   ```
   LAS > UI > Force Build UI Now (Editor)
   ```

#### UI Generator Window

For custom UI creation:

```
LAS > UI > UI Generator Window
```

Features:
- Create buttons, panels, text fields
- Auto-generate hierarchy
- Apply consistent styling
- Bind references automatically

### Validating UI References

Check all UI bindings are correct:

```
LAS > UI > Validate All UI References
```

This scans the scene and reports:
- Missing references
- Incorrectly named GameObjects
- Unbound attributes

## Gameplay Features

### Game Controller Setup

The `GameController` is the heart of the game logic.

```csharp
using LAS.Gameplay;
using LAS.Core;
using LAS.Events;

public class MyGameController : GameController
{
    protected override void Start()
    {
        base.Start(); // Registers services and initializes state machine

        // Your custom initialization
        var eventBus = ServiceLocator.Get<IEventBus>();
        eventBus?.Subscribe<GameOverEvent>(OnGameOver);
    }

    private void OnGameOver(GameOverEvent evt)
    {
        Debug.Log($"Game Over! Winner: Player {evt.winnerIndex}");
    }
}
```

### State Machine

Game flow is managed by states:

```csharp
// Transition to a new state
TransitionTo(new RollingState());

// Create custom states
public class MyCustomState : GameState
{
    public override void Enter(GameController controller)
    {
        // State entry logic
    }

    public override void Exit()
    {
        // State exit logic
    }

    public override void OnDiceRolled(int result)
    {
        // Handle dice roll in this state
    }
}
```

**Available States:**
- `IdleState`: Waiting for input
- `RollingState`: Dice is rolling
- `MovingState`: Player moving
- `GameOverState`: Game ended

### Event System

Communication between systems uses events:

```csharp
using LAS.Events;
using LAS.Core;

// Subscribe to events
void Start()
{
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus?.Subscribe<DiceRolledEvent>(OnDiceRolled);
    eventBus?.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
    eventBus?.Subscribe<TurnEndedEvent>(OnTurnEnded);
}

// Publish events
void RollDice()
{
    int result = Random.Range(1, 7);
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus?.Publish(new DiceRolledEvent { result = result });
}

// Always unsubscribe!
void OnDestroy()
{
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
    eventBus?.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
    eventBus?.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
}
```

**Available Events** (in `Events/GameEvents.cs`):
- `DiceRolledEvent`
- `TurnEndedEvent`
- `PlayerMovedEvent`
- `GameOverEvent`
- Add more as needed!

### Player Movement

```csharp
using LAS.Entities;

public class MyMovementController : MonoBehaviour
{
    [SerializeField] private MovementSystem movementSystem;

    void MovePlayer(int spaces)
    {
        movementSystem.MovePlayerBySpaces(playerPiece, spaces);
    }
}
```

### Dice Rolling

```csharp
using LAS.Entities;
using LAS.Config;

public class DiceController : MonoBehaviour
{
    [SerializeField] private DiceConfig config;
    private DiceModel diceModel;
    private DiceView diceView;

    void Start()
    {
        diceModel = new DiceModel(config);
        diceView = GetComponent<DiceView>();
    }

    public void RollDice()
    {
        int result = diceModel.Roll();
        diceView.ShowRollAnimation(result);

        var eventBus = ServiceLocator.Get<IEventBus>();
        eventBus?.Publish(new DiceRolledEvent { result = result });
    }
}
```

## AI Implementation

### Basic AI Controller

```csharp
using LAS.Gameplay;

public class AIPlayerController : MonoBehaviour
{
    [SerializeField] private float decisionDelay = 1f;

    public void TakeTurn()
    {
        StartCoroutine(TakeTurnCoroutine());
    }

    private IEnumerator TakeTurnCoroutine()
    {
        yield return new WaitForSeconds(decisionDelay);

        // AI logic here
        RollDice();
    }

    private void RollDice()
    {
        // Trigger dice roll
        var eventBus = ServiceLocator.Get<IEventBus>();
        eventBus?.Publish(new DiceRolledEvent { result = Random.Range(1, 7) });
    }
}
```

### AI Difficulty

Use `DifficultyPreset` ScriptableObjects:

```csharp
using LAS.Config;

[SerializeField] private DifficultyPreset difficulty;

void ConfigureAI()
{
    switch (difficulty.level)
    {
        case DifficultyLevel.Easy:
            decisionDelay = 2f;
            break;
        case DifficultyLevel.Hard:
            decisionDelay = 0.5f;
            break;
    }
}
```

## Multiplayer Setup

### Network Manager

Already configured in the Game scene.

### Host a Game

```csharp
using LAS.Networking;

NetworkManager networkManager = FindObjectOfType<NetworkManager>();
networkManager.StartHost();
```

### Join a Game

```csharp
networkManager.StartClient("127.0.0.1");
```

### Synchronized Game Controller

Use `MultiplayerGameController` instead of base `GameController`:

```csharp
using LAS.Networking;

public class MultiplayerGameController : GameController
{
    // Network-aware game logic
    // Automatically handles state synchronization
}
```

## Common Tasks

### Task 1: Add a New Game Mode

1. Create new state class:
   ```csharp
   public class TimeAttackState : GameState
   {
       // Implementation
   }
   ```

2. Add transition logic:
   ```csharp
   if (isTimeAttackMode)
   {
       TransitionTo(new TimeAttackState());
   }
   ```

### Task 2: Create Custom Board Layout

1. Open Board Generator:
   ```
   LAS > Board > Board Generator Window
   ```

2. Set dimensions and obstacles

3. Click "Generate" and save

4. Apply to scene:
   ```
   LAS > Board > Setup Board in Scene
   ```

### Task 3: Add New UI Element

1. Create GameObject in hierarchy

2. Name it clearly (e.g., "HighScoreText")

3. In your controller:
   ```csharp
   [UIReference("HighScoreText")]
   private TextMeshProUGUI highScoreText;
   ```

4. Bind:
   ```
   LAS > UI > Bind All UI References in Scene
   ```

### Task 4: Customize Dice Behavior

1. Create new DiceConfig:
   ```
   Right-click in Project > Create > LAS > DiceConfig
   ```

2. Adjust values (roll duration, animation curve, etc.)

3. Assign to DiceModel

### Task 5: Add Sound Effects

```csharp
using LAS.Events;

void Start()
{
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus?.Subscribe<DiceRolledEvent>(PlayDiceSound);
}

void PlayDiceSound(DiceRolledEvent evt)
{
    audioSource.PlayOneShot(diceRollClip);
}
```

### Task 6: Implement Power-Ups

1. Create PowerUp ScriptableObject

2. Add to board configuration

3. Subscribe to PlayerMovedEvent:
   ```csharp
   void OnPlayerMoved(PlayerMovedEvent evt)
   {
       if (evt.tile.hasPowerUp)
       {
           ApplyPowerUp(evt.tile.powerUp);
       }
   }
   ```

## Troubleshooting

### UI References Not Binding

**Symptom**: NullReferenceException when accessing UI elements

**Solutions**:
1. Check GameObject names match attribute strings exactly
2. Run: `LAS > UI > Validate All UI References`
3. Ensure `UIReferenceBinder.BindUIReferences(this)` is called in Start()
4. Add `UIReferenceAutoBinding` component to root

### Board Not Appearing

**Symptom**: Empty scene, no board visible

**Solutions**:
1. Run: `LAS > Board > Setup Board in Scene`
2. Check BoardConfig is assigned
3. Verify camera position
4. Check BoardRoot exists in hierarchy

### Events Not Firing

**Symptom**: Events published but not received

**Solutions**:
1. Ensure EventBus is registered:
   ```csharp
   ServiceLocator.Register<IEventBus>(new EventBus());
   ```
2. Check subscription before publish
3. Verify event types match exactly
4. Make sure you haven't unsubscribed prematurely

### Compilation Errors

**Symptom**: Scripts won't compile

**Solutions**:
1. Check all namespaces are correct (`using LAS.Core;`, etc.)
2. Verify Unity version (2021.3+)
3. Reimport all scripts
4. Clear Library folder and reopen project

### State Machine Not Transitioning

**Symptom**: Game stuck in one state

**Solutions**:
1. Check state Entry() and Exit() methods
2. Verify TransitionTo() is being called
3. Add debug logs to track state changes:
   ```csharp
   Debug.Log($"[GameController] Transitioning to {newState.GetType().Name}");
   ```

### Network Connection Failed

**Symptom**: Can't connect to multiplayer game

**Solutions**:
1. Check firewall settings
2. Verify IP address is correct
3. Ensure NetworkManager is in scene
4. Check port is not blocked

### Performance Issues

**Symptom**: Low frame rate, stuttering

**Solutions**:
1. Use object pooling for frequently spawned objects
2. Reduce board size
3. Profile with Unity Profiler
4. Check for event subscription leaks (always unsubscribe!)

## Best Practices Checklist

- [ ] Use ServiceLocator for global dependencies
- [ ] Subscribe to events in Start(), unsubscribe in OnDestroy()
- [ ] Use UIReference attributes instead of SerializeField for UI
- [ ] Create ScriptableObjects for all configuration data
- [ ] Use state machine for complex game flow
- [ ] Add validation to editor tools
- [ ] Write clear Debug.Log messages with prefixes
- [ ] Test in both editor and build
- [ ] Document custom editor tools
- [ ] Keep game logic in GameController, not UI controllers

## Quick Reference

### Editor Menu Shortcuts

| Action | Menu Path |
|--------|-----------|
| Build all scenes | `LAS > Scenes > Build All Scenes` |
| Create board | `LAS > Board > Create Default Board Config` |
| Setup scene | `LAS > Setup > Quick Scene Setup` |
| Bind UI | `LAS > UI > Bind All UI References in Scene` |
| Validate UI | `LAS > UI > Validate All UI References` |
| Board generator | `LAS > Board > Board Generator Window` |
| Architecture docs | `LAS > Documentation > Project Architecture` |

### Code Templates

**New UI Controller:**
```csharp
using LAS.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyUIController : MonoBehaviour
{
    [UIReference("MyButton")]
    private Button myButton;

    void Start()
    {
        UIReferenceBinder.BindUIReferences(this);
        myButton.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Implementation
    }
}
```

**New Game Event:**
```csharp
namespace LAS.Events
{
    public struct MyCustomEvent
    {
        public int value;
        public string message;
    }
}
```

**New Game State:**
```csharp
using LAS.Gameplay;

public class MyCustomState : GameState
{
    public override void Enter(GameController controller)
    {
        // Entry logic
    }

    public override void Exit()
    {
        // Exit logic
    }

    public override void OnDiceRolled(int result)
    {
        // Handle dice roll
    }
}
```

## Additional Resources

- [Architecture Documentation](ARCHITECTURE.md)
- Unity Documentation: https://docs.unity3d.com/
- C# Documentation: https://docs.microsoft.com/en-us/dotnet/csharp/

## Getting Help

1. Check this guide
2. Review [ARCHITECTURE.md](ARCHITECTURE.md)
3. Use `LAS > Documentation > Project Architecture` for quick reference
4. Check Unity Console for error messages
5. Run validation tools in LAS menu

---

**Happy coding! The LAS menu is your friend - use it often!**
