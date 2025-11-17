# DiceFace Component Setup Guide

The DiceFace component automatically detects which face of your dice is pointing up and returns the correct value (1-6).

## Quick Setup

### Method 1: Standard Dice (Recommended)

1. **Add component to your dice prefab**
   - Select your dice prefab
   - Add Component → Dice Face

2. **Click "Setup Standard Dice" button** in Inspector
   - This configures standard dice layout (opposite faces add to 7)
   - Top/Bottom: 1/6
   - Front/Back: 2/5
   - Right/Left: 3/4

3. **Rotate your dice and test**
   - Enter Play mode
   - Rotate the dice in scene
   - Watch "Top Face Value" update automatically

4. **Adjust if needed**
   - If values are wrong, manually adjust face directions
   - Or use Auto-Configuration (Method 2)

### Method 2: Auto-Configuration

Use this if your dice model has a non-standard orientation:

1. **Add DiceFace component** to your dice prefab

2. **Position dice with face "1" pointing up** in scene view

3. **Click "Detect Face 1"** button in Inspector

4. **Rotate dice to show face "2" up**

5. **Click "Detect Face 2"** button

6. **Repeat for all faces 1-6**

7. **Click "Validate Configuration"** to check setup is correct

## How It Works

The component:
- Defines 6 faces with local directions (up, down, forward, back, left, right)
- Each face has a value (1-6)
- `GetTopFaceValue()` finds which face is closest to world-up
- Returns that face's value

## Visual Debug

**Gizmos in Scene View:**
- Colored lines show each face direction
- Colors indicate values:
  - White = 1
  - Yellow = 2
  - Green = 3
  - Cyan = 4
  - Blue = 5
  - Red = 6
- White circle highlights current top face
- Yellow text shows current top value

**Toggle Gizmos:**
- Check/uncheck "Show Debug Gizmos" in Inspector
- Adjust "Gizmo Length" to change line size

## Face Configuration

Each face has:
- **Local Direction**: Which way this face points on the model
  - `Vector3.up` = top of cube
  - `Vector3.down` = bottom
  - `Vector3.forward` = front
  - `Vector3.back` = back
  - `Vector3.right` = right side
  - `Vector3.left` = left side

- **Value**: Number shown on this face (1-6)

### Standard Dice Layout

```
Top (up):      1
Bottom (down): 6
Front (fwd):   2
Back (back):   5
Right (right): 3
Left (left):   4
```

Opposite faces add to 7: (1+6, 2+5, 3+4)

## Using with ManualDiceRoller

The ManualDiceRoller automatically uses DiceFace if present:

```csharp
// In ReadDiceValues():
DiceFace diceFace = dice.GetComponent<DiceFace>();
if (diceFace != null)
{
    total += diceFace.GetTopFaceValue();
}
else
{
    // Falls back to random if no DiceFace
    total += Random.Range(1, 7);
}
```

## Advanced Usage

### Custom Face Values

Want different numbers on faces? Edit the array directly:

```csharp
// Inspector: Faces array
Face 0: Local Direction (0, 1, 0), Value: 1
Face 1: Local Direction (0, -1, 0), Value: 6
// etc.
```

### Check Specific Direction

Get value of face pointing in any direction:

```csharp
DiceFace diceFace = GetComponent<DiceFace>();
int frontValue = diceFace.GetFaceValueInDirection(Vector3.forward);
```

### Validation

Check if configuration is valid:

```csharp
if (diceFace.ValidateConfiguration())
{
    Debug.Log("Dice is correctly configured!");
}
```

Validation checks:
- Exactly 6 faces
- All values 1-6
- No duplicate values
- All values present

## Troubleshooting

### Wrong values showing

**Option A: Use Auto-Configuration**
1. Place dice with each number face-up
2. Click corresponding "Detect Face X" button
3. Validate when done

**Option B: Manual adjustment**
1. Look at your dice model in scene
2. Identify which local direction each number faces
3. Edit Face array in Inspector
4. Set Local Direction for each face

### Values don't change when rotating

1. Check gizmos are showing (Debug section)
2. Verify face directions are different (not all same)
3. Make sure component is on the dice GameObject
4. Ensure dice is actually rotating in world space

### Validation fails

Common issues:
- Duplicate values (two faces with same number)
- Missing values (not all 1-6 present)
- Values outside 1-6 range

Click "Validate Configuration" to see specific errors in Console

### Gizmos not showing

1. Make sure Gizmos are enabled in Scene view (top-right icon)
2. Check "Show Debug Gizmos" is enabled in Inspector
3. Select the dice object in Hierarchy

## Example Configurations

### Standard 6-sided Dice
```
Face 0: (0, 1, 0) = 1
Face 1: (0, -1, 0) = 6
Face 2: (0, 0, 1) = 2
Face 3: (0, 0, -1) = 5
Face 4: (1, 0, 0) = 3
Face 5: (-1, 0, 0) = 4
```

### Rotated Dice Model
If your model is rotated 90° on import:
```
Face 0: (1, 0, 0) = 1   // What was up is now right
Face 1: (-1, 0, 0) = 6  // etc.
// Adjust other faces accordingly
```

## Testing

### In Editor (Scene View)

1. Select dice in Hierarchy
2. Use Rotate tool to rotate dice
3. Watch gizmos update
4. Check "Current Reading" in Inspector

### In Play Mode

1. Press Play
2. Spawn dice (via DiceRoller)
3. Watch dice roll
4. Value is automatically read when settled

### Manual Testing

```csharp
// In another script:
DiceFace dice = GameObject.Find("Dice").GetComponent<DiceFace>();
Debug.Log($"Top face: {dice.GetTopFaceValue()}");
```

## Performance

- Very lightweight (just a few dot products)
- No physics required
- No Update() calls
- Only runs when GetTopFaceValue() is called
- Safe to call every frame if needed

## Integration Checklist

Setup:
- [ ] DiceFace component added to dice prefab
- [ ] Configuration method chosen (standard or auto)
- [ ] Faces configured correctly
- [ ] Validation passed

Testing:
- [ ] Gizmos showing correct colors/directions
- [ ] Top value updates when rotating in scene
- [ ] Works in Play mode
- [ ] ManualDiceRoller reads values correctly

Verification:
- [ ] All 6 faces configured
- [ ] Values 1-6 all present
- [ ] No duplicates
- [ ] Opposite faces add to 7 (for standard dice)

## Quick Reference

**Public Methods:**
- `GetTopFaceValue()` - Returns value of face pointing up
- `GetFaceValueInDirection(Vector3)` - Returns value of face pointing in direction
- `SetupStandardDice()` - Configure standard dice layout
- `AutoDetectCurrentFace(int)` - Auto-configure face currently pointing up
- `ValidateConfiguration()` - Check if setup is valid

**Inspector Buttons:**
- Setup Standard Dice
- Validate Configuration
- Detect Face 1-6 (for auto-configuration)

**Debug:**
- Show Debug Gizmos (toggle)
- Gizmo Length (size of debug lines)
- Current Reading (shows top value)
