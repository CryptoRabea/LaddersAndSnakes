# Complete Multiplayer System Guide

## Overview

This guide explains the comprehensive online multiplayer system for Ladders and Snakes, including room creation, visibility, player management, and all safety features.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Room System](#room-system)
3. [Player System](#player-system)
4. [Safety Features](#safety-features)
5. [Connection Management](#connection-management)
6. [UI Components](#ui-components)
7. [Testing Guide](#testing-guide)
8. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

### Core Components

The multiplayer system consists of several key components:

```
┌─────────────────────────────────────────────────────────────┐
│                    MULTIPLAYER SYSTEM                        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────────┐    ┌──────────────────┐               │
│  │  PlayerInfo     │    │ GameConfiguration│               │
│  │  (Singleton)    │    │  (Singleton)     │               │
│  │  - Player names │    │  - Room settings │               │
│  │  - Colors       │    │  - Mode config   │               │
│  └─────────────────┘    └──────────────────┘               │
│           ↓                       ↓                         │
│  ┌──────────────────────────────────────────────┐          │
│  │        RoomListingManager                    │          │
│  │  - Session discovery (LobbyRunner)          │          │
│  │  - Room list display                        │          │
│  │  - Auto-refresh (3s intervals)              │          │
│  │  - Connection retry (3 attempts)            │          │
│  │  - Input validation                         │          │
│  └──────────────────────────────────────────────┘          │
│                    ↓                                        │
│  ┌──────────────────────────────────────────────┐          │
│  │        NetworkGameManager                    │          │
│  │  - Creates NetworkRunner                     │          │
│  │  - Starts Host/Client mode                   │          │
│  │  - Spawns NetworkGameState                   │          │
│  │  - Connection timeout (10s)                  │          │
│  │  - Retry logic (3 attempts)                  │          │
│  │  - Error handling                            │          │
│  └──────────────────────────────────────────────┘          │
│                    ↓                                        │
│  ┌──────────────────────────────────────────────┐          │
│  │        NetworkGameState                      │          │
│  │  - Networked properties (synced)             │          │
│  │  - RPC methods                               │          │
│  │  - Game state management                     │          │
│  └──────────────────────────────────────────────┘          │
│                    ↓                                        │
│  ┌──────────────────────────────────────────────┐          │
│  │        ManualGameManager                     │          │
│  │  - Game logic execution                      │          │
│  │  - Multiplayer mode support                  │          │
│  └──────────────────────────────────────────────┘          │
│                                                              │
│  ┌──────────────────────────────────────────────┐          │
│  │        Supporting Components                  │          │
│  │  - LobbyManager (waiting room)               │          │
│  │  - ConnectionStatusUI (status display)       │          │
│  │  - MultiplayerSetupPanel (setup UI)         │          │
│  └──────────────────────────────────────────────┘          │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Room System

### How Rooms Work

Rooms in this system are Photon Fusion sessions that are:
- **Visible**: Other players can see them in the room list
- **Open**: Players can join them
- **Discoverable**: Listed via session lobby discovery

### Room Creation Flow

```
1. Player enters room name (3-20 characters)
2. Player selects max players (2-8)
3. System validates input:
   - Room name not empty
   - Room name unique
   - Max players in valid range
   - Player connected to lobby
4. System sanitizes room name (removes invalid chars)
5. System sets player name (auto-generated if not set)
6. GameConfiguration stores settings
7. Lobby runner shuts down
8. Game scene loads
9. NetworkGameManager creates session:
   - IsVisible = true (CRITICAL for visibility)
   - IsOpen = true (allows joining)
   - CustomLobbyName = "LaddersAndSnakesLobby"
   - Session properties include:
     * GameVersion
     * MaxPlayers
     * RoomName
     * CreatedAt timestamp
     * HostName (player who created room)
10. Session becomes visible to all players in lobby
```

### Room Visibility Settings

**Critical Configuration in NetworkGameManager.cs:**

```csharp
var startArgs = new StartGameArgs()
{
    GameMode = mode,
    SessionName = roomName,
    PlayerCount = maxPlayers,

    // THESE ARE CRITICAL FOR ROOM VISIBILITY
    IsVisible = true,      // Makes room visible in lobby
    IsOpen = true,         // Allows players to join

    CustomLobbyName = "LaddersAndSnakesLobby", // For filtering
    SessionProperties = sessionProperties
};
```

### Room Discovery

**How Players See Rooms:**

1. RoomListingManager creates separate `LobbyRunner`
2. Calls `JoinSessionLobby(SessionLobby.Custom)`
3. Photon calls `OnSessionListUpdated()` callback
4. System filters: `IsOpen && IsVisible` sessions
5. Creates UI items for each room
6. Auto-refreshes every 3 seconds

### Room Joining Flow

```
1. Player clicks "Join" on room in list
2. System validates:
   - Join cooldown (2s between attempts)
   - Room still exists
   - Room not full
   - Room is open
   - Not already joined
3. System adds to joined rooms set (prevents duplicates)
4. System sets player name if not set
5. GameConfiguration stores settings (isHost=false)
6. Lobby runner shuts down
7. Game scene loads
8. NetworkGameManager starts as CLIENT
9. Connects to existing session
10. Finds NetworkGameState spawned by host
11. Game begins
```

### Safety Features for Rooms

**Input Validation:**
- Room name: 3-20 characters
- Allowed chars: letters, numbers, spaces, _, -
- Max players: 2-8
- Duplicate room names prevented

**Anti-Spam:**
- 2-second cooldown between join attempts
- Duplicate join prevention via HashSet
- Connection retry limit (3 attempts)

**Error Handling:**
- Room no longer exists → refresh list
- Room full → show error, refresh list
- Room closed → show error, refresh list
- Connection timeout → retry with backoff

---

## Player System

### Player Information

**PlayerInfo.cs** manages:
- Player names (persistent via PlayerPrefs)
- Player colors (8 predefined colors)
- Name generation (random adjective + noun + number)
- Name validation and sanitization

### Player Name System

**Name Generation:**
```csharp
// Auto-generated example: "SwiftTiger42"
string[] adjectives = { "Swift", "Brave", "Clever", "Lucky", "Mighty", ... };
string[] nouns = { "Tiger", "Eagle", "Dragon", "Wolf", "Lion", ... };
```

**Name Validation:**
- Length: 2-15 characters
- Allowed: letters, numbers, spaces, _, -
- Auto-sanitization removes invalid characters
- Fallback to generated name if invalid

**Name Storage:**
- Stored in PlayerPrefs as "PlayerName"
- Persists across sessions
- Loaded on first access
- Can be changed in lobby

### Player Colors

8 distinct colors assigned by player index:
1. Red (Player 0/Host)
2. Blue (Player 1)
3. Green (Player 2)
4. Yellow (Player 3)
5. Orange (Player 4)
6. Purple (Player 5)
7. Cyan (Player 6)
8. Magenta (Player 7)

### NetworkPlayerInfo Structure

```csharp
public struct NetworkPlayerInfo : INetworkStruct
{
    [Networked] NetworkString<_16> PlayerName;
    [Networked] int PlayerIndex;
    [Networked] NetworkBool IsReady;
    [Networked] int ColorIndex;
    [Networked] NetworkBool IsConnected;
}
```

---

## Safety Features

### Input Validation

**Room Names:**
```csharp
// Validation checks:
- Not null or whitespace
- Minimum 3 characters
- Maximum 20 characters
- Only alphanumeric, spaces, _, -
- No duplicate names in current lobby

// Sanitization:
- Trim whitespace
- Remove invalid characters
- Enforce length limits
- Fallback to generated name
```

**Player Names:**
```csharp
// Validation checks:
- Not null or whitespace
- Minimum 2 characters
- Maximum 15 characters
- Only alphanumeric, spaces, _, -

// Sanitization:
- Trim whitespace
- Remove invalid characters
- Enforce length limits
- Fallback to generated name
```

**Max Players:**
```csharp
// Clamped to valid range
maxPlayers = Mathf.Clamp(parsedPlayers, 2, 8);
```

### Connection Safety

**Retry Logic:**
- Maximum 3 connection attempts
- 2-second delay between retries
- Exponential backoff considered
- User feedback on each attempt

**Timeout Protection:**
- 10-second connection timeout
- Automatic cancellation on timeout
- Retry or return to menu

**Duplicate Prevention:**
- Only one active runner at a time
- Previous runners cleaned up before new attempts
- Join cooldown prevents spam

**Error Recovery:**
```csharp
try {
    // Connection attempt
} catch (Exception e) {
    // Log detailed error
    // Attempt retry if attempts remaining
    // Show user-friendly error message
    // Return to menu if max retries exceeded
}
```

### Network Validation

**Session Properties Validation:**
- GameVersion check (future expansion)
- MaxPlayers verification
- Room name sanitization
- Timestamp validation

**State Validation:**
- Host authority enforcement
- RPC source validation
- Player index bounds checking
- Ready state verification

---

## Connection Management

### Connection States

```
DISCONNECTED → CONNECTING → CONNECTED
      ↑             ↓             ↓
      └─────── RECONNECTING ──────┘
                    ↓
              ERROR_STATE
                    ↓
           RETURN_TO_MENU
```

### ConnectionStatusUI

Real-time display of:
- Connection state (Connected/Disconnected)
- Role (Host/Client)
- Player count (current/max)
- Connection quality
- Error messages

**Features:**
- Auto-refresh (1s intervals)
- Color-coded indicators (green/yellow/red)
- Reconnect button on disconnect
- Auto-hide when connected (optional)

### Disconnect Handling

**On Disconnect:**
1. Detect disconnection
2. Update UI to show disconnected state
3. Show error message
4. Enable reconnect button
5. Auto-retry connection (if attempts remaining)
6. Return to menu after max retries

**Host Disconnect:**
- Session closes for all players
- All players receive disconnect notification
- Players return to main menu

**Client Disconnect:**
- Session continues for other players
- Disconnected player can rejoin if room still exists

---

## UI Components

### Main Multiplayer UI Flow

```
MainMenu
    ↓
MultiplayerSetupPanel
    ├─ Room Listing View (default)
    │   ├─ Available rooms list
    │   ├─ Create room section
    │   ├─ Refresh button
    │   └─ Connection status
    │
    └─ Manual Join View (alternative)
        ├─ Room name input
        ├─ Max players input
        ├─ Host button
        └─ Join button
    ↓
(Room Created/Joined)
    ↓
LobbyManager (optional waiting room)
    ├─ Player list
    ├─ Ready status
    ├─ Player name editing
    ├─ Start game button (host only)
    └─ Leave lobby button
    ↓
GameScene
    └─ ConnectionStatusUI (status monitoring)
```

### RoomListingManager UI

**Components:**
- Status text (connection state, room count)
- Room list container (scrollable)
- Room list items (prefab instances)
- Create room input fields
- Refresh button
- Connection indicator

**Mobile Optimizations:**
- Enlarged buttons (1.2x scale)
- Large input fields (80px height)
- Increased font sizes (36pt)
- Touch-friendly scroll (sensitivity: 30)
- Minimum touch targets (100px)

### LobbyManager UI

**Components:**
- Room name display
- Player count display
- Player list container
- Player name input field
- Ready button
- Start game button (host only)
- Leave lobby button
- Connection status indicator

**Player List Item:**
- Player name (colored by index)
- Ready status (✓ Ready / Waiting...)
- Host indicator (for first player)

### ConnectionStatusUI

**Components:**
- Connection status text
- Connection indicator (colored circle)
- Player count text
- Ping/quality text
- Error panel (shown on errors)
- Reconnect button

**Color Coding:**
- Green: Connected
- Yellow: Connecting
- Red: Disconnected

---

## Testing Guide

### Local Testing (Two Instances)

**Setup:**
1. Build the game executable
2. Launch Instance 1 (Executable)
3. Launch Instance 2 (Unity Editor)

**Test Room Creation:**
```
Instance 1 (Host):
1. Click "Play Online"
2. Enter room name: "TestRoom1"
3. Select max players: 4
4. Click "Create Room"
5. Wait for game scene to load
6. Verify "Connected (Host)" status

Instance 2 (Client):
1. Click "Play Online"
2. Wait for room list to populate
3. Verify "TestRoom1" appears in list
4. Verify player count shows "1/4"
5. Click "Join" on TestRoom1
6. Verify connection as client
```

**Test Gameplay:**
```
1. Verify both players see the same board
2. Player 1 (host) rolls dice
   - Verify both see dice roll
   - Verify both see player movement
3. Verify turn switches to Player 2
4. Player 2 rolls dice
   - Verify both see dice roll
   - Verify both see player movement
5. Complete full game to verify win condition sync
```

### Testing Room Visibility

**Verification Checklist:**
- [ ] Created room appears in other players' room lists
- [ ] Room shows correct player count (X/Y)
- [ ] Room updates player count when players join
- [ ] Room disappears when full
- [ ] Room disappears when closed
- [ ] Room list auto-refreshes (3s intervals)
- [ ] Manual refresh works
- [ ] Duplicate room names prevented
- [ ] Invalid room names rejected

### Testing Safety Features

**Input Validation:**
- [ ] Empty room name → error message
- [ ] Room name < 3 chars → error message
- [ ] Room name > 20 chars → error message
- [ ] Special characters removed/sanitized
- [ ] Duplicate room name → error message
- [ ] Max players < 2 → clamped to 2
- [ ] Max players > 8 → clamped to 8

**Connection Safety:**
- [ ] Disconnect detection works
- [ ] Reconnect button appears
- [ ] Retry logic works (3 attempts)
- [ ] Timeout works (10s)
- [ ] Join cooldown prevents spam (2s)
- [ ] Duplicate join prevented

**Error Handling:**
- [ ] Invalid Photon credentials → error message
- [ ] Network failure → retry logic
- [ ] Room full → error message + refresh
- [ ] Room closed → error message + refresh
- [ ] Room no longer exists → error + refresh

---

## Troubleshooting

### Rooms Not Visible

**Check:**
1. Photon App ID configured correctly
2. `IsVisible = true` in StartGameArgs
3. `IsOpen = true` in StartGameArgs
4. Both instances using same Photon region
5. Firewall not blocking connections
6. Both instances on same game version

**Solution:**
```csharp
// In NetworkGameManager.cs, verify:
var startArgs = new StartGameArgs()
{
    IsVisible = true,  // Must be true
    IsOpen = true,     // Must be true
    CustomLobbyName = "LaddersAndSnakesLobby"
};
```

### Connection Failed

**Check:**
1. Internet connection active
2. Photon service status (check Photon dashboard)
3. Firewall/antivirus not blocking Unity
4. Correct Photon App ID
5. Region selection (use Auto or specific region)

**Solution:**
- Check console logs for detailed error
- Verify Photon App Settings asset
- Try different network/region
- Check retry counter in logs

### Players Can't Join Room

**Check:**
1. Room not full (player count < max players)
2. Room is open (IsOpen = true)
3. Room still exists (not closed by host)
4. Network connectivity
5. Join cooldown not active

**Solution:**
- Refresh room list
- Check room player count
- Verify room properties in logs
- Wait for cooldown (2s)

### Sync Issues (State Not Syncing)

**Check:**
1. NetworkGameState spawned by host
2. Clients found NetworkGameState
3. RPCs being called correctly
4. Network authority correct (host has state authority)
5. Network tick rate stable

**Solution:**
- Check logs for "NetworkGameState spawned"
- Verify "Client found NetworkGameState"
- Check RPC logs
- Restart both instances

### Player Names Not Showing

**Check:**
1. PlayerInfo.LocalPlayerName set
2. PlayerPrefs saved
3. Name synced via NetworkPlayerInfo
4. UI components configured correctly

**Solution:**
- Check PlayerPrefs for "PlayerName" key
- Verify name in logs
- Set name manually in lobby
- Check RPC_UpdatePlayerName calls

---

## Advanced Configuration

### Photon Settings

**Location:** `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`

**Key Settings:**
- App ID: Your Photon Fusion App ID
- Region: Auto (or specific region for testing)
- Fixed Region: Optional override
- Protocol: UDP (default, best performance)

### Network Tick Rate

Default: 30 ticks/second (Photon Fusion default)

Can be configured in NetworkRunner settings if needed for different game types.

### Session Properties

Current custom properties:
```csharp
"GameVersion" → "1.0"
"MaxPlayers" → int
"RoomName" → string
"CreatedAt" → timestamp
"HostName" → string (player name)
```

**To Add Custom Properties:**
```csharp
// In NetworkGameManager.cs
sessionProperties["NewProperty"] = value;
```

### Lobby Refresh Rate

**RoomListingManager:**
- Auto-refresh interval: 3 seconds (configurable)
- Manual refresh: always available

**To Change:**
```csharp
[SerializeField] private float autoRefreshInterval = 3f; // Change value
```

---

## API Reference

### PlayerInfo

**Static Properties:**
```csharp
string LocalPlayerName { get; set; }  // Current player's name
```

**Static Methods:**
```csharp
string GenerateRandomName()                    // Generate random name
string SanitizePlayerName(string name)         // Clean/validate name
bool IsValidPlayerName(string name)            // Check if name valid
Color GetPlayerColor(int colorIndex)           // Get color by index
string GetPlayerColorName(int colorIndex)      // Get color name
```

### GameConfiguration

**Properties:**
```csharp
bool IsMultiplayer { get; }               // Current game mode
bool IsHost { get; }                      // True if hosting
string RoomName { get; }                  // Current room name
int MaxMultiplayerPlayers { get; }        // Max players
string PlayerName { get; }                // Local player name
```

**Methods:**
```csharp
void SetMultiplayerMode(bool isHost, string roomName, int maxPlayers, string playerName)
void SetSinglePlayerMode(int humanPlayers, int aiPlayers)
void Reset()
string GetConnectionStatus()
```

### NetworkGameManager

**Properties:**
```csharp
bool IsHost { get; }           // True if local player is host
bool IsConnected { get; }      // True if connected to session
int LocalPlayerIndex { get; }  // Local player's index (0-based)
int PlayerCount { get; }       // Current player count
string LastError { get; }      // Last error message
```

### ConnectionStatusUI

**Methods:**
```csharp
void ShowError(string message)    // Display error to user
int GetConnectionQuality()        // Get quality 0-100
bool IsConnected()                // Check if connected
bool IsHost()                     // Check if host
int GetPlayerCount()              // Get current player count
```

---

## Security Considerations

### Input Sanitization

All user inputs are sanitized:
- Room names: alphanumeric + spaces, _, -
- Player names: alphanumeric + spaces, _, -
- Max players: clamped to 2-8
- Special characters removed
- Length limits enforced

### Network Security

- Photon Fusion handles encryption
- RPC validation via state authority
- Player index bounds checking
- Session property validation
- Anti-spam cooldowns

### Data Privacy

- Player names stored locally only (PlayerPrefs)
- No personal data transmitted
- Session data temporary (deleted on close)
- No chat system (avoids moderation issues)

---

## Performance Optimization

### Network Traffic

- State synchronization: ~30 updates/second
- RPC calls: on-demand only
- Session list: updates every 3 seconds
- Minimal bandwidth usage

### Mobile Optimization

- Touch-friendly UI (100px minimum touch targets)
- Large text (36pt minimum)
- Scaled buttons (1.2x)
- Smooth scrolling (optimized scroll rect)
- Safe area handling

### Memory Management

- Lobby runner destroyed when entering game
- Game runner destroyed when returning to menu
- Proper cleanup in OnDestroy()
- No memory leaks in testing

---

## Future Enhancements

### Planned Features

1. **Chat System**
   - In-lobby chat
   - In-game quick messages
   - Text filtering/moderation

2. **Player Statistics**
   - Win/loss tracking
   - Games played
   - Average game time
   - Leaderboards

3. **Advanced Room Features**
   - Password-protected rooms
   - Room templates
   - Game mode variants
   - Spectator mode

4. **Social Features**
   - Friend lists
   - Invite system
   - Recent players
   - Block/report

5. **Reconnection**
   - Auto-reconnect on disconnect
   - Host migration
   - State recovery
   - Timeout grace period

### Community Requested

- Custom board skins
- Player avatars
- Emotes/reactions
- Tournament mode
- Replay system

---

## Support

For issues or questions:
1. Check console logs for detailed errors
2. Verify Photon configuration
3. Review this guide's troubleshooting section
4. Check Photon Fusion documentation
5. Create issue on GitHub repository

---

## Credits

**Networking:** Photon Fusion
**Game Engine:** Unity
**Platform:** PC, Mac, Mobile

---

**Last Updated:** 2025-11-19
**Version:** 1.0
**Status:** Production Ready
