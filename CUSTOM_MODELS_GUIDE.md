# Custom Player Pieces and Dice Guide

This guide shows you how to add your own 3D models for player pieces and dice to the Snakes & Ladders game.

## Adding Custom Player Pieces

The game now supports custom player piece models! You can use your own 3D models (characters, animals, objects, etc.) instead of the default cylinders.

### Method 1: Same Model for All Players (Different Colors)

If you want to use the same model for all players but with different colors:

1. **Import your 3D model** into Unity (drag your FBX, OBJ, or other 3D file into the Assets folder)

2. **Create a prefab** from your model:
   - Drag your model into the scene
   - Adjust its rotation/scale if needed (make sure it's facing forward)
   - Drag it from the Hierarchy into the Assets/Prefabs folder
   - Delete it from the scene

3. **Assign it to the GameManager**:
   - Select the GameManager in the scene
   - In the Inspector, find "Player Settings"
   - Drag your prefab into the "Player Prefab" field

4. **Adjust height and spacing** (if needed):
   - `Player Height Offset`: How high above the board square the piece floats (default: 0.5)
   - `Player Spacing`: Distance between pieces on the same square (default: 0.3)

**Note**: The game will automatically color each player differently using the colors in the "Player Colors" array.

### Method 2: Different Model for Each Player

If you want completely different models for each player (e.g., different characters):

1. **Import all your player models** into Unity

2. **Create prefabs** for each model:
   - Drag each model into the scene
   - Adjust rotation/scale
   - Create prefabs for each
   - Delete from scene

3. **Assign them to the GameManager**:
   - Select the GameManager in the scene
   - In "Player Settings", expand "Custom Player Prefabs"
   - Set Size to 4
   - Drag your prefabs into slots:
     - Element 0: Player 1's model
     - Element 1: Player 2's model
     - Element 2: Player 3's model
     - Element 3: Player 4's model

4. **Leave unused slots empty** if you only want custom models for some players

**Important**: When using custom prefabs, the automatic coloring is disabled. Your models will keep their original materials/textures.

### Tips for Player Models

- **Scale**: Make sure your models fit on the board squares (test in Play mode)
- **Pivot Point**: The model's pivot should be at its base (bottom center)
- **Orientation**: Models should face forward (positive Z direction in Unity)
- **Colliders**: Not required, but won't hurt
- **Materials**: Your models keep their own materials when using Custom Player Prefabs

## Adding a Custom Dice

The game supports visual 3D dice with animation!

### Step 1: Create Your Dice Model

1. **Import your dice 3D model** into Unity
   - Should be a cube-like shape
   - Each face should show numbers 1-6 (as textures, materials, or geometry)

2. **Optional: Add physics**
   - Add a Rigidbody component if you want physics-based rolling
   - Add a Collider (Box Collider for a cube dice)

### Step 2: Create a Dice Prefab

1. Drag your dice model into the scene
2. Scale it to an appropriate size (around 0.5-1.0 units)
3. Create a prefab from it (drag to Assets/Prefabs)
4. Delete from scene

### Step 3: Set Up the Dice Roller

1. **Add a DiceRoller GameObject** to your scene:
   - In Hierarchy, right-click → Create Empty
   - Name it "DiceRoller"

2. **Add the SimpleDiceRoller component**:
   - Select the DiceRoller object
   - Add Component → Simple Dice Roller

3. **Configure the DiceRoller**:
   - **Dice Model**: Drag your dice prefab here
   - **Dice Spawn Point**: (Optional) Create an empty GameObject where dice should appear
   - **Roll Duration**: How long the animation lasts (default: 1 second)
   - **Spin Speed**: How fast the dice spins (default: 720 degrees/sec)
   - **Use Physics**: Check this if your dice has a Rigidbody
   - **Display Position**: Where the dice rests after rolling (e.g., 0, 1, 0)
   - **Display Rotation**: Base rotation for the dice (e.g., 0, 45, 0)

### Step 4: Connect to GameManager

1. **Select the GameManager** in your scene

2. **In "Dice Settings"**:
   - **Dice Roller**: Drag your DiceRoller GameObject here
   - **Use Visual Dice**: Check this box to enable visual dice

3. **Test it**: Press Play and click Roll Dice!

### Customizing Dice Face Rotations

The default dice rotation assumes a standard cube orientation. If your dice doesn't show the right number after rolling:

1. Open `SimpleDiceRoller.cs`
2. Find the `SetDiceToShowNumber` method
3. Adjust the rotation values for each number:

```csharp
Quaternion[] faceRotations = new Quaternion[]
{
    Quaternion.Euler(0, 0, 0),      // Face showing 1
    Quaternion.Euler(0, 180, 0),    // Face showing 2
    Quaternion.Euler(0, 90, 0),     // Face showing 3
    Quaternion.Euler(0, -90, 0),    // Face showing 4
    Quaternion.Euler(90, 0, 0),     // Face showing 5
    Quaternion.Euler(-90, 0, 0)     // Face showing 6
};
```

Test different rotations until each number shows correctly.

## Quick Setup Checklist

### For Custom Player Pieces:
- [ ] Import 3D models to Unity
- [ ] Create prefabs
- [ ] Assign to GameManager (either Player Prefab or Custom Player Prefabs)
- [ ] Adjust Player Height Offset and Player Spacing if needed
- [ ] Test in Play mode

### For Custom Dice:
- [ ] Import dice 3D model
- [ ] Create dice prefab
- [ ] Create DiceRoller GameObject in scene
- [ ] Add SimpleDiceRoller component
- [ ] Configure dice settings
- [ ] Assign to GameManager
- [ ] Enable "Use Visual Dice"
- [ ] Test dice roll in Play mode
- [ ] Adjust face rotations if needed

## Example Configurations

### Example 1: Cartoon Characters as Players

```
GameManager Settings:
- Custom Player Prefabs (Size: 4)
  - Element 0: Character_Robot
  - Element 1: Character_Cat
  - Element 2: Character_Dog
  - Element 3: Character_Alien
- Player Height Offset: 0.8 (characters are taller)
- Player Spacing: 0.4 (give them more room)
```

### Example 2: Simple Colored Pawns

```
GameManager Settings:
- Player Prefab: Pawn_Model
- Player Colors: Red, Blue, Green, Yellow
- Player Height Offset: 0.5
- Player Spacing: 0.3
```

### Example 3: Realistic Dice with Physics

```
DiceRoller Settings:
- Dice Model: RealisticDice_Prefab (with Rigidbody)
- Use Physics: ✓ Enabled
- Roll Duration: 2.0 (longer for physics)
- Spin Speed: 500
- Display Position: (1, 0.5, 8)
- Display Rotation: (0, 0, 0)

GameManager Settings:
- Dice Roller: DiceRoller GameObject
- Use Visual Dice: ✓ Enabled
```

## Troubleshooting

### Player pieces are floating or sinking into the board
- Adjust **Player Height Offset** in GameManager
- Check your model's pivot point (should be at the base)

### Player pieces overlap each other
- Increase **Player Spacing** in GameManager

### Dice shows wrong numbers
- Adjust the face rotations in `SimpleDiceRoller.cs` → `SetDiceToShowNumber()`
- Each dice model is oriented differently, so this needs customization

### Dice is too fast/slow
- Adjust **Roll Duration** (longer = slower)
- Adjust **Spin Speed** (lower = slower spin)

### Dice flies off screen
- Reduce **Spin Speed** if using physics
- Check that your dice prefab has a Rigidbody with appropriate mass (around 1)

### Colors not applying to custom models
- This is expected! Custom Player Prefabs keep their original materials
- If you want different colors, create separate prefabs for each player

### Dice doesn't appear
- Make sure **Use Visual Dice** is checked
- Make sure **Dice Roller** field is assigned
- Check that your dice prefab is assigned in the DiceRoller component

## Asset Recommendations

### Where to Get 3D Models

Free options:
- **Sketchfab** (many free CC models)
- **TurboSquid** (has free section)
- **Unity Asset Store** (free 3D models)
- **Mixamo** (free characters with animations)

Tips:
- Look for low-poly models for better performance
- Check the license before using
- Models with textures look better than plain colors

### Model Format Support

Unity supports:
- .FBX (recommended)
- .OBJ
- .3DS
- .DAE (Collada)
- .DXF
- .MAX (requires 3ds Max)
- .BLEND (requires Blender)

## Advanced: Animated Player Pieces

If your player models have animations:

1. Make sure your prefab has an Animator component
2. Create an Animator Controller with states:
   - "Idle" - standing still
   - "Move" - walking/moving animation

3. The game doesn't currently trigger animations, but you can extend it:
   - Edit `SimpleGameManager.cs`
   - In `MoveToPosition()`, add:
   ```csharp
   Animator anim = obj.GetComponent<Animator>();
   if (anim != null) anim.SetTrigger("Move");
   ```
   - When movement ends, trigger "Idle"

## Need Help?

Check the following files:
- `SimpleGameManager.cs` - Main game logic
- `SimpleDiceRoller.cs` - Dice rolling logic
- `GAME_SETUP.md` - General game setup guide

You can modify these scripts to customize the game further!
