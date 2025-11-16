# Board Generator System

A complete procedural board generation system for Ladders and Snakes game with validation, difficulty presets, and an intuitive editor UI.

## Features

- **Procedural Board Generation**: Generate random board configurations with customizable parameters
- **Difficulty Presets**: Easy, Medium, Hard, and Extreme difficulty levels
- **Validation System**: Ensures generated boards follow game rules
- **Balance Control**: Configure the ratio between ladder advancement and snake setbacks
- **Editor Window**: User-friendly UI for board generation
- **Menu Integration**: Quick access via Unity Editor menus
- **Preset System**: Save and reuse custom difficulty configurations

## Quick Start

### Method 1: Using the Editor Window (Recommended)

1. Open Unity Editor
2. Go to menu: `LAS > Board Generator`
3. Choose a difficulty preset or configure manually
4. Click "Generate"
5. Review the generated board in the preview
6. Click "Update DefaultBoardConfig" to save

### Method 2: Using Menu Shortcuts

Generate a board directly from the menu:
- `LAS > Generate Random Board > Easy`
- `LAS > Generate Random Board > Medium`
- `LAS > Generate Random Board > Hard`
- `LAS > Generate Random Board > Extreme`

## Components

### 1. BoardGeneratorConfig (ScriptableObject)

Main configuration asset for board generation parameters.

**Location**: `Assets/Scripts/Config/BoardGeneratorConfig.cs`

**Key Parameters**:
- `boardSize`: Total number of tiles (default: 100)
- `columns`: Number of columns (default: 10)
- `minLadders/maxLadders`: Range of ladders to generate
- `minSnakes/maxSnakes`: Range of snakes to generate
- `minLadderLength/maxLadderLength`: Ladder jump distance range
- `minSnakeLength/maxSnakeLength`: Snake jump distance range
- `balanceRatio`: Target ratio of ladder advancement vs snake setback
- `minDistanceBetweenJumps`: Minimum spacing between jump starts
- `seed`: Random seed for reproducible generation (0 = random)

**Create Asset**: Right-click in Project > `Create > LAS > Board Generator Config`

### 2. BoardGeneratorAlgorithm

Core generation algorithm with validation and placement logic.

**Location**: `Assets/Scripts/BoardGeneratorAlgorithm.cs`

**Key Methods**:
```csharp
// Generate a random board
List<BoardJump> GenerateBoard(out string error)

// Generate with specific difficulty
List<BoardJump> GenerateBoardWithDifficulty(DifficultyLevel difficulty, out string error)
```

**Usage Example**:
```csharp
var config = CreateInstance<BoardGeneratorConfig>();
var algorithm = new BoardGeneratorAlgorithm(config);
var jumps = algorithm.GenerateBoardWithDifficulty(DifficultyLevel.Medium, out string error);

if (jumps != null)
{
    // Use generated jumps
    boardConfig.jumps = jumps;
}
else
{
    Debug.LogError($"Generation failed: {error}");
}
```

### 3. BoardValidator

Validates board configurations to ensure they follow game rules.

**Location**: `Assets/Scripts/BoardValidator.cs`

**Validation Rules**:
- No duplicate jump start positions
- No chained jumps (landing on another jump start)
- Ladders must go up, snakes must go down
- All positions within board bounds
- Minimum distance between jumps respected
- Optional balance ratio checking

**Key Methods**:
```csharp
// Validate entire board
bool ValidateJumps(List<BoardJump> jumps, int boardSize, out string error)

// Validate single jump
bool ValidateJump(BoardJump jump, int boardSize, out string error)

// Calculate balance ratio
float CalculateBalanceRatio(List<BoardJump> jumps)

// Check if board is balanced
bool IsBalanced(List<BoardJump> jumps, float targetRatio, float tolerance = 0.3f)
```

### 4. DifficultyPreset (ScriptableObject)

Stores reusable difficulty configurations.

**Location**: `Assets/Scripts/Config/DifficultyPreset.cs`

**Create Asset**: Right-click in Project > `Create > LAS > Difficulty Preset`

**Usage**:
1. Create a preset asset
2. Configure parameters in Inspector
3. Apply to generator via BoardGeneratorWindow

### 5. BoardGeneratorWindow (Editor)

User-friendly editor window for board generation.

**Location**: `Assets/Scripts/Editor/BoardGeneratorWindow.cs`

**Open**: `LAS > Board Generator`

**Features**:
- Difficulty selection (Easy/Medium/Hard/Extreme/Custom)
- Preset support
- Advanced settings editor
- Live preview with statistics
- Save options (new config, update default, save preset)
- Balance ratio display
- Jump list viewer

## Difficulty Presets

### Easy
- **Ladders**: 8-12 (length: 10-40)
- **Snakes**: 5-8 (length: 5-25)
- **Balance Ratio**: 1.5 (favor ladders)
- **Best For**: Beginners, quick games

### Medium
- **Ladders**: 6-9 (length: 8-35)
- **Snakes**: 6-9 (length: 8-35)
- **Balance Ratio**: 1.2 (slightly favor ladders)
- **Best For**: Standard gameplay

### Hard
- **Ladders**: 5-7 (length: 5-30)
- **Snakes**: 8-12 (length: 10-45)
- **Balance Ratio**: 0.8 (favor snakes)
- **Best For**: Challenging gameplay

### Extreme
- **Ladders**: 3-5 (length: 5-20)
- **Snakes**: 12-15 (length: 15-60)
- **Balance Ratio**: 0.5 (heavily favor snakes)
- **Best For**: Very difficult, long games

