# Room Listing Setup Guide

This guide explains how to set up the room creation and listing system for multiplayer in your Ladders and Snakes game.

## Overview

The room listing system allows players to:
1. **Create rooms** with custom names and player limits
2. **Browse available rooms** created by other players
3. **Join rooms** from the list
4. **Manually enter room names** if preferred

## Components Created

### Scripts
1. **RoomListingManager.cs** - Manages session discovery and room listing
2. **RoomListItem.cs** - UI component for individual room entries
3. **MultiplayerSetupPanel.cs** - Updated to support room listing

## UI Setup Instructions

### 1. Main Menu Scene Setup

In your MainMenu scene, you need to configure the MultiplayerSetupPanel with the following UI structure:

#### A. Room Listing Panel Structure

Create a panel hierarchy like this:

```
MultiplayerSetupPanel (GameObject with MultiplayerSetupPanel.cs)
├── StatusText (TextMeshProUGUI)
├── RoomListingPanel (GameObject) [Main panel for browsing rooms]
│   ├── RoomListingManager (GameObject with RoomListingManager.cs)
│   ├── CreateRoomSection (Panel)
│   │   ├── TitleText (TextMeshProUGUI) - "Create New Room"
│   │   ├── RoomNameInputField (TMP_InputField) - Enter room name
│   │   ├── MaxPlayersInputField (TMP_InputField) - Enter max players (2-8)
│   │   └── CreateRoomButton (Button) - "Create Room"
│   │
│   ├── AvailableRoomsSection (Panel)
│   │   ├── TitleText (TextMeshProUGUI) - "Available Rooms"
│   │   ├── RefreshButton (Button) - "Refresh List"
│   │   ├── ScrollView (Scroll Rect)
│   │   │   └── Content (Transform) - [Will be populated with room items]
│   │   └── StatusText (TextMeshProUGUI) - Shows connection status
│   │
│   └── SwitchViewButton (Button) - "Enter Room Code Instead"
│
├── ManualJoinPanel (GameObject) [Traditional manual entry]
│   ├── TitleText (TextMeshProUGUI) - "Join or Host Game"
│   ├── RoomNameInputField (TMP_InputField)
│   ├── MaxPlayersInputField (TMP_InputField)
│   ├── HostButton (Button)
│   ├── JoinButton (Button)
│   └── SwitchViewButton (Button) - "Browse Rooms Instead"
│
└── BackButton (Button)
```

#### B. Room List Item Prefab

Create a prefab called **RoomListItemPrefab** with this structure:

```
RoomListItemPrefab (GameObject with RoomListItem.cs)
├── Background (Image)
├── RoomNameText (TextMeshProUGUI)
│   └── Text: "Room Name"
├── PlayerCountText (TextMeshProUGUI)
│   └── Text: "0/4"
└── JoinButton (Button)
    └── Text (TextMeshProUGUI): "Join"
```

**Important**: The child GameObjects must be named exactly:
- `RoomNameText`
- `PlayerCountText`
- `JoinButton`

These names are used by the RoomListingManager to find components.

### 2. Inspector Configuration

#### MultiplayerSetupPanel Component

Assign these references in the Inspector:

**UI References:**
- Room Name Input: `RoomNameInputField`
- Max Players Input: `MaxPlayersInputField`
- Status Text: `StatusText`

**Buttons:**
- Host Button: `HostButton`
- Join Button: `JoinButton`
- Back Button: `BackButton`

**Room Listing:**
- Room Listing Panel: `RoomListingPanel` (GameObject)
- Manual Join Panel: `ManualJoinPanel` (GameObject)
- Show Room List Button: Button to switch to room list view
- Show Manual Join Button: Button to switch to manual entry view
- Room Listing Manager: The RoomListingManager component

**Settings:**
- Game Scene Name: "GameScene"
- Default Max Players: 4

#### RoomListingManager Component

Assign these references in the Inspector:

**UI References:**
- Room List Container: The `Content` transform inside the ScrollView
- Room List Item Prefab: Your RoomListItemPrefab
- Status Text: TextMeshProUGUI to show connection status
- Refresh Button: Button to refresh the room list

**Create Room UI:**
- Create Room Name Input: TMP_InputField for room name
- Max Players Input: TMP_InputField for max players
- Create Room Button: Button to create room

### 3. Room List Item Prefab Setup

