# 🌐 Online Multiplayer for Ladders and Snakes

Welcome to the online multiplayer implementation for your Ladders and Snakes game! This implementation uses **Unity Netcode for GameObjects** to enable true client-server multiplayer gameplay.

## 📦 What's Included

### New Scripts
- **`NetworkedGameManager.cs`** - Core networking manager using Unity Netcode
  - Handles client/server connections
  - Manages player sessions
  - Broadcasts game events via RPCs
  - Location: `Assets/Scripts/Networking/`

- **`NetworkedMultiplayerGameController.cs`** - Networked game controller
  - Extends the base GameController with network support
  - Manages turn synchronization
  - Handles local vs networked gameplay
  - Location: `Assets/Scripts/Networking/`

- **`NetworkDiceController.cs`** - Networked dice rolling
  - Synchronizes dice rolls across all clients
  - Validates player turns before allowing rolls
  - Location: `Assets/Scripts/Networking/`

- **`NetworkConnectionUI.cs`** - Connection status UI
  - Shows connection status, player count, current turn
  - Displays local player index
  - Provides disconnect functionality
  - Location: `Assets/Scripts/UI/`

### Updated Scripts
- **`MainMenuController.cs`** - Updated to support Unity Netcode
  - Host/Join functionality integrated
  - Connection error handling
  - Location: `Assets/Scripts/UI/`

### Editor Tools
- **`MultiplayerSetupHelper.cs`** - Automated setup tool
  - Menu: LAS → Multiplayer → Auto Setup Scene
  - Quick menu items for adding network components
  - Location: `Assets/Scripts/Editor/`

### Documentation
- **`MULTIPLAYER_QUICKSTART.md`** - 5-minute quick start guide
- **`MULTIPLAYER_SETUP_GUIDE.md`** - Comprehensive setup and troubleshooting
- **`MULTIPLAYER_README.md`** - This file

## 🚀 Quick Start

**Choose your path:**

1. **⚡ Fast Track** (5 minutes): Follow [MULTIPLAYER_QUICKSTART.md](MULTIPLAYER_QUICKSTART.md)
2. **📖 Detailed Setup** (15 minutes): Follow [MULTIPLAYER_SETUP_GUIDE.md](MULTIPLAYER_SETUP_GUIDE.md)
3. **🤖 Automated** (2 minutes): Use `LAS → Multiplayer → Auto Setup Scene` in Unity Editor

## 🎮 How It Works

### Architecture

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│   Client 1  │         │   Client 2  │         │   Client 3  │
│  (Player 1) │         │  (Player 2) │         │  (Player 3) │
└──────┬──────┘         └──────┬──────┘         └──────┬──────┘
       │                       │                       │
       │    Network RPCs       │                       │
       └───────────┬───────────┴───────────────────────┘
                   │
            ┌──────▼──────┐
            │   Host      │
            │  (Server +  │
            │   Client 0) │
            └─────────────┘
