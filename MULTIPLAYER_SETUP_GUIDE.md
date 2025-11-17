# Multiplayer Setup Guide - Ladders and Snakes

This guide provides step-by-step instructions for setting up Photon Fusion multiplayer in your Ladders and Snakes game.

## Architecture Overview

```
MainMenu Scene:
  └─ MainMenuController → Sets GameConfiguration

GameScene:
  ├─ NetworkGameManager (Creates & manages NetworkRunner)
  │   └─ Spawns → NetworkGameState Prefab
  │
  ├─ ManualGameManager (Local game logic)
  │   └─ Receives events from NetworkGameState
  │
  └─ NetworkGameState (Networked, syncs state)
      └─ Broadcasts: Dice rolls, Turn changes, Game end
```

---

## Part 1: Scene Setup (GameScene)

### Step 1: Create Network Manager GameObject

1. **In GameScene hierarchy**, create a new empty GameObject
2. Name it: `NetworkManager`
3. Add component: `NetworkGameManager` script

### Step 2: Create NetworkGameState Prefab

1. **Create new GameObject in scene** (temporary)
   - Name: `NetworkGameState`
   - Position: (0, 0, 0)

2. **Add NetworkObject component**
   - Click "Add Component" → Search "Network Object"
   - This is a Photon Fusion component

3. **Add NetworkGameState script**
   - Click "Add Component" → Search "NetworkGameState"

4. **Create Prefab**
   - Drag `NetworkGameState` GameObject from Hierarchy → Project window (Assets/Prefabs folder)
   - Delete the GameObject from the scene (it will be spawned at runtime)

5. **Important**: The prefab should be in a "Resources" folder or registered with Fusion for network spawning

---

## Part 2: Component Configuration

### NetworkGameManager (on NetworkManager GameObject)

**Inspector Settings:**

```
Network Settings:
  └─ Room Name: "LaddersAndSnakes" (or leave default)
  └─ Max Players: 2 (or 3-4 if you want more players)
  └─ Game Version: "1.0"

UI References: (Optional - leave empty if auto-starting from menu)
  └─ Lobby Panel: [Leave empty if using MainMenu flow]
  └─ Host Button: [Leave empty]
  └─ Join Button: [Leave empty]
  └─ Game Panel: [Leave empty]

Game References:
  └─ Game Manager: Drag your ManualGameManager GameObject here
  └─ Network State Prefab: Drag NetworkGameState prefab here
```

**How it works:**
- Auto-detects multiplayer mode from `GameConfiguration`
- Spawns `NetworkGameState` prefab when hosting
- Finds spawned `NetworkGameState` when joining
- Configures `ManualGameManager` with network state

---

### NetworkGameState Prefab Configuration

**1. NetworkObject Component Settings:**

```
Allow State Authority Override: ☐ (unchecked)
Destroy When State Authority Leaves: ☐ (unchecked)
```

**2. NetworkGameState Component:**

No inspector settings needed - all properties are `[Networked]` and managed at runtime.

**Networked Properties (automatically synced):**
- `CurrentPlayer` - Current turn (0-based index)
- `LastDiceRoll` - Last dice result
- `IsRolling` - Is someone currently rolling
- `NumberOfPlayers` - Total players in game
- `WinnerIndex` - Winner player index (-1 if no winner)
- `GameEnded` - Has game ended
- `Player0Position` to `Player3Position` - Board positions

---

### ManualGameManager Configuration

**Inspector Settings:**

```
Multiplayer:
  └─ Is Multiplayer: ☐ (Leave unchecked - set at runtime)
  └─ Local Player Index: 0 (Will be set by NetworkGameManager)
  └─ Network State: [Leave empty - set at runtime]
```

**Runtime Configuration:**
The `NetworkGameManager` calls:
```csharp
gameManager.ConfigureMultiplayer(networkState, localPlayerIndex, totalPlayers);
```

**What happens:**
1. Sets `isMultiplayer = true`
2. Sets `localPlayerIndex` (0 for host, 1 for first client, etc.)
3. Subscribes to `NetworkGameState` events:
   - `OnDiceRolled` → Updates dice display, moves player
   - `OnTurnChanged` → Updates current turn, enables/disables button
   - `OnGameEnded` → Shows winner

---

## Part 3: Network Flow Diagram

### When Player Clicks "Host" in Main Menu:

```
1. MainMenuController.OnHostGame()
   └─ GameConfiguration.SetMultiplayerMode(isHost: true)
   └─ Load GameScene

2. GameScene loads
   └─ NetworkGameManager.Start()
       └─ Detects GameConfiguration.IsMultiplayer = true
       └─ AutoStartFromConfiguration()
           └─ StartGame(GameMode.Host)
               └─ Creates NetworkRunner
               └─ Starts as Host
               └─ OnGameStarted()
                   └─ SpawnNetworkState()
                       └─ Spawns NetworkGameState prefab
                       └─ ConfigureGameManager()
                           └─ gameManager.ConfigureMultiplayer(networkState, 0, maxPlayers)
```

