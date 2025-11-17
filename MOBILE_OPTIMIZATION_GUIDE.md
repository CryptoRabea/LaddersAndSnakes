# Mobile Optimization Guide

## Overview

This document describes the comprehensive mobile optimizations implemented for the Ladders and Snakes game. The project has been fully migrated to Unity's New Input System and optimized for mobile devices including touch controls, responsive UI, and performance enhancements.

## Table of Contents

1. [New Input System Migration](#new-input-system-migration)
2. [Mobile-Specific Scripts](#mobile-specific-scripts)
3. [UI Optimizations](#ui-optimizations)
4. [Testing on Mobile Devices](#testing-on-mobile-devices)
5. [Performance Considerations](#performance-considerations)

---

## New Input System Migration

### What Changed

The project has been migrated from Unity's legacy EventSystem (IPointerDownHandler/IPointerUpHandler) to the **Unity New Input System** (com.unity.inputsystem 1.14.2).

### Benefits

- **Better mobile support**: Native touch input handling
- **Cross-platform**: Works seamlessly on desktop (mouse/keyboard) and mobile (touch)
- **Gesture recognition**: Support for swipes, long press, and multi-touch
- **Haptic feedback**: Vibration feedback on supported devices
- **Modern API**: Future-proof with Unity's recommended input solution

### Input Actions

A new Input Actions asset has been created at:
- **Location**: `Assets/InputActions/GameInputActions.inputactions`

**Actions defined:**
- `DicePress`: Touch/mouse press for dice rolling
- `TouchPosition`: Track touch/mouse position
- `AlternateRoll`: Keyboard spacebar for desktop

**Control Schemes:**
- `Touch`: For mobile devices (Touchscreen)
- `Keyboard&Mouse`: For desktop (Keyboard + Mouse)

### Affected Scripts

#### ManualDiceRoller.cs
**Changes:**
- Removed `IPointerDownHandler` and `IPointerUpHandler` interfaces
- Added Unity New Input System actions
- Integrated `MobileInputHelper` for gestures and haptics
- Added haptic feedback on dice press and throw
- Optional swipe-to-roll feature

**New Features:**
- Touch hold-to-shake, release-to-throw
- Mouse click support (same mechanic)
- Keyboard spacebar for quick roll
- Haptic feedback (vibration) on mobile
- Swipe up to roll (optional, disabled by default)

---

## Mobile-Specific Scripts

### 1. SafeAreaHandler.cs

**Purpose**: Handles safe area for mobile devices with notches, rounded corners, and status bars.

**Usage:**
```csharp
// Attach to any RectTransform that should respect safe area
[RequireComponent(typeof(RectTransform))]
public class SafeAreaHandler : MonoBehaviour
```

**Features:**
- Automatically adjusts UI to avoid notches and system UI
- Configurable X and Y axis conformance
- Continuous update option for orientation changes
- Debug info available via `GetSafeAreaInfo()`

**Recommended Usage:**
Attach this component to your main Canvas or root UI panels to ensure proper display on devices with notches (iPhone X+, modern Android devices).

### 2. MobileInputHelper.cs

**Purpose**: Provides gesture detection, touch events, and haptic feedback.

**Features:**
- Swipe detection (left, right, up, down)
- Touch events (started, moved, ended)
- Long press detection
- Haptic feedback with different intensities
- Multi-touch support ready

**Events Available:**
```csharp
public System.Action<Vector2> OnSwipe;
public System.Action OnSwipeLeft;
public System.Action OnSwipeRight;
public System.Action OnSwipeUp;
public System.Action OnSwipeDown;
public System.Action<Vector2> OnTouchStarted;
public System.Action<Vector2> OnTouchMoved;
public System.Action<Vector2> OnTouchEnded;
public System.Action<float> OnLongPress;
```

**Haptic Feedback Types:**
- `Selection`: Light tap (10ms) - for UI selections
- `LightImpact`: Light impact (20ms) - for button presses
- `MediumImpact`: Medium impact (40ms) - for important actions
- `HeavyImpact`: Heavy impact (60ms) - for critical events

**Platform Support:**
- ✅ Android (via vibrator service)
- ✅ iOS (via Handheld.Vibrate, can be enhanced with native plugins)
- ❌ Desktop (no haptics)

### 3. ResponsiveUIScaler.cs

**Purpose**: Adapts UI scaling to different screen sizes and orientations.

**Features:**
- Automatic orientation detection
- Separate reference resolutions for portrait and landscape
- Mobile-specific scaling multiplier
- DPI-aware button and text sizing
- Notch detection

**Configuration:**
- Portrait Reference Resolution: 1080x1920 (default)
- Landscape Reference Resolution: 1920x1080 (default)
- Mobile Scale Multiplier: 1.2x (default)

**Helper Methods:**
```csharp
public bool IsPortrait();
public bool IsLandscape();
public float GetAspectRatio();
public static bool HasNotch();
public float GetRecommendedButtonSize();
public float GetRecommendedTextSize(float baseSize);
```

**Recommended Usage:**
Attach this component to your Canvas object. It will automatically adjust the CanvasScaler based on device orientation and screen size.

---

## UI Optimizations

### Touch-Friendly Controls

All UI elements have been optimized for touch interaction:

#### Button Sizes
- **Minimum height**: 100 pixels (recommended touch target: 44-48 dp)
- **Scale multiplier on mobile**: 1.2x - 1.3x
- **Spacing**: Increased padding between buttons

#### Input Fields
- **Minimum height**: 80 pixels
- **Font size on mobile**: Minimum 32-36 points
- **Touch area**: Enlarged for easier text selection

#### Text Readability
- **Mobile text scale**: 1.1x - 1.2x
- **Font sizes**: Automatically enlarged on mobile
- **Contrast**: Maintained for outdoor visibility

### Optimized Scripts

#### ManualGameManager.cs
**Mobile Optimizations:**
- Buttons scaled 1.3x on mobile
- Text enlarged 1.2x
- Minimum move speed for better visibility
- Auto-applied on `Start()` if running on mobile

#### MainMenuController.cs
**Mobile Optimizations:**
- All buttons scaled 1.2x
- Minimum button height: 100px
- Input fields enlarged (80px height, 32pt font)
- Dropdowns enlarged for easier selection

#### PlayerSetupPanel.cs
**Mobile Optimizations:**
- Input fields: 80px height, 36pt font
- Buttons: 1.2x scale, 100px min height
- Text: 1.1x - 1.2x larger
- Touch-friendly number input

#### MultiplayerSetupPanel.cs
**Mobile Optimizations:**
- Room name input: 80px height, 36pt font
- All buttons: 1.2x scale, 100px min height
- Status text: 1.2x larger

#### RoomListingManager.cs
**Mobile Optimizations:**
- Scrollable room list optimized for touch
- Scroll sensitivity: 30 (increased for touch)
- Inertia scrolling enabled
- Room items enlarged for easier tapping
- Input fields: 80px height, 36pt font

---

## Testing on Mobile Devices

### Build Settings

**Android:**
```
Target Device: Mobile
Minimum API Level: Android 7.0 (API 24) or higher
Target API Level: Latest (API 34+)
Aspect Ratio Support: 1:1 to 2.1:1 (for tall screens)
```

**iOS:**
```
Minimum iOS Version: 13.0 or higher
Target Device: iPhone, iPad
Requires Full Screen: Yes (for safe area detection)
```

### Unity Player Settings to Configure

1. **Enable New Input System:**
   - Edit → Project Settings → Player → Other Settings
   - Active Input Handling: **Input System Package (New)** or **Both**

2. **Mobile Optimization:**
   - Edit → Project Settings → Player → Other Settings
   - Accelerometer Frequency: 60 Hz
   - Graphics API: Auto (or Vulkan for Android)

3. **Quality Settings:**
   - Edit → Project Settings → Quality
   - Mobile preset recommended
   - Disable shadows on low-end devices
   - V-Sync: Off (use target frame rate instead)

### Testing Checklist

- [ ] Touch input works for dice rolling
- [ ] Buttons are easily tappable (no missed touches)
- [ ] Text is readable without zooming
- [ ] Safe area respected on notched devices
- [ ] Haptic feedback works (if enabled)
- [ ] Orientation changes handled smoothly
- [ ] Scrolling is smooth in room list
- [ ] Input fields accept text properly
- [ ] No UI overlaps with system UI

### Debugging on Device

**Enable Debug Logs:**
All mobile optimization scripts output debug logs when applied:
- "Applying mobile optimizations..."
- "Mobile optimizations applied"
- "SafeArea applied: ..."

**Check Safe Area:**
```csharp
Debug.Log(SafeAreaHandler.GetSafeAreaInfo());
```

**Check Haptics:**
```csharp
if (MobileInputHelper.SupportsHaptics())
{
    Debug.Log("Haptics supported!");
}
```

---

## Performance Considerations

### Mobile-Specific Optimizations

1. **Input System:**
   - Uses Unity's optimized Enhanced Touch API
   - Minimal overhead for touch processing
   - Efficient gesture detection

2. **UI Scaling:**
   - Dynamic scaling only on orientation change
   - Cached calculations
   - No per-frame updates unless needed

3. **Haptic Feedback:**
   - Short vibration bursts (10-60ms)
   - Conditional compilation (#if UNITY_ANDROID)
   - Can be disabled via inspector

4. **Physics:**
   - Dice settling time optimized
   - Rigidbody sleep for static objects
   - Reduced gravity calculations when not rolling

### Recommended Settings for Low-End Mobile

In ManualDiceRoller.cs:
```csharp
[SerializeField] private bool usePhysics = false; // Use animation instead
[SerializeField] private float settleTime = 1.5f; // Reduce from 2.5s
```

In ManualGameManager.cs:
```csharp
[SerializeField] private float moveSpeed = 8f; // Faster movement
```

### Memory Management

- Input actions properly disposed in OnDestroy
- Mobile input helper unsubscribes from events
- UI elements scaled once, not per-frame
- Room list items pooled (destroyed when cleared)

---

## Migration Checklist

If you're setting up a scene with these optimizations:

### Scene Setup

1. **Canvas Setup:**
   - [ ] Add `ResponsiveUIScaler` to Canvas
   - [ ] Add `SafeAreaHandler` to root UI panel
   - [ ] Configure reference resolutions

2. **Dice Roller:**
   - [ ] Ensure `ManualDiceRoller` is on dice button
   - [ ] Configure haptic feedback settings
   - [ ] Test touch press/release

3. **Game Manager:**
   - [ ] Enable mobile optimizations flag
   - [ ] Adjust button scale multipliers if needed
   - [ ] Test on target device

4. **Input System:**
   - [ ] Player Settings: Enable New Input System
   - [ ] Import GameInputActions.inputactions
   - [ ] Verify touch controls work

### Code Migration (if adding to new scripts)

**Old EventSystem Pattern:**
```csharp
public class OldScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
}
```

**New Input System Pattern:**
```csharp
public class NewScript : MonoBehaviour
{
    private InputAction pressAction;

    void OnEnable()
    {
        pressAction = new InputAction("Press", binding: "<Touchscreen>/primaryTouch/press");
        pressAction.AddBinding("<Mouse>/leftButton");
        pressAction.started += OnPressStarted;
        pressAction.canceled += OnPressCanceled;
        pressAction.Enable();
    }

    void OnDisable()
    {
        pressAction.started -= OnPressStarted;
        pressAction.canceled -= OnPressCanceled;
        pressAction.Disable();
    }

    void OnDestroy()
    {
        pressAction?.Dispose();
    }

    private void OnPressStarted(InputAction.CallbackContext context) { }
    private void OnPressCanceled(InputAction.CallbackContext context) { }
}
```

---

## Troubleshooting

### Issue: Touch not working
**Solution:**
1. Check Player Settings → Active Input Handling is set to "Input System Package (New)" or "Both"
2. Ensure EventSystem is in scene
3. Verify Canvas has GraphicRaycaster component

### Issue: Haptics not working
**Solution:**
1. Check device supports vibration (not all emulators do)
2. Verify app has vibration permission (Android)
3. Enable haptics in ManualDiceRoller inspector

### Issue: UI too small/large on device
**Solution:**
1. Adjust ResponsiveUIScaler reference resolutions
2. Change mobile scale multiplier
3. Check Canvas Scaler match mode

### Issue: UI clipped by notch
**Solution:**
1. Add SafeAreaHandler to root panel
2. Enable conformX and conformY
3. Test on actual device (not emulator)

---

## Additional Resources

### Unity New Input System
- [Official Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/index.html)
- Package: com.unity.inputsystem 1.14.2 (already installed)

### Mobile Development
- [Unity Mobile Optimization Guide](https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html)
- [Touch Input](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Touch.html)

### Safe Area
- [Unity Safe Area Documentation](https://docs.unity3d.com/ScriptReference/Screen-safeArea.html)

---

## Summary of Changes

### New Files Created
1. `Assets/Scripts/SafeAreaHandler.cs` - Safe area management
2. `Assets/Scripts/MobileInputHelper.cs` - Gestures and haptics
3. `Assets/Scripts/ResponsiveUIScaler.cs` - Responsive UI scaling
4. `Assets/InputActions/GameInputActions.inputactions` - Input action definitions

### Modified Files
1. `Assets/Scripts/ManualDiceRoller.cs` - Migrated to New Input System + haptics
2. `Assets/Scripts/ManualGameManager.cs` - Mobile UI optimizations
3. `Assets/Scripts/MainMenuController.cs` - Touch-friendly controls
4. `Assets/Scripts/PlayerSetupPanel.cs` - Enlarged inputs and buttons
5. `Assets/Scripts/MultiplayerSetupPanel.cs` - Responsive layout
6. `Assets/Scripts/RoomListingManager.cs` - Mobile scrolling optimization

### Key Improvements
- ✅ Fully migrated to Unity New Input System
- ✅ Touch controls optimized for mobile
- ✅ Haptic feedback on dice interactions
- ✅ Safe area handling for notched devices
- ✅ Responsive UI scaling for all screen sizes
- ✅ Enlarged buttons and text for better touch targets
- ✅ Smooth scrolling for room lists
- ✅ Cross-platform input support (mobile + desktop)

---

**Last Updated:** 2025-11-17
**Unity Version:** 2023.x or higher
**Input System Version:** 1.14.2
