# Main Menu UI Setup Instructions

## One-Time Setup (Automatic)

The MainMenu UI has been scripted for automatic creation. Follow these steps:

### Steps:

1. **Open Unity Editor**
   - Open the LaddersAndSnakes project in Unity

2. **Open the MainMenu Scene**
   - Navigate to `Assets/Scenes/MainMenu.unity`
   - Double-click to open it

3. **Run the Setup Tool**
   - In Unity's top menu, click: `Tools > Setup Main Menu UI (One Time)`
   - A confirmation dialog will appear
   - Click "Yes" to proceed

4. **Save the Scene**
   - After the setup completes, save the scene: `File > Save` or `Ctrl+S`

### What Gets Created:

The setup script will automatically create:

#### Canvas & Event System
- Canvas with proper screen scaling (1920x1080 reference)
- EventSystem for UI interaction

#### Main Panel (Default View)
- Title: "LADDERS & SNAKES"
- 5 Buttons:
  - Play Local
  - Play vs AI
  - Play Online
  - Settings
  - Quit

#### Multiplayer Panel
- Title: "MULTIPLAYER SETUP"
- Player Count Dropdown (2-4 players)
- Server Address Input Field
- Host Game Button
- Join Game Button
- Back Button

#### Settings Panel
- Title: "SETTINGS"
- Placeholder text for future settings
- Back Button

#### Controller Setup
- MainMenuController GameObject with all references automatically wired up
- All buttons, panels, input fields, and dropdowns connected

### Visual Design:

- **Main Panel**: Dark blue theme (rgba: 0.1, 0.1, 0.15, 0.95)
- **Multiplayer Panel**: Dark green theme (rgba: 0.1, 0.15, 0.1, 0.95)
- **Settings Panel**: Dark red theme (rgba: 0.15, 0.1, 0.1, 0.95)
- **Buttons**: Blue-gray with hover effects
- **Text**: TextMeshPro (white, centered, various sizes)

### After Setup:

1. The MainMenu will be fully functional
2. Test it by entering Play Mode in Unity
3. All navigation should work (panels switching)
4. The menu is ready to launch the game

### Customization (Optional):

After the initial setup, you can customize:
- Button positions and sizes
- Colors and fonts
- Add background images
- Add sound effects to buttons
- Expand the Settings panel with actual settings

### Troubleshooting:

- **Script not in menu**: Make sure `MainMenuSetup.cs` is in the `Assets/Editor/` folder
- **Missing TextMeshPro**: Import TextMeshPro if prompted by Unity
- **References not connected**: Re-run the setup tool (it will overwrite existing UI)

---

**Note**: This is a ONE-TIME setup. Running it multiple times will recreate the UI hierarchy.