### When Player Clicks "Join" in Main Menu:

```
1. MainMenuController.OnJoinGame()
   └─ GameConfiguration.SetMultiplayerMode(isHost: false)
   └─ Load GameScene

2. GameScene loads
   └─ NetworkGameManager.Start()
       └─ Detects GameConfiguration.IsMultiplayer = true
       └─ AutoStartFromConfiguration()
           └─ StartGame(GameMode.Client)
               └─ Creates NetworkRunner
               └─ Starts as Client (joins host's session)
               └─ OnGameStarted()
                   └─ TryFindNetworkState()
                       └─ Finds NetworkGameState spawned by host
                       └─ ConfigureGameManager()
                           └─ gameManager.ConfigureMultiplayer(networkState, 1, maxPlayers)
```

### When Player Rolls Dice:

```
Host (Player 0):
1. Player clicks Roll button
2. ManualGameManager.OnRollButtonPressed()
   └─ Checks: currentPlayer == localPlayerIndex (0 == 0) ✓
   └─ networkState.RequestRollDice()
       └─ NetworkGameState.RPC_RollDice() [RpcTarget: StateAuthority]
           └─ Generates random dice result (1-6)
           └─ RPC_BroadcastDiceResult(result) [RpcTarget: All]
               └─ Invokes OnDiceRolled event on ALL clients
                   └─ ManualGameManager.OnNetworkDiceRolled(result)
                       └─ Shows dice result
                       └─ Moves player
                       └─ (Host only) NextTurn()
                           └─ NetworkGameState.NextTurn()
                               └─ RPC_BroadcastTurnChange(newPlayer) [RpcTarget: All]
                                   └─ Invokes OnTurnChanged event on ALL clients
                                       └─ ManualGameManager.OnNetworkTurnChanged(1)
                                           └─ Updates current player to 1
                                           └─ Updates UI

Client (Player 1):
1. Receives RPC_BroadcastDiceResult → Sees Player 0's movement
2. Receives RPC_BroadcastTurnChange → Turn changes to Player 1
3. Button becomes enabled (currentPlayer == localPlayerIndex: 1 == 1) ✓
4. Player clicks Roll button → Same flow as above
```

---

## Part 4: Critical Settings Checklist

### ✓ GameConfiguration (Runtime, set by MainMenu)
- [x] `IsMultiplayer = true` (set by OnHostGame/OnJoinGame)
- [x] `IsHost = true/false` (true for host, false for client)
- [x] `MaxMultiplayerPlayers = 2` (or more)

### ✓ NetworkGameManager (Inspector)
- [x] Game Manager reference assigned
- [x] Network State Prefab assigned
- [x] Max Players set (2-4)

### ✓ NetworkGameState Prefab (Inspector)
- [x] Has NetworkObject component
- [x] Has NetworkGameState component
- [x] Saved as prefab in Assets/Prefabs or Resources folder

### ✓ ManualGameManager (Inspector)
- [x] All player prefabs assigned
- [x] Dice roller assigned
- [x] UI references assigned (roll button, turn text, etc.)
- [x] Multiplayer section exists (will be configured at runtime)

### ✓ Photon Fusion Settings
- [x] Photon Fusion imported from Unity Asset Store or Package Manager
- [x] App ID configured (if using Photon Cloud)
- [x] Build settings include GameScene

---

## Part 5: Common Issues & Solutions

### Issue 1: "Single player mode: 1 human, 1 AI"
**Cause:** GameConfiguration not set to multiplayer mode
**Solution:** Ensure MainMenuController calls `GameConfiguration.Instance.SetMultiplayerMode()`

### Issue 2: "NetworkGameState not found"
**Cause:** NetworkGameState prefab not assigned or not spawning
**Solution:**
- Check NetworkGameManager → Network State Prefab is assigned
- Verify prefab has NetworkObject component
- Check host is spawning it in OnGameStarted()

### Issue 3: Only Player 1 can play
**Cause:** Both host and client calling NextTurn(), causing double advancement
**Solution:** Already fixed in ManualGameManager.cs - only host calls NextTurn()

### Issue 4: Button always disabled for Player 2
**Cause:** Button state not updated correctly for local player
**Solution:** Ensure UpdateUI() checks `currentPlayer == localPlayerIndex`

### Issue 5: Players can't connect to each other
**Cause:** Photon app settings or network configuration
**Solution:**
- Check Photon App ID is set
- Ensure both clients use same Room Name
- Verify internet connection (if using Photon Cloud)
- For local testing, use `GameMode.Host` and `GameMode.Client` (not AutoHostOrClient)