1. Create a new UI GameObject in Unity
2. Add the **RoomListItem** component
3. Add child elements:
   - TextMeshProUGUI named "RoomNameText"
   - TextMeshProUGUI named "PlayerCountText"
   - Button named "JoinButton"
4. Style the prefab as desired
5. Save as a prefab

### 4. Layout Recommendations

#### Room List Item Layout
```
Recommended size: 400 x 60 pixels
Layout:
[Room Name............] [2/4] [Join]
```

**Example settings:**
- Background: Light gray panel with border
- Room Name: Left-aligned, bold font
- Player Count: Center, smaller font
- Join Button: Right-aligned, 80px wide

#### Scroll View Settings
- Vertical scrolling only
- Content: Vertical Layout Group
  - Child Force Expand: Width = true, Height = false
  - Spacing: 10
  - Padding: 10 on all sides

## How It Works

### Room Creation Flow

1. Player enters room name and max players
2. Clicks "Create Room" button
3. RoomListingManager calls `CreateRoom()`
4. GameConfiguration is set to Host mode
5. Scene loads to GameScene
6. NetworkGameManager starts game as Host
7. Room becomes visible to other players

### Room Browsing Flow

1. RoomListingManager creates a lobby NetworkRunner
2. Calls `JoinSessionLobby()` to connect to Photon
3. Photon automatically calls `OnSessionListUpdated()` with available rooms
4. RoomListingManager filters open/visible rooms
5. Creates RoomListItem instances for each room
6. Updates UI with room name, player count, and join button

### Joining a Room Flow

1. Player sees room in list
2. Clicks "Join" button
3. RoomListingManager calls `JoinRoom()`
4. GameConfiguration is set to Client mode
5. Scene loads to GameScene
6. NetworkGameManager starts game as Client
7. Client connects to the host's session

## Testing

### Single Player Test
1. Run the game in Unity Editor
2. Go to Multiplayer Setup
3. Create a room called "TestRoom"
4. You should load into GameScene as host

### Two Player Test (Requires Build)
1. Build the game (File > Build and Run)
2. Run the build
3. In build: Create room "Room1"
4. In editor: Wait a few seconds for room list to update
5. In editor: You should see "Room1" appear in the list
6. In editor: Click "Join" on Room1
7. Both clients should connect and see each other's moves

## Troubleshooting

### Rooms not appearing
- Check Photon App ID is configured in `PhotonAppSettings.asset`
- Verify both clients are in the same region
- Check console for connection errors
- Make sure `OnSessionListUpdated()` is being called

### Can't join rooms
- Verify room name matches exactly
- Check if room is full (player count)
- Ensure GameConfiguration is being set correctly
- Look for errors in NetworkGameManager startup

### UI not updating
- Verify all prefab child names are exact ("RoomNameText", etc.)
- Check that RoomListingManager has correct references
- Ensure Content transform has Vertical Layout Group

## Advanced Features

### Custom Room Properties
You can extend the system to show additional room info:

```csharp
// In RoomListingManager.cs, modify CreateRoomListItem():
var mapText = item.transform.Find("MapText")?.GetComponent<TextMeshProUGUI>();
if (mapText != null)
{
    // Get custom properties from session.Properties
    mapText.text = "Classic Board";
}
```

### Passwords/Private Rooms
Photon Fusion supports session properties. You can add:

```csharp
// When creating a room:
var sessionProps = new Dictionary<string, SessionProperty>();
sessionProps["password"] = "1234";

// Then filter in OnSessionListUpdated:
var hasPassword = session.Properties.ContainsKey("password");
```

### Player Names
Display who's in the room by storing player names in session properties.

## Notes

- Maximum 8 players supported (configurable in code)
- Room names must be unique
- Photon Fusion handles session discovery automatically
- Lobby runner is separate from game runner
- Session list updates every few seconds automatically

## Related Files

- `/Assets/Scripts/RoomListingManager.cs` - Main room listing logic
- `/Assets/Scripts/RoomListItem.cs` - Room list UI component
- `/Assets/Scripts/MultiplayerSetupPanel.cs` - Updated setup panel
- `/Assets/Scripts/NetworkGameManager.cs` - Network game manager
- `/Assets/Scripts/GameConfiguration.cs` - Singleton for game settings

## See Also

- [MULTIPLAYER_SETUP.md](MULTIPLAYER_SETUP.md) - Basic multiplayer setup
- [MULTIPLAYER_SETUP_GUIDE.md](MULTIPLAYER_SETUP_GUIDE.md) - Detailed guide
