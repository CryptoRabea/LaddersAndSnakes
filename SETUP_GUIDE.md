# Ladders and Snakes - Complete Setup Guide

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

### "Nothing happens when I press Play"

**Solution**: Run the setup tool
1. Go to `LAS > Setup Game Scene`
2. Wait for the "Setup Complete" dialog
3. Press Play again

### "No UI visible"

**Check**:
- GameCanvas exists in Hierarchy
- EventSystem exists in Hierarchy
- Canvas is set to "Screen Space - Overlay"

**Fix**: Run `LAS > Setup Game Scene` again

### "Dice button doesn't work"

**Check**:
- DiceModel exists (created automatically at runtime)
- GameController exists (created automatically at runtime)
- Console for any errors

**Fix**: Check that GameSetupManager has "Auto Setup On Start" enabled

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

**Enjoy the game! 🎲🪜🐍**