---

## Part 6: Testing Procedure

### Single Machine Testing (Two Unity Instances)

1. **Build the game:**
   - File → Build Settings → Build (creates executable)

2. **Run two instances:**
   - Launch built executable (Instance 1)
   - Launch Unity Editor Play mode (Instance 2)

3. **Test flow:**
   ```
   Instance 1 (Standalone):
     - Main Menu → Play Online → Host
     - Wait for connection

   Instance 2 (Editor):
     - Main Menu → Play Online → Join
     - Should connect to Instance 1
   ```

4. **Verify:**
   - Both see the game board
   - Player 1 (host) can roll when it's their turn
   - Player 2 (client) can roll when it's their turn
   - Turn alternates properly
   - Both see synchronized movements
   - Game ends when someone wins

### Console Logs to Look For

**Host (Instance 1):**
```
[MainMenu] Hosting game
Game configured for Multiplayer: Host, Room: LaddersAndSnakes
Network configured from GameConfiguration: Room=LaddersAndSnakes, Max=2, IsHost=True
Auto-starting multiplayer as Host...
Starting game in Host mode...
Successfully started in Host mode!
NetworkGameState spawned successfully!
Game manager configured for multiplayer. Local player: 0
Player 1 joined (0-based index: 0)!
Player 2 joined (0-based index: 1)!  ← When client joins
Updated network player count to: 2
```

**Client (Instance 2):**
```
[MainMenu] Joining game at 127.0.0.1
Game configured for Multiplayer: Client, Room: LaddersAndSnakes
Network configured from GameConfiguration: Room=LaddersAndSnakes, Max=2, IsHost=False
Auto-starting multiplayer as Client...
Starting game in Client mode...
Successfully started in Client mode!
Connected to server!
Client found NetworkGameState!
Game manager configured for multiplayer. Local player: 1
```

---

## Part 7: Quick Reference - Component Responsibilities

| Component | Role | Who Has It |
|-----------|------|------------|
| **GameConfiguration** | Stores menu choices (Host/Join) | Singleton (persists between scenes) |
| **NetworkGameManager** | Creates NetworkRunner, spawns NetworkGameState | 1 per scene (GameScene) |
| **NetworkRunner** | Photon Fusion's network engine | Created by NetworkGameManager |
| **NetworkGameState** | Synced game state (turn, dice, positions) | 1 spawned by host, synced to all |
| **ManualGameManager** | Local game logic, UI, movement | 1 per scene (GameScene) |

---

## Part 8: Inspector Screenshot Guide

### NetworkManager GameObject
```
NetworkManager (GameObject)
└─ NetworkGameManager (Component)
    ├─ Network Settings
    │   ├─ Room Name: "LaddersAndSnakes"
    │   ├─ Max Players: 2
    │   └─ Game Version: "1.0"
    ├─ UI References (all empty for auto-start)
    └─ Game References
        ├─ Game Manager: → ManualGameManager (drag from hierarchy)
        └─ Network State Prefab: → NetworkGameState (drag from Project)
```

### NetworkGameState Prefab (in Project window)
```
NetworkGameState (Prefab)
├─ Transform (0, 0, 0)
├─ NetworkObject (Component)
│   ├─ Allow State Authority Override: ☐
│   └─ Destroy When State Authority Leaves: ☐
└─ NetworkGameState (Component)
    └─ (No public fields - all automatic)
```

### ManualGameManager GameObject
```
ManualGameManager (GameObject)
└─ ManualGameManager (Component)
    ├─ Manual Board Setup
    │   └─ Board Squares: [Assign 100 board positions]
    ├─ Player Setup
    │   ├─ Player Prefabs: [Assign player pieces]
    │   └─ Number Of Players: 2
    ├─ Dice Setup
    │   └─ Dice Roller: → ManualDiceRoller
    ├─ Multiplayer
    │   ├─ Is Multiplayer: ☐ (set at runtime)
    │   ├─ Local Player Index: 0 (set at runtime)
    │   └─ Network State: None (set at runtime)
    └─ UI References
        ├─ Roll Dice Button: → RollButton
        ├─ Turn Text: → TurnText
        └─ [etc...]
```

---

## Summary

**Key Points:**
1. MainMenu sets GameConfiguration → GameScene reads it
2. NetworkGameManager spawns NetworkGameState (host) or finds it (client)
3. NetworkGameState syncs turn, dice, positions via RPCs
4. ManualGameManager listens to events, only host advances turns
5. Local player index controls whose turn it is

**Files Modified:**
- ✅ MainMenuController.cs - Sets GameConfiguration
- ✅ ManualGameManager.cs - Host-only turn advancement
- ✅ NetworkGameState.cs - Already correct
- ✅ NetworkGameManager.cs - Already correct

**Ready to test!** Follow the testing procedure above.
