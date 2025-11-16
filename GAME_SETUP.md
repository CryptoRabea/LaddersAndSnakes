# Snakes & Ladders - Complete Rebuild

## What Was Fixed

The original game had multiple issues:
- Overly complex architecture with ServiceLocator, EventBus, PoolManager
- Multiple broken game scenes (GameScene, GameScene 2, 3, 11)
- Missing visual board generation
- Broken dependencies and uninitialized references
- No clear working entry point

## New Simple Architecture

The game has been completely rebuilt with a clean, simple architecture:

### Core Scripts
1. **SimpleGameManager.cs** - Main game controller
   - Generates the board visually (10x10 grid = 100 squares)
   - Manages player movement
   - Handles dice rolling
   - Implements snakes and ladders logic
   - Manages game state and win conditions

2. **SimpleMainMenu.cs** - Main menu controller
   - Allows selecting 2, 3, or 4 players
   - Loads the game scene

3. **SimpleGameSetup.cs** (Editor Script) - Automated setup
   - Creates all necessary prefabs
   - Builds MainMenu scene with UI
   - Builds GameScene with complete setup
   - Wires up all references automatically

## How to Set Up the Game

### Automatic Setup (Recommended)

1. Open Unity
2. Go to menu: **Snakes & Ladders → Setup Complete Game**
3. Click "Yes" to create everything
4. Open the **MainMenu** scene
5. Press **Play**!

That's it! The game is ready to play.

### What Gets Created

The setup creates:
- `Assets/Prefabs/Square.prefab` - Board square prefab
- `Assets/Prefabs/Player.prefab` - Player piece prefab
- `Assets/Scenes/MainMenu.unity` - Main menu scene
- `Assets/Scenes/GameScene.unity` - Game scene with everything wired up

## Game Features

### Board Layout
- 100 squares in a snake pattern (10x10 grid)
- Square 1 (bottom-left) is the start
- Square 100 (top-right) is the goal
- Ladders (green squares) move you up
- Snakes (red squares) move you down

### Snakes and Ladders Positions

**Ladders (going up):**
- 4 → 14
- 9 → 31
- 21 → 42
- 28 → 84
- 51 → 67
- 72 → 91
- 80 → 99

**Snakes (going down):**
- 17 → 7
- 54 → 34
- 62 → 19
- 64 → 60
- 87 → 36
- 93 → 73
- 95 → 75
- 98 → 79

### Game Rules
1. Each player starts off the board (position 0)
2. Players take turns rolling a dice (1-6)
3. Move forward by the dice amount
4. Landing on a ladder moves you up
5. Landing on a snake moves you down
6. Must roll exact number to land on square 100
7. First player to reach square 100 wins!

### UI Controls
- **Roll Dice** button - Roll the dice for current player
- Displays current player's turn
- Shows dice result
- Shows game messages (ladder/snake encounters)
- Win screen with Play Again and Main Menu buttons

### Player Settings
- Supports 2-4 players
- Each player has a different color:
  - Player 1: Red
  - Player 2: Blue
  - Player 3: Green
  - Player 4: Yellow

## Technical Details

### No Dependencies on Old Code
The new implementation is completely independent:
- No ServiceLocator
- No EventBus
- No PoolManager
- No complex state machines
- No ScriptableObject configs required

### Everything is Self-Contained
- Board is generated procedurally at runtime
- All references are wired up by the editor script
- No manual inspector assignments needed
- Prefabs are created automatically

### Clean Code Structure
- Single responsibility principle
- No external dependencies
- Easy to understand and modify
- Well-commented

## Customization

You can easily customize the game by modifying SimpleGameManager.cs:

### Change Board Size
```csharp
[SerializeField] private int boardSize = 100;  // Total squares
[SerializeField] private int boardWidth = 10;  // Squares per row
```

### Modify Snakes and Ladders
Edit the `SetupSnakesAndLadders()` method:
```csharp
void SetupSnakesAndLadders()
{
    // Add your own snakes and ladders
    snakesAndLadders[fromSquare] = toSquare;
}
```

### Adjust Movement Speed
```csharp
[SerializeField] private float moveSpeed = 5f;
```

### Change Player Colors
```csharp
[SerializeField] private Color[] playerColors = new Color[] {
    Color.red, Color.blue, Color.green, Color.yellow
};
```

## Troubleshooting

### Missing TextMeshPro
If you see errors about TextMeshPro:
1. Go to Window → TextMeshPro → Import TMP Essential Resources
2. Click "Import"
3. Re-run the setup

### Scenes Not in Build Settings
The setup script automatically adds scenes to build settings, but if needed:
1. Go to File → Build Settings
2. Add MainMenu scene (index 0)
3. Add GameScene scene (index 1)

### Prefabs Not Found
Re-run the setup: Snakes & Ladders → Setup Complete Game

## Development Notes

This is a complete rewrite focusing on:
- Simplicity over complexity
- Working over "perfect architecture"
- Easy setup and maintenance
- Clear, readable code

The old complex system has been replaced with straightforward, functional code that actually works.

## Custom Models Support

Want to use your own 3D models for player pieces and dice?

See **[CUSTOM_MODELS_GUIDE.md](CUSTOM_MODELS_GUIDE.md)** for detailed instructions on:
- Adding custom player piece models (characters, animals, objects, etc.)
- Using different models for each player
- Adding animated 3D dice with physics
- Troubleshooting and tips

The game fully supports custom models - just drag and drop your prefabs!

## Credits

Rebuilt from scratch to fix all issues with the original implementation.
