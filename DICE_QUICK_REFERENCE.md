# Dice Setup Quick Reference

## Fixed Issues

✅ **Dice now throws properly with physics**
✅ **Spacebar support: Hold Space to shake, release to throw**
✅ **Button support: Hold button to shake, release to throw**
✅ **Automatic Rigidbody configuration**
✅ **Proper gravity and physics**

## How to Use

### Method 1: Spacebar (Keyboard)

1. **Hold SPACE** → Dice spawns and shakes
2. **Release SPACE** → Dice throws
3. Dice rolls with physics
4. Result is read automatically

### Method 2: Button (Mouse/Touch)

1. **Click and HOLD** button → Dice spawns and shakes
2. **Release** button → Dice throws
3. Dice rolls with physics
4. Result is read automatically

## Dice Prefab Requirements

Your dice prefab needs:

### Required:
- **3D Model** (cube or custom shape)
- **Collider** (BoxCollider, MeshCollider, etc.)

### Optional (will be added automatically):
- **Rigidbody** - Auto-added if missing
- **DiceFace component** - For accurate value reading

The script will automatically:
- Add Rigidbody if missing
- Configure gravity, drag, angular drag
- Add BoxCollider if no collider exists
- Warn you if DiceFace is missing

## DiceRoller Settings

### Dice Setup
- **Dice Prefab**: Your dice model
- **Dice Throw Position**: Where dice spawns (empty GameObject)
- **Number Of Dice**: 1 or 2

### Shake Settings
- **Shake Intensity**: 0.5 (how much shake)
- **Shake Speed**: 20 (how fast shake)

### Throw Settings
- **Throw Force**: 8 (how hard to throw)
- **Throw Torque**: 300 (spin force)
- **Settle Time**: 2.5 (seconds to wait)
- **Throw Direction**: (0, -0.3, 1) - adjust this!

### Physics
- **Use Physics**: ✓ (enabled for real physics)
- **Drag**: 0.5 (air resistance)
- **Angular Drag**: 0.5 (spin resistance)

### Input
- **Roll Key**: Space (change if you want)
- **Enable Keyboard Input**: ✓

## Positioning the Dice Throw

**IMPORTANT: Set the throw position manually!**

1. Create empty GameObject
2. Name it "DiceThrowPoint"
3. Position it where dice should appear
4. Adjust height so dice can fall (e.g., Y = 2)
5. Assign to DiceRoller → Dice Throw Position

**Adjust Throw Direction:**
- X: Left (-) / Right (+)
- Y: Down (-) / Up (+)
- Z: Backward (-) / Forward (+)

Example: `(0, -0.3, 1)` = Slightly down and forward

## Troubleshooting

### Dice spins in air forever
**Fixed!** Now uses proper gravity and physics.

### Dice doesn't fall
- Check throw position Y is above ground
- Ensure there's a ground/floor with collider
- Increase throw force

### Dice flies too far
- Reduce Throw Force (try 5)
- Reduce Throw Torque (try 150)
- Adjust Throw Direction

### Dice doesn't shake
- Make sure you're holding Space or button
- Check Shake Intensity > 0
- Watch console for errors

### Wrong dice values
- Add DiceFace component to dice prefab
- Configure faces (see DICEFACE_SETUP.md)
- Or accept random values

### Space key doesn't work
- Check Enable Keyboard Input is enabled
- Change Roll Key if Space conflicts
- Make sure EventSystem exists in scene

### Button doesn't work
- Button needs EventSystem in scene
- DiceRoller must be on the button GameObject
- OR button calls DiceRoller on another GameObject

## Scene Setup Checklist

Dice Configuration:
- [ ] Dice prefab created
- [ ] Dice has 3D model
- [ ] Dice has Collider
- [ ] DiceFace component added (optional)

DiceRoller Setup:
- [ ] DiceRoller GameObject created
- [ ] ManualDiceRoller component added
- [ ] Dice prefab assigned
- [ ] Dice Throw Position set
- [ ] Settings adjusted

Ground/Floor:
- [ ] Ground plane exists
- [ ] Ground has Collider
- [ ] Ground is below throw position

Input:
- [ ] EventSystem in scene
- [ ] Roll button assigned (if using button)
- [ ] Button has Image component
- [ ] Spacebar tested

Testing:
- [ ] Hold Space → dice shakes
- [ ] Release Space → dice throws
- [ ] Dice falls and rolls
- [ ] Value is read correctly

## Advanced Configuration

### Two Dice

```
DiceRoller:
- Number Of Dice: 2
```

Both dice spawn side-by-side and throw together.

### Custom Throw Direction

Want dice to throw backwards?
```
Throw Direction: (0, -0.5, -1)
```

Want dice to throw left?
```
Throw Direction: (-1, -0.3, 0.5)
```

### Faster/Slower Roll

Faster:
```
Throw Force: 12
Settle Time: 1.5
```

Slower:
```
Throw Force: 5
Settle Time: 3.0
```

### More Realistic Physics

```
Drag: 0.2
Angular Drag: 0.3
Throw Torque: 500
```

## Debug Features

### Gizmos in Scene View

Yellow sphere = Spawn position
Red arrow = Throw direction

Use these to position your throw point!

### Console Logs

- "Dice thrown with force: ..." - Shows throw vector
- "Dice face value: X" - Shows each die value
- "Dice settled. Total value: X" - Final result
- Warnings if Rigidbody or DiceFace missing

## Integration with ManualGameManager

The dice automatically integrates:

```csharp
// ManualGameManager calls:
diceRoller.RollDice(OnDiceRolled);

// When done, OnDiceRolled(int result) is called
void OnDiceRolled(int result)
{
    // result = sum of all dice (1-6 for 1 die, 2-12 for 2 dice)
}
```

## Quick Setup Guide

**5-Minute Setup:**

1. **Create dice prefab**
   - Cube with material
   - Save as prefab

2. **Create DiceRoller**
   - Empty GameObject
   - Add ManualDiceRoller component
   - Assign dice prefab

3. **Position throw point**
   - Position DiceRoller where dice should spawn
   - Above your board
   - Example: (8, 2, 5)

4. **Add ground**
   - Plane at Y = 0
   - BoxCollider enabled

5. **Test**
   - Press Play
   - Hold Space
   - See dice shake
   - Release Space
   - Watch it throw and roll!

6. **Adjust**
   - Tweak Throw Force
   - Adjust Throw Direction
   - Change Settle Time

Done! Working dice system.

## Performance Tips

- Use simple dice models (low poly)
- Disable gizmos in builds
- Clear old dice (already automatic)
- Don't spawn too many dice at once

## Common Settings

### Standard Setup
```
Throw Force: 8
Throw Torque: 300
Settle Time: 2.5
Throw Direction: (0, -0.3, 1)
```

### Gentle Roll
```
Throw Force: 5
Throw Torque: 150
Settle Time: 3.0
Throw Direction: (0, -0.5, 0.8)
```

### Aggressive Throw
```
Throw Force: 12
Throw Torque: 500
Settle Time: 2.0
Throw Direction: (0.2, -0.2, 1.5)
```

That's it! Your dice should now throw properly with full physics.
