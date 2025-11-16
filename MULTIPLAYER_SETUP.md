# Multiplayer Setup Guide

## Overview
This Snakes & Ladders game now supports multiplayer functionality with both local hotseat and online modes.

## Quick Start

### Automatic Scene Builder
The project includes an automated scene builder that creates all necessary scenes and configures the project:

1. Open Unity Editor
2. Go to menu: **LAS → Build All Scenes**
3. Done! Your project is ready to build and test

### Manual Scene Building
If you prefer to build scenes individually:

- **LAS → Build MainMenu Scene** - Creates the main menu with multiplayer UI
- **LAS → Build Game Scene** - Creates the game scene with networking support
- **LAS → Configure Build Settings** - Sets up build settings with both scenes

## Features

### 🎮 Multiplayer Modes

#### Local Multiplayer (Hotseat)
- 2-4 players on the same device
- Players take turns using the same controls
- Perfect for family/friends gameplay

#### Online Multiplayer
- Host/Join game sessions
- Real-time synchronization
- Support for remote players

### 🔧 Network Manager
The `NetworkManager` handles all multiplayer functionality:

```csharp
// Start local multiplayer
NetworkManager.Instance.StartLocalMultiplayer(playerCount: 2);

// Host online game
NetworkManager.Instance.StartHost();

// Join online game
NetworkManager.Instance.StartClient(serverAddress: "127.0.0.1");
```

### 📱 Main Menu Features
- **Play Local Multiplayer** - Start hotseat mode (2-4 players)
- **Play Online** - Access online multiplayer options
  - Host Game - Create a game session
  - Join Game - Connect to existing session
  - Server Address - Configure server IP
  - Player Count - Select number of players
- **Settings** - Game configuration
- **Quit** - Exit application

## Project Structure

```
Assets/
├── Scenes/
│   ├── MainMenu.unity      # Main menu with multiplayer UI
│   └── GameScene.unity     # Game scene with networking
├── Scripts/
│   ├── Networking/
│   │   ├── NetworkManager.cs              # Core networking
│   │   └── MultiplayerGameController.cs   # Game controller with network support
│   ├── UI/
│   │   └── MainMenuController.cs          # Main menu logic
│   ├── Editor/
│   │   └── SceneBuilder.cs                # Automated scene builder
│   └── Gameplay/
│       ├── GameController.cs              # Base game controller
│       └── GameStateMachine.cs            # Game state management
```

## Fixed Issues

### ✅ Null Reference Exceptions
The original errors were caused by:
1. `.gitignore` excluding essential Unity files (`.meta`, `.prefab`, `.unity`, `.asset`)
2. Missing scene files and references

**Solution:**
- Updated `.gitignore` to include essential Unity files
- Created automated scene builder to ensure consistent scene setup
- Added proper reference management in all scripts

### ✅ Missing References
The `MissingReferenceException` errors have been resolved by:
- Ensuring all GameObject references are properly assigned
- Using SerializedObject for reliable component references
- Creating scenes programmatically to avoid manual configuration errors

## Building the Game

### Development Build
1. Ensure scenes are built: **LAS → Build All Scenes**
2. Open Build Settings: **File → Build Settings**
3. Both scenes should be listed:
   - MainMenu (index 0)
   - GameScene (index 1)
4. Select platform and click **Build**

### Testing Multiplayer

#### Local Multiplayer
1. Run the game
2. Click "Play Local Multiplayer"
3. Select number of players
4. Game starts with all players ready

#### Online Multiplayer
1. Build the game
2. Launch first instance → Host Game
3. Launch second instance → Join Game
4. Enter host IP address (127.0.0.1 for localhost)
5. Start playing!

## Network Architecture

### Simple Client-Server Model
- **Host**: Acts as both server and client
- **Client**: Connects to host
- **Local**: All players on same instance (no network needed)

### Event Synchronization
Game events are synchronized using the EventBus:
```csharp
// Send networked event
NetworkManager.Instance.SendNetworkEvent(new DiceRolledEvent { result = 6 });
```

### Turn Management
- MultiplayerGameController tracks whose turn it is
- Only the active player can interact
- Turn transitions are synchronized across all clients

## Extending the System

### Adding New Network Events
1. Define event struct in `Events/GameEvents.cs`
2. Use `NetworkManager.SendNetworkEvent<T>()` to broadcast
3. Subscribe to events in relevant controllers

### Custom Network Behavior
The `NetworkManager` can be extended for:
- Player authentication
- Matchmaking
- Leaderboards
- Chat systems

## Troubleshooting

### "NetworkManager not found"
- Make sure NetworkManager persists between scenes (uses DontDestroyOnLoad)
- Check that scenes are built with **LAS → Build All Scenes**

### "Scene not in build settings"
- Run **LAS → Configure Build Settings**
- Or manually add scenes via **File → Build Settings**

### Reference errors still occurring
- Delete Library folder
- Reimport all assets
- Rebuild scenes with **LAS → Build All Scenes**

## Requirements

- Unity 2021.3 or later
- TextMeshPro package (imported automatically)
- .NET Standard 2.1 API compatibility level

## Performance Notes

- Network traffic is minimal (event-based)
- Local multiplayer has zero network overhead
- Recommended for 2-4 players (configurable to support more)

## Future Enhancements

Potential improvements:
- Dedicated server support
- Relay server for NAT traversal
- Lobby system
- Player cosmetics/customization
- Replay system
- AI opponents

---

**Need Help?**
Check the source code comments for detailed implementation notes.
All networking code is in `Assets/Scripts/Networking/`