```

### Network Flow

1. **Connection**:
   - Host starts server + client
   - Clients connect to host
   - NetworkedGameManager assigns player indices

2. **Game Start**:
   - All players auto-ready
   - Game starts when minimum players connected (2+)

3. **Gameplay**:
   - Current player rolls dice (local action)
   - Roll broadcasted via ServerRpc → ClientRpc
   - All clients receive and process the event
   - Game state synchronized across all clients

4. **Turn Management**:
   - Host tracks current turn
   - Only current player can interact
   - Turn changes broadcast to all clients

### Key Components

#### NetworkedGameManager
- Singleton pattern for easy access
- RPCs for all game events (dice rolls, moves, turns, game over)
- Player session management
- Connection lifecycle handling

#### NetworkedMultiplayerGameController
- Extends base GameController
- Validates local player actions
- Broadcasts events to network
- Fallback to local mode if not networked

#### NetworkDiceController
- Listens for dice roll requests
- Validates player turn
- Broadcasts results

## 🔧 Configuration

### Default Settings
- **Port**: 7777
- **Max Players**: 4
- **Transport**: Unity Transport (UTP)
- **Protocol**: UDP

### Customization
All settings can be modified in the NetworkedGameManager inspector:
- Maximum players (2-8 recommended)
- Port number
- Connection timeout

## 📋 Requirements

- Unity 2021.3 or later
- Unity Netcode for GameObjects 2.7.0 (already installed)
- Unity Transport Package (already installed)

## 🧪 Testing

### Local Testing (Same Computer)
1. Build the game
2. Run the build as Host
3. Run Unity Editor as Client
4. Connect to "127.0.0.1"

### LAN Testing
1. Host: Find local IP (192.168.x.x)
2. Client: Connect to host's local IP

### Internet Testing
1. Host: Forward port 7777 in router
2. Host: Share public IP
3. Client: Connect to public IP

## 🐛 Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| "NetworkManager.Singleton is null" | Add Unity NetworkManager to scene |
| "Failed to start host" | Check port 7777 is not in use |
| "Failed to connect" | Verify firewall settings and IP address |
| Dice not syncing | Add NetworkDiceController to dice |
| Players not syncing | Ensure NetworkedGameManager has NetworkObject |

See [MULTIPLAYER_SETUP_GUIDE.md](MULTIPLAYER_SETUP_GUIDE.md) for detailed troubleshooting.

## 🔐 Security Considerations

**Current Implementation**: Development/Testing
- No authentication
- No anti-cheat
- Unencrypted connections
- Trust-based validation

**Production Recommendations**:
- Implement player authentication
- Add server-side validation
- Enable DTLS encryption
- Rate limit RPCs
- Add anti-cheat measures
- Use dedicated servers

## 🎯 Features

### ✅ Implemented
- [x] Client-server architecture
- [x] Player connection management
- [x] Turn-based synchronization
- [x] Networked dice rolls
- [x] Game state synchronization
- [x] Player join/leave handling
- [x] Connection UI
- [x] Local fallback mode

### 🔄 Future Enhancements
- [ ] Lobby/waiting room
- [ ] Player chat
- [ ] Reconnection support
- [ ] Match-making
- [ ] Game replays
- [ ] Spectator mode
- [ ] Leaderboards
- [ ] Player authentication

## 📚 API Reference

### NetworkedGameManager

```csharp
// Start hosting
NetworkedGameManager.Instance.StartHost();

// Connect as client
NetworkedGameManager.Instance.StartClient("192.168.1.100");

// Disconnect
NetworkedGameManager.Instance.Disconnect();

// Broadcast dice roll
NetworkedGameManager.Instance.BroadcastDiceRollServerRpc(result, rawRoll);

// Get player count
int count = NetworkedGameManager.Instance.PlayerCount;

// Get local player index
int index = NetworkedGameManager.Instance.LocalPlayerIndex;
```

### NetworkedMultiplayerGameController

```csharp
// Check if local player's turn
bool isMyTurn = gameController.IsLocalPlayerTurn();

// Check if player can act
bool canAct = gameController.CanPlayerAct();

// Check if networked game
bool isNetworked = gameController.IsNetworkedGame();

// Get local player index
int myIndex = gameController.GetLocalPlayerIndex();
```

## 🤝 Contributing

To extend the multiplayer functionality:

1. Add new game events to `GameEvents.cs`
2. Create corresponding RPCs in `NetworkedGameManager.cs`
3. Update `NetworkedMultiplayerGameController.cs` to broadcast events
4. Test locally, then LAN, then internet

## 📖 Unity Netcode Resources

- [Official Documentation](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Transport](https://docs.unity3d.com/Packages/com.unity.transport@latest)
- [Netcode Samples](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects)
- [Community Discord](https://discord.gg/unity)

## 📝 License

This multiplayer implementation follows the same license as your main project.

## ❓ Support

Issues or questions? Check:
1. [MULTIPLAYER_QUICKSTART.md](MULTIPLAYER_QUICKSTART.md)
2. [MULTIPLAYER_SETUP_GUIDE.md](MULTIPLAYER_SETUP_GUIDE.md)
3. Unity Console logs (enable Developer mode)
4. Unity Multiplayer Documentation

---

**Ready to play online? Start with [MULTIPLAYER_QUICKSTART.md](MULTIPLAYER_QUICKSTART.md)!** 🎮🌐