## Advanced Usage

### Creating Custom Presets

1. Open Board Generator window: `LAS > Board Generator`
2. Disable "Use Preset"
3. Select "Custom" difficulty
4. Configure advanced settings
5. Click "Generate" to test
6. Click "Save Config as Preset"
7. Name and save your preset asset

### Programmatic Generation

```csharp
using LaddersAndSnakes;
using LAS.Config;

// Create config
var config = ScriptableObject.CreateInstance<BoardGeneratorConfig>();
config.minLadders = 5;
config.maxLadders = 8;
config.minSnakes = 5;
config.maxSnakes = 8;
config.balanceJumps = true;
config.balanceRatio = 1.2f;
config.seed = 12345; // For reproducible results

// Generate
var algorithm = new BoardGeneratorAlgorithm(config);
var jumps = algorithm.GenerateBoard(out string error);

if (jumps != null)
{
    // Validate
    if (BoardValidator.ValidateJumps(jumps, config.boardSize, out error))
    {
        Debug.Log($"Generated valid board with {jumps.Count} jumps");

        // Calculate statistics
        float balance = BoardValidator.CalculateBalanceRatio(jumps);
        Debug.Log($"Balance ratio: {balance:F2}");
    }
}
```

### Custom Validation Rules

```csharp
// Check if a tile can have a jump
bool canPlace = BoardValidator.CanPlaceJumpAt(
    tile: 50,
    existingJumps: currentJumps,
    blockedTiles: new int[] { 1, 100 },
    minDistance: 3
);

// Validate custom jump
var customJump = new BoardJump(25, 75, true);
if (BoardValidator.ValidateJump(customJump, 100, out string error))
{
    // Jump is valid
}
```

## Menu Commands

### LAS/Create Default Board Config
Creates the default hardcoded board configuration (8 snakes, 7 ladders)

### LAS/Select Board Config
Selects the DefaultBoardConfig asset in the Project window

### LAS/Board Generator
Opens the Board Generator window

### LAS/Generate Random Board (Medium)
Quick generation with Medium difficulty

### LAS/Generate Random Board/[Difficulty]
Generate boards with specific difficulty levels:
- Easy
- Medium
- Hard
- Extreme

## Board Configuration Structure

The generated boards use the `BoardJump` struct:

```csharp
public struct BoardJump
{
    public int from;      // Starting tile (1-100)
    public int to;        // Ending tile (1-100)
    public bool isLadder; // true = ladder, false = snake
}
```

**Rules**:
- Ladders: `to > from` (moves player forward)
- Snakes: `to < from` (moves player backward)
- No jumps on tile 1 (start) or tile 100 (finish)
- No overlapping or chained jumps

## Statistics and Balance

The system calculates balance as:

```
Balance Ratio = Total Ladder Advancement / Total Snake Setback
```

**Interpretation**:
- Ratio > 1.0: Ladders give more advancement (easier)
- Ratio = 1.0: Perfect balance
- Ratio < 1.0: Snakes cause more setback (harder)

**Example**:
- 3 ladders: +10, +20, +15 = 45 total
- 3 snakes: -8, -12, -10 = 30 total
- Ratio = 45/30 = 1.5 (easier board)

## Troubleshooting

### "Generation failed: Failed to generate valid board after X attempts"

**Causes**:
- Parameters too restrictive (e.g., too many jumps, too little space)
- Conflicting requirements (e.g., very short board with many long jumps)

**Solutions**:
- Reduce min/max jump counts
- Increase board size
- Adjust jump length ranges
- Reduce minimum distance between jumps
- Increase `maxGenerationAttempts` in config

### "Invalid configuration" errors

**Check**:
- Board size must be divisible by columns
- Min values must be ≤ max values
- All counts must be non-negative
- Start tile range must be within board bounds

### Balance ratio not met

**Adjust**:
- Increase `maxGenerationAttempts` for more tries
- Relax balance requirements (use Custom difficulty)
- Adjust min/max ladder/snake counts
- Modify jump length ranges

## Best Practices

1. **Start with Presets**: Use built-in difficulty presets before customizing
2. **Test Balance**: Generate multiple boards to find desired difficulty
3. **Use Seed**: Set a specific seed for reproducible results during testing
4. **Validate Always**: The system validates automatically, but check error messages
5. **Save Presets**: Save successful custom configurations as presets
6. **Preview First**: Always preview generated boards before saving
7. **Backup**: Keep a copy of your favorite board configurations

## File Locations

```
Assets/
├── Scripts/
│   ├── Config/
│   │   ├── BoardConfig.cs              # Board data structure
│   │   ├── BoardGeneratorConfig.cs     # Generator settings
│   │   └── DifficultyPreset.cs         # Preset system
│   ├── Editor/
│   │   ├── BoardConfigGenerator.cs     # Menu commands
│   │   └── BoardGeneratorWindow.cs     # UI window
│   ├── BoardGeneratorAlgorithm.cs      # Core algorithm
│   └── BoardValidator.cs               # Validation system
└── Config/
    └── DefaultBoardConfig.asset        # Active board config
```

## API Reference

### DifficultyLevel Enum
```csharp
public enum DifficultyLevel
{
    Easy,    // More ladders, fewer snakes
    Medium,  // Balanced
    Hard,    // Fewer ladders, more snakes
    Extreme, // Very few ladders, many snakes
    Custom   // User-defined settings
}
```

## Support

For issues or feature requests, please refer to the project repository.
