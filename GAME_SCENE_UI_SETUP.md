# Game Scene UI Setup

This document explains the new game scene UI features including the main menu button, settings button, and settings panel with surrender functionality.

## Features Added

### 1. Main Menu Button
- **Location**: Top-left corner of the game scene
- **Functionality**: Returns to the main menu from the game scene
- **Behavior**:
  - Cleans up network connections if in multiplayer mode
  - Loads the MainMenu scene

### 2. Settings Button
- **Location**: Top-right corner of the game scene
- **Functionality**: Opens the settings panel
- **Mobile Optimized**: Larger touch targets on mobile devices

### 3. Settings Panel
A centered panel that appears when the settings button is clicked.

#### Components:
- **Title**: "Settings" header
- **Surrender Button**: Main action button (red)
- **Close Button**: Returns to the game (gray)

#### Surrender Button Behavior:

##### Local/AI Games:
- Immediately quits the game and returns to main menu
- No network cleanup needed

##### Online Multiplayer Games:
- **If only 1 player remaining** (you):
  - Closes the entire room
  - Shuts down the network session completely
  - Returns to main menu
- **If multiple players remaining**:
  - Leaves the room gracefully
  - Other players can continue playing
  - Returns to main menu

## Installation

### Automatic Setup (Recommended)

1. Open your GameScene in Unity
2. Go to menu: **Ladders & Snakes → Setup Game Scene UI**
3. Click "Yes" in the confirmation dialog
4. Save your scene (Ctrl+S / Cmd+S)

This will automatically create:
- A `GameSceneUI` GameObject
- Main menu button (top-left)
- Settings button (top-right)
- Settings panel with surrender and close buttons

### Manual Setup

If you prefer to set up manually:

1. Create an empty GameObject in your scene named "GameSceneUI"
2. Add the `GameSceneUISetup` component to it
3. The component will automatically create all UI elements at runtime
4. Alternatively, check "Auto Create UI" in the inspector and use the "Setup UI" context menu

## Scripts

### GameSceneUISetup.cs
**Purpose**: Creates and manages UI elements programmatically

**Features**:
- Auto-creates UI elements at runtime if they don't exist
- Customizable button and panel colors
- Mobile optimizations (larger buttons on mobile)
- Assigns references to SettingsPanelController automatically

**Inspector Options**:
- `Auto Create UI`: Enable/disable automatic UI creation
- `Button Color`: Color for main menu and settings buttons
- `Panel Color`: Background color for settings panel

### SettingsPanelController.cs
**Purpose**: Handles all UI interactions and game logic

**Key Methods**:
- `OpenSettingsPanel()`: Shows the settings panel
- `CloseSettingsPanel()`: Hides the settings panel
- `GoToMainMenu()`: Returns to main menu with network cleanup
- `OnSurrenderClicked()`: Handles surrender logic based on game mode

**Network Features**:
- Detects if game is multiplayer via GameConfiguration
- Counts remaining players using NetworkRunner
- Gracefully shuts down network connections
- Closes room if last player, otherwise just leaves

## Mobile Optimizations

All UI elements include mobile-specific optimizations:
- **Button Scaling**: 1.2x scale on mobile for better touch targets
- **Text Sizing**: Larger fonts for readability
- **Touch-Friendly**: Minimum button sizes for accurate tapping

## Technical Details

### Network Cleanup

The surrender system properly handles Photon Fusion networking:

```csharp
// Close room (last player)
networkRunner.Shutdown(true, ShutdownReason.Ok);

// Leave room (multiple players)
networkRunner.Shutdown(false, ShutdownReason.Ok);
```

### Game Mode Detection

The system uses `GameConfiguration.Instance.IsMultiplayer` to determine the current game mode and apply the appropriate surrender logic.

### Player Count Check

```csharp
int playerCount = networkRunner.SessionInfo.PlayerCount;
if (playerCount <= 1) {
    // Close room
} else {
    // Leave room
}
```

## Testing

### Test Local/AI Game:
1. Start a local or AI game
2. Click Settings button
3. Click Surrender
4. Verify return to main menu

### Test Online Game (2+ Players):
1. Host or join an online game
2. Click Settings button
3. Click Surrender
4. Verify you leave the room
5. Other players should be able to continue

### Test Online Game (1 Player):
1. Host an online game (alone)
2. Click Settings button
3. Click Surrender
4. Verify room closes and returns to main menu

## Troubleshooting

### Buttons not appearing
- Check that GameSceneUISetup component is in the scene
- Verify "Auto Create UI" is enabled
- Check the Canvas exists in the scene

### Surrender not working in multiplayer
- Verify NetworkGameManager exists in the scene
- Check GameConfiguration is properly set
- Look for error logs related to NetworkRunner

### UI too small/large on mobile
- Adjust `mobileButtonScaleMultiplier` in GameSceneUISetup
- Default is 1.2x for mobile devices

## Future Enhancements

Potential additions to the settings panel:
- Sound volume controls
- Graphics quality settings
- Controls customization
- Player statistics
- Achievement tracking

## Related Files

- `/Assets/Scripts/GameSceneUISetup.cs`
- `/Assets/Scripts/SettingsPanelController.cs`
- `/Assets/Scripts/Editor/GameSceneUISetupEditor.cs`
- `/Assets/Scripts/ManualGameManager.cs` (existing main menu logic)
- `/Assets/Scripts/NetworkGameManager.cs` (network management)
- `/Assets/Scripts/GameConfiguration.cs` (game mode configuration)

## Support

For issues or questions:
1. Check Unity Console for error messages
2. Verify all required components are in the scene
3. Ensure Photon Fusion is properly configured for multiplayer
4. Check that GameConfiguration singleton exists

---

**Version**: 1.0
**Last Updated**: 2025-11-19
**Compatible With**: Unity 2021.3+ with Photon Fusion
