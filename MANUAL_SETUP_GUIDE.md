# Manual Setup Guide - Full Developer Control

**NO AUTOMATIONS. YOU SET UP EVERYTHING MANUALLY.**

This guide is for developers who want complete control over their game setup. No automatic scene generation, no editor tools - just pure manual configuration.

## Core Scripts

### ManualGameManager.cs
The main game controller with manual setup fields.

### ManualDiceRoller.cs
Hold-to-shake, release-to-throw dice system with 1 or 2 dice support.

## Manual Scene Setup Steps

### Step 1: Create Your Game Scene

1. Create a new scene: File → New Scene
2. Save it as "GameScene" in Assets/Scenes/

### Step 2: Set Up Camera

**IMPORTANT: You position the camera yourself!**

1. Select Main Camera
2. Position it where you want to view your board
3. Adjust rotation, FOV, background color
4. Example position: (5, 15, -5) looking down at board

### Step 3: Add Your Board

**Option A: Use Your Board Prefab**

1. Drag your board prefab into the scene
2. Position it at (0, 0, 0) or wherever you want
3. Your board should have 100 child transforms for squares (numbered 1-100)

**Option B: Manual Square Placement**

1. Create empty GameObjects for each square
2. Name them "Square_1", "Square_2", etc.
3. Position them in your board layout
4. Parent them under a "Board" object

**Option C: No Visual Board**

Skip this - the game will use logical positions

### Step 4: Create Game Manager

1. Create empty GameObject: Right-click → Create Empty
2. Name it "GameManager"
3. Add Component → Manual Game Manager

### Step 5: Configure Game Manager

Select GameManager and configure in Inspector:

#### Manual Board Setup
- **Board Squares**: Assign your 100 square transforms in order (1-100)
  - Expand the array, set size to 100
  - Drag each square transform into the array
  - OR leave empty to use logical positions
- **Board Size**: 100 (default)

#### Player Setup
- **Player Prefabs**: Drag your 4 player piece prefabs
  - Element 0: Player 1's model
  - Element 1: Player 2's model
  - Element 2: Player 3's model
  - Element 3: Player 4's model
- **Number Of Players**: 2, 3, or 4
- **Is AI**: Check boxes for AI players
  - Check Element 0: Player 1 is AI
  - Check Element 1: Player 2 is AI
  - etc.
- **Player Height Offset**: How high pieces float (default: 0.5)
- **Player Spacing**: Distance between pieces (default: 0.3)
- **Move Speed**: Movement speed (default: 5)

#### Dice Setup
- **Dice Roller**: Assign your DiceRoller GameObject (see Step 6)
- **Number Of Dice**: 1 or 2 dice

#### Multiplayer
- **Is Multiplayer**: Check for multiplayer mode
- **Local Player Index**: Which player is local (0-3)

#### UI References
- **Roll Dice Button**: Button to roll dice
- **Turn Text**: Shows current turn
- **Dice Text**: Shows dice result
- **Message Text**: Shows game messages
- **Win Panel**: Panel shown on game over
- **Winner Text**: Shows winner
- **Play Again Button**: Restart button
- **Main Menu Button**: Main menu button

#### Snakes and Ladders
- **Jumps**: Already configured with default snakes/ladders
- Edit the array to add/remove/modify jumps
- Each jump has:
  - From: Square to jump from
  - To: Square to jump to
  - Is Ladder: True for ladder, false for snake

### Step 6: Set Up Dice Roller

1. Create empty GameObject: Right-click → Create Empty
2. Name it "DiceRoller"
3. Position it where you want dice to appear (e.g., to the side of board)
4. Add Component → Manual Dice Roller

#### Configure Dice Roller:

**Dice Setup**
- **Dice Prefab**: Drag your dice model prefab
- **Dice Throw Position**: This GameObject's transform (or create another position marker)
- **Number Of Dice**: 1 or 2 (set by GameManager)

**Shake Settings**
- **Shake Intensity**: 0.5 (how much it shakes while holding)
- **Shake Speed**: 20 (how fast it shakes)

