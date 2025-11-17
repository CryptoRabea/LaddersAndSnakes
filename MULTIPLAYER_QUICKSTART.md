# 🎮 Online Multiplayer - Quick Start

Get your Ladders and Snakes game online in **5 minutes**!

## ⚡ Super Quick Setup

### 1. Add NetworkManager to MainMenu Scene (1 min)

```
1. Open MainMenu scene
2. GameObject → Create Empty → Name it "NetworkManager"
3. Add Component → "NetworkManager" (Unity.Netcode)
4. Add Component → "Unity Transport"
5. Drag "Unity Transport" into NetworkManager's "Transport" field
6. Save scene
```

### 2. Add NetworkManager to GameScene (1 min)

```
1. Open GameScene
2. GameObject → Create Empty → Name it "NetworkManager"
3. Add Component → "NetworkManager" (Unity.Netcode)
4. Add Component → "Unity Transport"
5. Drag "Unity Transport" into NetworkManager's "Transport" field
6. ✅ Check "Don't Destroy On Load" in NetworkManager
7. Save scene
```

### 3. Setup NetworkedGameManager (1 min)

```
In GameScene:
1. GameObject → Create Empty → Name it "NetworkedGameManager"
2. Add Component → "NetworkedGameManager" (LAS.Networking)
3. Add Component → "Network Object"
4. Save scene
```

### 4. Replace GameController (1 min)

```
In GameScene:
1. Find GameObject with "GameController" or "MultiplayerGameController"
2. Remove old controller component
3. Add Component → "NetworkedMultiplayerGameController"
4. Set "Require Networking" = true
5. Save scene
```

### 5. Add Network Dice (30 sec)

```
In GameScene:
1. Find Dice GameObject
2. Add Component → "NetworkDiceController"
3. Drag DiceModel into the "Dice Model" field (if present)
4. Save scene
```

### 6. Optional: Add Connection UI (30 sec)

```
In GameScene:
1. Create UI → Canvas (if not exists)
2. Create UI → Panel → Name it "ConnectionPanel"
3. Add Component → "NetworkConnectionUI"
4. Link UI elements in the inspector
5. Save scene
```

## 🎯 Test It!

### Local Test (Same Computer):

**Terminal 1** (Host):
```bash
# Build the game first
# File → Build Settings → Build
./YourGame.exe
# Click "Play Online" → "Host Game"
```

**Terminal 2** (Client):
```bash
# Or use Unity Editor (Press Play)
# Click "Play Online"
# Enter "127.0.0.1"
# Click "Join Game"
```

### LAN Test (Different Computers):

**Host**:
```
1. Find your IP: ipconfig (Windows) or ifconfig (Mac/Linux)
2. Note your local IP (e.g., 192.168.1.100)
3. Run game → "Play Online" → "Host Game"
```

**Client**:
```
1. Run game → "Play Online"
2. Enter host's IP (e.g., 192.168.1.100)
3. Click "Join Game"
```

### Internet Test:

**Host**:
```
1. Forward port 7777 in your router
2. Find public IP: whatismyip.com
3. Share public IP with friend
4. Run game → "Host Game"
```

**Client**:
```
1. Enter host's public IP
2. Click "Join Game"
```

## 🐛 Quick Fixes

### "NetworkManager.Singleton is null"
➡️ You forgot Step 1 or 2. Add NetworkManager to the scene!

### "Failed to connect"
➡️ Check firewall, port 7777, and IP address

### "Dice not syncing"
➡️ Add NetworkDiceController to dice (Step 5)

### "Players not syncing"
➡️ NetworkedGameManager needs NetworkObject component (Step 3)

## 📖 Full Documentation

For detailed setup, troubleshooting, and advanced features, see:
- **[MULTIPLAYER_SETUP_GUIDE.md](MULTIPLAYER_SETUP_GUIDE.md)** - Complete setup instructions

## 🎮 Controls

- Host starts the game when all players are ready
- Players take turns rolling dice
- Only the current player can roll
- Game syncs automatically across all clients

## 🔧 Customization

### Change Port:
```csharp
// In NetworkedGameManager inspector
Port = 7777 // Change to your desired port
```

### Change Max Players:
```csharp
// In NetworkedGameManager inspector
Max Players = 4 // Change to 2-8
```

### Enable Debug Logs:
```csharp
// In NetworkManager (Unity.Netcode) inspector
Log Level = Developer
```

## 🎉 You're Done!

Your game now supports:
- ✅ Host/Client architecture
- ✅ Turn-based synchronization
- ✅ Networked dice rolls
- ✅ Player connection/disconnection handling
- ✅ Automatic game state sync

## 📚 Next Steps

- Add lobby/waiting room
- Implement player chat
- Add reconnection handling
- Create match-making system
- Deploy to a dedicated server

**Happy Networking! 🚀**
