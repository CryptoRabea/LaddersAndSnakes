# Ladders and Snakes - Multiplayer Setup Guide

This guide will help you configure online multiplayer for your Ladders and Snakes game using Photon Fusion.

## Prerequisites

- Unity project with Photon Fusion installed ✅ (Already installed in your project)
- The 3 game scripts: ManualGameManager, ManualDiceRoller, DiceFace ✅
- Network scripts: NetworkGameManager, NetworkGameState ✅

## Step 1: Get Your Photon App ID

1. Go to [Photon Engine Dashboard](https://dashboard.photonengine.com/)
2. Sign up or log in to your account
3. Click "Create New App"
4. Select **Photon Fusion** as the Photon Type
5. Give it a name (e.g., "Ladders and Snakes")
6. Copy your **App ID** (it looks like: `12345678-1234-1234-1234-123456789012`)

## Step 2: Configure Photon in Unity

1. In Unity, go to **Window → Photon Fusion → Fusion Hub**
2. If not already done, click **Setup project for Fusion**
3. Paste your **App ID** when prompted
4. Click **Setup**

Alternatively, you can manually set it:
1. Go to **Assets → Photon → Fusion → Resources**
2. Select **PhotonAppSettings**
3. Paste your App ID in the **App Id Fusion** field

## Step 3: Create NetworkGameState Prefab

1. In Unity Hierarchy, create a new **Empty GameObject**
2. Rename it to `NetworkGameState`
3. Add the **NetworkGameState** component to it
4. Add the **NetworkObject** component (required by Fusion)
   - Click "Add Component" → Search for "Network Object"
5. Drag the GameObject from Hierarchy to your **Assets/Prefabs** folder (create folder if needed)
6. Delete the GameObject from the Hierarchy (we only need the prefab)

## Step 4: Scene Setup

### A. Game Manager Setup

1. Find your existing **ManualGameManager** GameObject in the scene
2. Verify it has the **ManualGameManager** component attached
3. Make sure all the required references are assigned:
   - Board Squares (if using manual squares)
   - Player Prefabs
   - Dice Roller
   - UI References (buttons, texts, panels)

### B. Network Manager Setup

1. Create a new **Empty GameObject** in the scene
2. Rename it to `NetworkManager`
3. Add the **NetworkGameManager** component to it
4. Configure the NetworkGameManager:

   **Network Settings:**
   - Room Name: `LaddersAndSnakes` (or your choice)
   - Max Players: `4` (or 2 for 1v1)
   - Game Version: `1.0`

   **UI References:**
   - Lobby Panel: Drag your lobby UI panel
   - Host Button: Drag the "Host Game" button
   - Join Button: Drag the "Join Game" button
   - Game Panel: Drag your main game UI panel

   **Game References:**
   - Game Manager: Drag your **ManualGameManager** GameObject
   - Network State Prefab: Drag the **NetworkGameState** prefab you created

## Step 5: UI Setup

You need two UI panels:

### Lobby Panel (for matchmaking)
Create a UI panel with:
- **Host Game** button (will start as host)
- **Join Game** button (will join as client)
- Optional: Room name input field

### Game Panel (the actual game)
This is your existing game UI with:
- Roll Dice button
- Turn display
- Dice result display
- Win panel
- etc.

**Important:**
- Lobby Panel should be **active** by default
- Game Panel should be **inactive** by default
- The NetworkGameManager will toggle these automatically

## Step 6: Fusion Scene Configuration

1. Go to **File → Build Settings**
2. Make sure your game scene is added to "Scenes in Build"
3. Note the scene's build index (usually 0 if it's your only scene)

## Step 7: Test Multiplayer

### Testing Locally (Same Computer)

1. **Build the game:**
   - Go to **File → Build Settings**
   - Click **Build** (not Build and Run)
   - Save it somewhere (e.g., Desktop/LaddersAndSnakes.exe)

2. **Run two instances:**
   - Launch the built game (Instance 1)
   - Click "Host Game"
   - Launch the game again (Instance 2)
   - Click "Join Game"

3. **Play:**
   - Player 1 (Host) takes their turn
   - Player 2 (Client) waits, then takes their turn
   - Dice rolls and movements should sync between both instances

### Testing Over Network

1. Build the game
2. Send it to a friend
3. One person clicks "Host Game"
4. The other clicks "Join Game"
5. Both should connect to the same room and play together!

## Step 8: Troubleshooting

### "Failed to start: Invalid App Id"
- Double-check your Photon App ID in PhotonAppSettings
- Make sure you're using a **Fusion** App ID, not PUN2

### "NetworkGameState component not found"
- Make sure your NetworkGameState prefab has both:
  - NetworkGameState component
  - NetworkObject component

### "Players not connecting"
- Check that both players are using the same Room Name
- Verify your internet connection
- Make sure Photon Fusion is properly set up

### "Dice rolls not syncing"
- Check Unity Console for errors
- Verify NetworkGameManager has reference to ManualGameManager
- Make sure NetworkGameState prefab is assigned

### "Turn not changing"
- Check that NetworkGameState has authority (host)
- Look for RPC errors in console
- Verify player indices are correct

## Advanced Configuration

### Customize Number of Dice
In ManualGameManager inspector:
- Set "Number Of Dice" to 1 or 2
- This will work automatically in multiplayer

### Add More Players
1. Set Max Players in NetworkGameManager
2. Add more player prefabs in ManualGameManager
3. Adjust player spacing if needed

### Region Selection
For better connection, you can specify Photon regions:
- In PhotonAppSettings, set Fixed Region
- Options: US, EU, Asia, Japan, etc.

## Network Architecture

Here's how the multiplayer works:

1. **NetworkGameManager**: Handles connection, hosting, joining
2. **NetworkGameState**: Synchronizes game state (turns, positions, dice rolls)
3. **ManualGameManager**: Runs the game logic, receives network events
4. **Host (Server)**: Authoritative - makes all decisions
5. **Clients**: Display synced state, send input requests

### Game Flow:

```
Player clicks Roll Dice
    ↓
ManualGameManager.OnRollButtonPressed()
    ↓
NetworkGameState.RequestRollDice() [RPC to host]
    ↓
Host generates random dice result
    ↓
Host broadcasts result to all clients [RPC to all]
    ↓
All clients receive result and animate dice
    ↓
Player piece moves (synced across all clients)
    ↓
Next turn [RPC to all]
```

## Files Overview

| File | Purpose |
|------|---------|
| **ManualGameManager.cs** | Main game logic, handles local and network play |
| **ManualDiceRoller.cs** | Dice rolling mechanics (visual only in multiplayer) |
| **DiceFace.cs** | Dice value detection |
| **NetworkGameManager.cs** | Photon Fusion connection manager |
| **NetworkGameState.cs** | Synchronized game state (NetworkBehaviour) |

## Support

If you encounter issues:
1. Check Unity Console for errors
2. Enable Debug.Log messages (they're already in the code)
3. Test in single-player first to verify game logic works
4. Then test multiplayer locally (two builds on same PC)
5. Finally test over internet

## Next Steps

- Add player names/avatars
- Add chat functionality
- Add reconnection handling
- Add spectator mode
- Add match history/stats

Enjoy your multiplayer Ladders and Snakes game! 🎲🎮