**Throw Settings**
- **Throw Force**: 5 (how hard dice is thrown)
- **Throw Torque**: 100 (spin force)
- **Settle Time**: 2 (how long to wait for physics)

**Physics**
- **Use Physics**: Check if your dice has Rigidbody
- **Ground Layer**: Layer for ground (if using physics)

### Step 7: Create UI Canvas

1. Right-click Hierarchy → UI → Canvas
2. Set Canvas to Screen Space - Overlay
3. Set Reference Resolution: 1920 x 1080

#### Add UI Elements:

**Turn Text**
- UI → Text - TextMeshPro
- Position top center
- Text: "Player 1's Turn"
- Font size: 48

**Dice Text**
- UI → Text - TextMeshPro
- Position below turn text
- Text: "Rolled: -"
- Font size: 36

**Message Text**
- UI → Text - TextMeshPro
- Position bottom center
- Text: "Press and hold to shake, release to throw!"
- Font size: 32

**Roll Dice Button**
- UI → Button - TextMeshPro
- Position bottom center (above message)
- Text: "Roll Dice"
- Size: 300 x 80
- **IMPORTANT**: Add Event System if not present

**Win Panel** (Hidden by default)
- UI → Panel
- Cover full screen
- Add child texts:
  - Winner Text: "Player 1 Wins!"
  - Play Again Button
  - Main Menu Button

### Step 8: Wire Up UI to GameManager

1. Select GameManager
2. Drag each UI element to corresponding field:
   - Roll Dice Button → Roll Dice Button field
   - Turn Text → Turn Text field
   - Dice Text → Dice Text field
   - Message Text → Message Text field
   - Win Panel → Win Panel field
   - Winner Text → Winner Text field
   - Play Again Button → Play Again Button field
   - Main Menu Button → Main Menu Button field

3. Drag DiceRoller GameObject → Dice Roller field

### Step 9: Prepare Dice Prefab

Your dice prefab should have:

1. **3D Model** with dice texture/materials
2. **Rigidbody** (if using physics)
   - Mass: 1
   - Drag: 0.5
   - Angular Drag: 0.5
3. **Collider** (Box Collider for cube)
4. **Optional: DiceFace component** (on ManualDiceRoller script)
   - Automatically detects which face is up
   - Configure face directions and values

### Step 10: Dice Throw Position

**IMPORTANT: Position where dice spawns**

1. Create empty GameObject
2. Name it "DiceThrowPoint"
3. Position it where you want dice to appear
   - Example: Above and to the side of the board
   - Position: (8, 2, 5)
4. Assign to DiceRoller → Dice Throw Position

### Step 11: Test and Adjust

1. Press Play
2. Adjust camera position/rotation
3. Adjust dice throw position
4. Adjust player height and spacing
5. Test rolling dice (hold button, release to throw)
6. Adjust throw force and torque if needed

## Dice Mechanics

### Hold-to-Shake, Release-to-Throw

The dice button works like this:

1. **Press and Hold**: Dice spawns and shakes visually
2. **Release**: Dice is thrown with physics or animation
3. **Wait**: Dice settles and result is read
4. **Move**: Player piece moves based on result

### Using 1 or 2 Dice

In GameManager:
- `Number Of Dice = 1`: Single dice (1-6)
- `Number Of Dice = 2`: Two dice (2-12)

The total is automatically calculated and used for movement.

## AI Players

To set up AI players:

1. In GameManager → Player Setup → Is AI array
2. Check the boxes for AI players:
   - Element 0 checked = Player 1 is AI
   - Element 1 checked = Player 2 is AI
   - etc.

AI will automatically:
- Wait 1-2 seconds before rolling
- Roll dice automatically
- Move their piece

## Multiplayer Setup

For local/online multiplayer:

1. In GameManager:
   - **Is Multiplayer**: Check this
   - **Local Player Index**: Set to local player (0-3)

2. Only the local player can roll dice
3. Other players are controlled remotely (you need to add networking)

## No Board Visual? No Problem

If you don't assign board squares:
- Game uses logical grid positions (10x10)
- Players move in snake pattern
- Still fully playable, just no custom board

## Customization Tips

### Custom Board Layout

