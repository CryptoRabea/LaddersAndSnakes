# Room Listing Setup Guide

This guide explains how to set up the room listing UI in your Unity project using the provided tools.

## Quick Setup (Recommended)

### Step 1: Use the Editor Tool

1. In Unity, go to **Tools → Room Listing Setup Tool**
2. Assign your **Main Menu Controller** reference
3. Assign your **Multiplayer Panel** reference
4. Click **"Create Room List Panel"** - This creates the entire room browsing UI
5. Click **"Create Room Panel"** - This creates the room creation UI
6. Click **"Generate Room List Item Prefab"** - Save it in your Prefabs folder
7. Click **"Auto-Assign All References"** - Automatically connects everything

That's it! The tool handles all the tedious setup work for you.

### Step 2: Verify Setup

The validation section will show checkmarks (✓) for all assigned references:
- ✓ Room List Panel
- ✓ Room List Container
- ✓ Room List Item Prefab
- ✓ Room List Status Text
- ✓ Room List Scroll Rect
- ✓ Create Room Panel
- ✓ Room Name Input
- ✓ Player Count Dropdown
- ✓ Refresh Room List Button
- ✓ Show Create Room Button
- ✓ Show Room List Button

## Runtime Helper (Alternative/Backup)

If you prefer to set things up manually or need runtime validation:

### Option 1: Attach the Helper Component

1. Select your **MainMenuController** GameObject
2. Add the **RoomListingUIHelper** component
3. In the Inspector, right-click on the component
4. Select **"Auto-Find All References"** to automatically find and assign everything

### Option 2: Context Menu Options

Right-click on the **RoomListingUIHelper** component in the Inspector to access:
- **Auto-Find All References** - Automatically finds and assigns all UI references
- **Validate All References** - Checks which references are set and which are missing
- **Print Required UI Structure** - Shows the complete UI hierarchy in the console

## Manual Setup (Advanced)

If you want to create the UI manually, here's the complete structure:

### Required UI Hierarchy

```
MultiplayerPanel/
├── RoomListPanel (GameObject + Image)
│   ├── Header (TextMeshProUGUI)
│   ├── StatusText (TextMeshProUGUI)
│   ├── ScrollView (ScrollRect + Image)
│   │   └── Viewport (RectTransform + Mask + Image)
│   │       └── Content (RectTransform + VerticalLayoutGroup + ContentSizeFitter)
│   └── ButtonsPanel (HorizontalLayoutGroup)
│       ├── RefreshButton (Button)
│       └── ShowCreateRoomButton (Button)
│
└── CreateRoomPanel (GameObject + Image)
    ├── Header (TextMeshProUGUI)
    ├── ContentArea (VerticalLayoutGroup)
    │   ├── RoomNameInput (TMP_InputField)
    │   ├── PlayerCountDropdown (TMP_Dropdown)
    │   └── ButtonsPanel (HorizontalLayoutGroup)
    │       ├── HostGameButton (Button)
    │       └── ShowRoomListButton (Button)
```

### Room List Item Prefab

```
RoomListItem (Prefab)
├── Image (Background)
├── HorizontalLayoutGroup
├── RoomNameText (TextMeshProUGUI)
├── PlayerCountText (TextMeshProUGUI)
└── SelectButton (Button)
    └── Text (TextMeshProUGUI)
```

### Component Settings

**ScrollView (ScrollRect):**
- Horizontal: Off
- Vertical: On
- Movement Type: Elastic
- Scroll Sensitivity: 30 (for mobile)
- Inertia: On
- Deceleration Rate: 0.135

**Content (VerticalLayoutGroup):**
- Spacing: 10
- Padding: 10 (all sides)
- Child Control Width: On
- Child Control Height: Off
- Child Force Expand Width: On

**Content (ContentSizeFitter):**
- Vertical Fit: Preferred Size

**Room List Item (HorizontalLayoutGroup):**
- Spacing: 20
- Padding: 20, 10 (horizontal, vertical)
- Child Control Width: Off
- Child Control Height: On

## References to Assign in MainMenuController

After creating the UI, assign these references in the MainMenuController Inspector:

### Room Listing Section
- **Room List Panel**: The main room browsing panel GameObject
- **Room List Container**: The "Content" Transform inside the ScrollView
- **Room List Item Prefab**: The prefab created with the generator tool
- **Room List Status Text**: The TextMeshProUGUI showing connection status
- **Room List Scroll Rect**: The ScrollRect component on ScrollView

### Room Creation Section
- **Create Room Panel**: The room creation panel GameObject
- **Room Name Input**: The TMP_InputField for entering room names
- **Player Count Dropdown**: The TMP_Dropdown for selecting max players

### Buttons
- **Refresh Room List Button**: Button to manually refresh the room list
- **Show Create Room Button**: Button to switch to create room view
- **Show Room List Button**: Button to return to room list view
- **Host Game Button**: Button to create and host a room
- **Join Game Button**: Button to join the selected room (already exists in multiplayer panel)

## Features

### Automatic Room Discovery
- Connects to Photon Fusion lobby on panel open
- Auto-refreshes every 3 seconds
- Shows real-time player counts

### Room Selection
- Click "Select" on any room to choose it
- Selected room highlights with blue background
- Join button only enabled when a room is selected

### Room Validation
- Checks if room is full before allowing join
- Validates room names (3-20 characters)
- Prevents duplicate room names
- Shows clear error messages

### Mobile Optimization
- Touch-friendly button sizes (100px minimum height)
- Large text (28-36px)
- Smooth scrolling with inertia
- Optimized for portrait and landscape

## Troubleshooting

### "No rooms available" message
- Make sure another player has created a room
- Check your internet connection
- Ensure Photon AppId is correctly configured

### Join button is disabled
- You must select a room first
- Click the "Select" button on any available room
- The join button will enable after selection

### Room list not updating
- Check the status text for connection errors
- Try clicking the Refresh button
- Make sure your Photon AppId is valid

### References not auto-assigning
- Make sure object names match exactly (case-sensitive)
- Objects must be active in the scene
- Run "Auto-Find All References" again after creating UI

## Testing

1. **Create a room:**
   - Click "Play Online" in main menu
   - Click "Create Room" button
   - Enter a room name
   - Select max players
   - Click "Host Game"

2. **Join a room:**
   - Click "Play Online" in main menu
   - Wait for rooms to appear
   - Click "Select" on a room
   - Click "Join Game"

3. **Verify selection requirement:**
   - Go to multiplayer panel
   - Observe that "Join Game" button is disabled
   - Select a room
   - Verify button becomes enabled

## Support

If you encounter issues:
1. Use the validation tools to check references
2. Check the console for detailed error messages
3. Ensure all components are properly configured
4. Verify Photon Fusion is correctly set up in your project
