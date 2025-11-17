# Online Multiplayer Setup Guide

This guide will help you configure online multiplayer for your Ladders and Snakes game using Unity Netcode for GameObjects.

## 📋 Prerequisites

- Unity Netcode for GameObjects 2.7.0 (already installed in `Packages/manifest.json`)
- Unity 2021.3 or later
- Basic understanding of Unity networking concepts

## 🚀 Quick Setup Steps

### Step 1: Add Unity NetworkManager to MainMenu Scene

1. Open the **MainMenu** scene
2. Create a new GameObject (GameObject → Create Empty)
3. Rename it to `NetworkManager`
4. Add the **NetworkManager** component:
   - Click "Add Component"
   - Search for "NetworkManager" (from Unity.Netcode)
   - Add it to the GameObject
5. Add the **UnityTransport** component:
   - Click "Add Component"
   - Search for "Unity Transport"
   - Add it to the GameObject
6. Configure the NetworkManager:
   - In the NetworkManager component, find the **Transport** field
   - Drag the UnityTransport component into this field
7. Save the scene

### Step 2: Add Unity NetworkManager to GameScene

1. Open the **GameScene** scene
2. Create a new GameObject (GameObject → Create Empty)
3. Rename it to `NetworkManager`
4. Add the **NetworkManager** component (same as Step 1)
5. Add the **UnityTransport** component (same as Step 1)
6. Link the transport in the NetworkManager component
7. **Important**: Check the box "Don't Destroy" on the NetworkManager component to prevent it from being destroyed when loading scenes
8. Save the scene

### Step 3: Set Up NetworkedGameManager

1. In the **MainMenu** scene:
   - Create a new GameObject (GameObject → Create Empty)
   - Rename it to `NetworkedGameManager`
   - Add the **NetworkedGameManager** component (from Scripts/Networking/)
   - This will be created automatically at runtime, but you can add it manually for testing

2. In the **GameScene** scene:
   - Create a new GameObject (GameObject → Create Empty)
   - Rename it to `NetworkedGameManager`
   - Add the **NetworkedGameManager** component
   - This needs to be a **NetworkObject** to work with Netcode:
     - Add Component → Search "Network Object"
     - Add the NetworkObject component
   - Save the scene

### Step 4: Update GameController in GameScene

1. Find the GameObject with the **GameController** or **MultiplayerGameController** component
2. Replace it with **NetworkedMultiplayerGameController**:
   - Remove the old GameController component
   - Add the **NetworkedMultiplayerGameController** component (from Scripts/Networking/)
3. Configure the component:
   - Set "Require Networking" to true for online games
   - Set to false for local/offline games
4. Save the scene

### Step 5: Set Up Network Prefabs (Optional but Recommended)

For full network synchronization, you may want to make player pieces and dice network objects:

1. **Player Pieces**:
   - Select a player piece prefab
   - Add the **NetworkObject** component
   - Add the **NetworkTransform** component (for position synchronization)
   - Save the prefab

2. **Dice**:
   - Select the dice GameObject or prefab
   - Add the **NetworkDiceController** component (from Scripts/Networking/)
   - This handles networked dice rolling
   - Save

3. **Register Prefabs**:
   - Go to the NetworkManager GameObject
   - In the NetworkManager component, find "Network Prefabs List"
   - Add any prefabs that have NetworkObject components

### Step 6: Configure Port and Connection Settings (Optional)

1. Open the **NetworkedGameManager** script or inspector
2. Configure settings:
   - **Max Players**: Default is 4
   - **Port**: Default is 7777
   - Change these in the inspector if needed

## 🎮 How to Use

### Hosting a Game

1. Run the game
2. Click "Play Online"
3. Click "Host Game"
4. The game will start as a host (server + client)
5. Share your IP address with other players

### Joining a Game

1. Run the game
2. Click "Play Online"
3. Enter the host's IP address in the "Server Address" field
4. Click "Join Game"
5. You will connect to the host's game

### Testing Locally

You can test multiplayer on the same machine:

1. **Host**:
   - Build the game (File → Build Settings → Build)
   - Run the built executable
   - Click "Play Online" → "Host Game"

2. **Client**:
   - In Unity Editor, press Play
   - Click "Play Online"
   - Enter "127.0.0.1" as server address
   - Click "Join Game"

3. Alternatively, build two executables and run them both

## 🔧 Advanced Configuration

### Custom Transport Settings

1. Select the NetworkManager GameObject
2. Find the UnityTransport component
3. Configure:
   - **Connection Data**: Address and Port
   - **Timeout**: Connection timeout settings
   - **Debug Simulator**: For testing latency and packet loss

### Network Logging

Enable detailed network logs for debugging:

1. Select NetworkManager
2. In the NetworkManager component:
   - Set **Log Level** to "Developer" or "Normal"
3. Check the Console for detailed network events

### Firewall Configuration

For online play across different networks:

1. **Host**: Open port 7777 (or your custom port) in your firewall
2. **Host**: Forward port 7777 in your router settings
3. **Client**: No special configuration needed

## 🐛 Troubleshooting

### "NetworkManager.Singleton is null"

**Solution**: Make sure you have added the Unity NetworkManager component to your scene (see Step 1 and 2).

### "Failed to start host" or "Failed to connect"

**Solutions**:
- Check that the port is not already in use
- Verify firewall settings
- Make sure the server address is correct
- Check that both host and client are using the same Unity Netcode version

### Players not synchronizing

**Solutions**:
- Ensure NetworkedGameManager has a NetworkObject component
- Check that the NetworkManager's "Network Prefabs List" includes all networked prefabs
- Verify that all clients are connected (check console logs)

### Dice rolls not syncing

**Solutions**:
- Add the NetworkDiceController component to your dice GameObject
- Ensure the NetworkedGameManager is active and running
- Check that it's the local player's turn before rolling

## 📝 Code Integration Examples

### Checking if it's the local player's turn:

```csharp
var gameController = FindObjectOfType<NetworkedMultiplayerGameController>();
if (gameController != null && gameController.CanPlayerAct())
{
    // Allow dice roll or other actions
}
```

### Broadcasting custom events:

```csharp
// In NetworkedGameManager, add:
[ServerRpc(RequireOwnership = false)]
public void BroadcastCustomEventServerRpc(int data)
{
    BroadcastCustomEventClientRpc(data);
}

[ClientRpc]
private void BroadcastCustomEventClientRpc(int data)
{
    // Handle event on all clients
}
```

## 🎯 Next Steps

1. **Test locally**: Use two instances to verify basic connectivity
2. **Test over LAN**: Connect two computers on the same network
3. **Test over Internet**: Configure port forwarding and test with a friend
4. **Add lobby UI**: Create a waiting room for players to ready up
5. **Add chat**: Implement player communication
6. **Add reconnection**: Handle disconnects gracefully

## 📚 Additional Resources

- [Unity Netcode Documentation](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Transport Package](https://docs.unity3d.com/Packages/com.unity.transport@latest)
- [Netcode for GameObjects Samples](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects)

## 🔐 Security Notes

- The current implementation is for development/testing
- For production:
  - Implement authentication
  - Add anti-cheat measures
  - Use secure connections (DTLS)
  - Validate all server-side inputs
  - Rate limit RPCs to prevent abuse

---

**Need help?** Check the Unity Console for detailed error messages and network logs.
