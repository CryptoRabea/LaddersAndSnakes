# 🎲 Snakes and Ladders 3D

A complete multiplayer 3D Snakes and Ladders board game built with Unity.

## ✨ Features

- 🎮 **Local Multiplayer** - 2-4 players on the same device
- 🌐 **Online Multiplayer** - Play with friends online using Unity Netcode
- 🎨 **3D Graphics** - Beautiful 3D board with procedural generation
- 🤖 **AI Players** - Add computer opponents
- 📊 **Complete UI System** - Main menu, HUD, and game over screens
- 🎯 **Event-Driven Architecture** - Clean, modular code
- 🚀 **Easy to Customize** - Add your own models, rules, and effects

## 🚀 Quick Start

### Prerequisites
- Unity 2021.3 or newer
- TextMeshPro package (should import automatically)

### Installation

1. Open the project in Unity
2. Open the `IMPLEMENTATION_GUIDE.md` for detailed setup instructions
3. Follow the guide to create your game scenes
4. Build and play!

## 📁 Project Structure

```
Assets/Scripts/
├── Config/          # ScriptableObject configurations
├── Core/            # Core systems (EventBus, ServiceLocator, PoolManager)
├── Entities/        # Game entities (PlayerPiece, DiceController, etc.)
├── Events/          # Event definitions
├── Gameplay/        # Game logic (GameController, PlayerManager, StateMachine)
├── Networking/      # Online multiplayer components
├── UI/              # UI controllers
├── BoardGenerator.cs
└── BoardManager.cs
```

## 🎮 How to Play

### Local Multiplayer
1. Launch the game
2. Select "Play" from main menu
3. Choose number of players (2-4)
4. Click "Start Game"
5. Players take turns rolling the dice
6. First to reach tile 100 wins!

### Online Multiplayer
1. Install Unity Netcode package
2. Follow networking setup in `IMPLEMENTATION_GUIDE.md`
3. One player hosts, others join with lobby code
4. Play together online!

## 📖 Documentation

- **[IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md)** - Complete step-by-step setup guide
- Includes scene setup, component configuration, and troubleshooting

## 🎯 Game Rules

- Players take turns rolling a single die (1-6)
- Move your piece forward by the number rolled
- Landing on a **ladder** moves you up
- Landing on a **snake** moves you down
- First player to reach tile 100 wins
- (Optional) Must roll exact number to win

## 🛠️ Customization

### Add Custom Board
- Modify `BoardGenerator` settings
- Use your own tile, snake, and ladder 3D models

### Add Custom Player Pieces
- Create your 3D model
- Add `PlayerPiece` component
- Assign in `PlayerManager`

### Change Game Rules
- Edit `GameController.cs`
- Modify win conditions, turn logic, etc.

### Add Visual Effects
- Particle systems for dice, snakes, ladders
- Camera animations
- Sound effects

## 🏗️ Architecture

The project uses a clean, event-driven architecture:

```
ServiceLocator → EventBus → Game Systems
                           ↓
        GameController (State Machine)
                ↓
    Entities respond to Events
                ↓
        UI updates automatically
```

### Key Components

- **GameController** - Main game logic and state management
- **PlayerManager** - Handles all player data and pieces
- **BoardManager** - Board tile positions and snake/ladder data
- **BoardGenerator** - Procedural board creation
- **DiceController** - Dice rolling mechanics
- **MovementSystem** - Player piece movement and animations
- **EventBus** - Decoupled event communication

## 🧪 Testing

Run in Unity Editor:
1. Open GameScene
2. Press Play
3. Use Inspector to test different scenarios

Build and Test:
1. File → Build Settings
2. Add scenes: MainMenu, GameScene
3. Build and Run
4. Test multiplayer with multiple instances

## 📦 Dependencies

### Required
- **Unity 2021.3+**
- **TextMeshPro** (Unity package)

### Optional (for online multiplayer)
- **Unity Netcode for GameObjects**
- **Unity Transport**

## 🤝 Contributing

This is a complete game template. Feel free to:
- Modify for your own projects
- Add new features
- Improve existing systems
- Share your creations!

## 📝 Script Reference

### Core Scripts
- `GameController.cs` - Main game controller
- `PlayerManager.cs` - Player management
- `BoardManager.cs` - Board data management
- `BoardGenerator.cs` - Procedural board generation
- `DiceController.cs` - Dice mechanics
- `MovementSystem.cs` - Movement logic

### UI Scripts
- `MainMenuUI.cs` - Main menu controller
- `GameHUD.cs` - In-game HUD
- `GameOverUI.cs` - Victory screen

### Networking Scripts
- `NetworkGameManager.cs` - Network game logic
- `LobbyManager.cs` - Lobby system
- `NetworkPlayerPiece.cs` - Synced player pieces

## 🎨 Assets Needed

To complete the visual setup, you'll need:
- Tile 3D models (or use generated cubes)
- Snake 3D model (or use generated cylinder)
- Ladder 3D model (or use generated cylinder)
- Player piece models (or use generated spheres)
- Dice 3D model (or use generated cube)
- UI sprites and icons
- Sound effects (optional)
- Particle effects (optional)

Default primitives work great for prototyping!

## 🚀 Performance

The game is optimized for:
- Multiple simultaneous players
- Smooth animations
- Event-driven updates (no Update() loops where possible)
- Object pooling for frequently created objects

## 📄 License

This is a Unity game template. Use it for your projects!

## 🎉 Credits

Built with:
- Unity Engine
- C# Programming
- Event-Driven Architecture
- Unity Netcode for GameObjects

---

**Ready to build your game?**

Open `IMPLEMENTATION_GUIDE.md` and follow the step-by-step instructions!

Happy Game Development! 🎮
