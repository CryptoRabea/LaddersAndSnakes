# Room Listing - Quick Start Guide

## What's New

Your Ladders and Snakes game now supports:
- ✅ **Room Creation** - Players can create rooms with custom names
- ✅ **Room Discovery** - See all available rooms in a list
- ✅ **Easy Joining** - Click to join any available room
- ✅ **Manual Entry** - Still supports entering room names manually

## Files Added/Modified

### New Scripts
- `Assets/Scripts/RoomListingManager.cs` - Handles room discovery and listing
- `Assets/Scripts/RoomListItem.cs` - UI component for room entries

### Modified Scripts
- `Assets/Scripts/MultiplayerSetupPanel.cs` - Now supports room browsing
- `Assets/Scripts/NetworkGameManager.cs` - Sessions are now visible in lobby

### Documentation
- `ROOM_LISTING_SETUP.md` - Complete setup instructions
- `ROOM_LISTING_QUICKSTART.md` - This file

## Quick Setup (5 Steps)

### 1. Create Room List Item Prefab

In Unity Editor:
1. Create a new UI GameObject (Right-click in Hierarchy > UI > Panel)
2. Rename it to "RoomListItemPrefab"
3. Add the `RoomListItem` component to it
4. Add three child GameObjects:
   - TextMeshProUGUI named "RoomNameText"
   - TextMeshProUGUI named "PlayerCountText"
   - Button named "JoinButton" (with TextMeshProUGUI child for button text)
5. Drag to Prefabs folder to save as prefab
6. Delete from scene

### 2. Update MultiplayerSetupPanel UI

In your MainMenu scene:
1. Find the MultiplayerSetupPanel GameObject
2. Add two child panels:
   - "RoomListingPanel" (for room browsing)
   - "ManualJoinPanel" (for manual entry)
3. Move existing Host/Join UI elements into ManualJoinPanel

### 3. Create Room Listing UI

Inside RoomListingPanel, create:

**Create Room Section:**
- TMP_InputField for room name (reference as "createRoomNameInput")
- TMP_InputField for max players (reference as "maxPlayersInput")
- Button "Create Room" (reference as "createRoomButton")

**Room List Section:**
- ScrollView with Content transform (reference Content as "roomListContainer")
- Button "Refresh" (reference as "refreshButton")
- TextMeshProUGUI for status (reference as "statusText")

### 4. Add RoomListingManager

1. Create empty GameObject inside RoomListingPanel
2. Name it "RoomListingManager"
3. Add `RoomListingManager` component
4. Assign references in Inspector:
   - Room List Container → Content transform
   - Room List Item Prefab → Your prefab from step 1
   - Status Text → Status TextMeshProUGUI
   - Refresh Button → Refresh button
   - Create Room Name Input → Room name input field
   - Max Players Input → Max players input field
   - Create Room Button → Create room button

### 5. Configure MultiplayerSetupPanel

In the MultiplayerSetupPanel component, assign:
- Room Listing Panel → RoomListingPanel GameObject
- Manual Join Panel → ManualJoinPanel GameObject
- Room Listing Manager → RoomListingManager component
- Add buttons to switch between panels (optional)

## That's It!

Now when players open the multiplayer menu:
1. They'll see the room listing by default
2. They can create a room with a custom name
3. Other players will see the room in their list
4. They can click "Join" to connect

## Testing

**Quick Test:**
1. Run in Unity Editor
2. Click "Play Online"
3. Create a room called "TestRoom"
4. Build the game
5. Run the build and check if "TestRoom" appears in the list

## Common Issues

**Rooms not appearing?**
- Make sure Photon App ID is configured
- Check console for connection errors
- Wait 3-5 seconds for refresh

**Can't join rooms?**
- Verify both players are in the same region
- Check if room is full
- Look for NetworkGameManager errors

**UI not showing?**
- Verify child object names are exact
- Check all Inspector references are assigned
- Make sure prefab has RoomListItem component

## Need More Details?

See `ROOM_LISTING_SETUP.md` for complete documentation including:
- Detailed UI hierarchy
- Layout recommendations
- Advanced features (passwords, player names, etc.)
- Troubleshooting guide

## How It Works

```
Player 1 (Host)                    Photon Server                    Player 2 (Client)
     |                                    |                               |
     | Create "MyRoom"                    |                               |
     |-------------------------------->   |                               |
     |                                    |                               |
     |                                    | <-- Session registered         |
     |                                    |                               |
     |                                    |   Request session list         |
     |                                    | <-----------------------------|
     |                                    |                               |
     |                                    |   Send session list           |
     |                                    |-----------------------------> |
     |                                    |   (includes "MyRoom")         |
     |                                    |                               |
     |                                    |   Join "MyRoom"               |
     |                                    | <-----------------------------|
     |                                    |                               |
     | <-- Player 2 connected ----------- | -- Player joined ------------> |
     |                                    |                               |
     | <-------------- Game synchronized via NetworkGameState ----------> |
```

## Next Steps

- Customize the room list item appearance
- Add room passwords (see Advanced Features)
- Display player names in rooms
- Add map selection
- Implement quick match button

Happy gaming! 🎲