Want a different board shape?
1. Create your 100 squares in any layout
2. Assign them in order to Board Squares array
3. Snakes/ladders work with any layout

### Custom Snakes and Ladders

Edit the Jumps array in GameManager:
- Add new jump: Increase size, set From/To/IsLadder
- Remove jump: Delete element
- Modify jump: Change From/To values

### Custom Dice Behavior

Edit `ManualDiceRoller.cs`:
- `ThrowWithPhysics()`: Physics-based rolling
- `ThrowWithAnimation()`: Scripted animation
- `ReadDiceValues()`: How dice value is determined

### Multiple Dice Models

To use different dice models:
1. Create multiple DiceRoller GameObjects
2. Each with different dice prefab
3. Switch which one is assigned to GameManager

## Troubleshooting

### Dice doesn't shake when holding button
- Make sure you added Manual Dice Roller component
- Check button has Event Trigger or pointer events work
- Dice Prefab must be assigned

### Dice flies off screen
- Reduce Throw Force (default: 5)
- Reduce Throw Torque (default: 100)
- Add ground collider to catch dice

### Dice shows wrong number
- Add DiceFace component to dice prefab
- Configure face directions in DiceFace
- Or edit ReadDiceValues() in ManualDiceRoller

### Players overlap
- Increase Player Spacing (default: 0.3)

### Players floating/sinking
- Adjust Player Height Offset (default: 0.5)

### Button not working
- Check Event System exists in scene
- Check button OnClick event is set
- Make sure GameManager's Roll Dice Button field is assigned

### AI doesn't move
- Check Is AI array has correct player marked
- Check that player index is within array bounds

## Advanced: Reading Dice Faces

The `DiceFace` component (optional) automatically detects which face is up:

1. Add to your dice prefab
2. Configure face directions:
   - Up = 1
   - Down = 6
   - Forward = 2
   - Back = 5
   - Right = 3
   - Left = 4

3. Adjust based on your dice model's orientation

## Example Configurations

### 2 Player Game vs AI

```
GameManager:
- Number Of Players: 2
- Is AI: [unchecked, checked] (Player 2 is AI)
- Number Of Dice: 1
```

### 4 Player Local Multiplayer

```
GameManager:
- Number Of Players: 4
- Is AI: all unchecked
- Number Of Dice: 1
```

### High Stakes (2 Dice)

```
GameManager:
- Number Of Players: 2
- Number Of Dice: 2
```

## File Structure

```
Assets/
├── Scenes/
│   ├── GameScene.unity (your manually created scene)
│   └── MainMenu.unity
├── Scripts/
│   ├── ManualGameManager.cs (main controller)
│   ├── ManualDiceRoller.cs (dice system)
│   └── SimpleMainMenu.cs (menu - still works)
├── Prefabs/
│   ├── YourBoardPrefab.prefab
│   ├── Player1.prefab
│   ├── Player2.prefab
│   ├── Player3.prefab
│   ├── Player4.prefab
│   └── Dice.prefab
```

## Quick Checklist

Scene Setup:
- [ ] Camera positioned manually
- [ ] Board placed in scene (or using logical positions)
- [ ] GameManager object created
- [ ] DiceRoller object created
- [ ] UI Canvas with all elements
- [ ] Event System present

GameManager Configuration:
- [ ] Board squares assigned (or left empty)
- [ ] All 4 player prefabs assigned
- [ ] Number of players set
- [ ] AI players marked
- [ ] Dice roller assigned
- [ ] Number of dice set (1 or 2)
- [ ] All UI references assigned

DiceRoller Configuration:
- [ ] Dice prefab assigned
- [ ] Throw position set
- [ ] Physics settings configured

Testing:
- [ ] Camera shows board properly
- [ ] Dice spawns at correct position
- [ ] Hold button shakes dice
- [ ] Release button throws dice
- [ ] Players move correctly
- [ ] Snakes/ladders work
- [ ] Win condition works
- [ ] AI works (if enabled)

## You're the Developer!

This system gives you FULL CONTROL:
- Position everything yourself
- Use your own prefabs
- Adjust every parameter
- No hidden automations
- No expensive editor operations
- Pure manual setup

Customize everything to match your vision!
