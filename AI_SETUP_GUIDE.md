# AI Setup Guide - Play vs AI

## How to Play Against AI

The game now has a working "Play vs AI" feature! Here's how to use it:

### Quick Start

1. **Launch the game** and go to the main menu
2. **Click "Play AI"** button
3. **You're ready!** You'll play as Player 1 (Red), and the AI is Player 2 (Blue)

### How It Works

When you click "Play vs AI":
- You (human) are assigned as Player 0
- AI opponent is assigned as Player 1
- The game automatically switches turns between you and the AI

### Turn Flow

1. **Your Turn**: Click the "Roll Dice" button to roll
2. **AI Turn**: The AI automatically rolls after a 1-second "thinking" delay
3. The game continues alternating until someone wins

### What Was Fixed

The AI system existed in the codebase but wasn't being activated. The fixes:

1. **GameSetupManager.cs** (`SetupGameController` method):
   - Now automatically adds `AIPlayerController` component when in AI mode
   - Checks `NetworkManager.IsSinglePlayerAI` flag

2. **AIPlayerController.cs**:
   - Updated deprecated Unity APIs (`FindObjectOfType` → `FindFirstObjectByType`)
   - Fixed warnings in Unity 2023.x+

### Technical Details

**AI Behavior:**
- **Delay**: 1 second "thinking" time before rolling (configurable)
- **Player Index**: AI is always Player 1 in single-player mode
- **Auto-play**: No button clicks needed - AI rolls automatically

**Key Components:**
- `AIPlayerController` - Listens for turn events and auto-rolls dice
- `NetworkManager.IsSinglePlayerAI` - Flag that activates AI mode
- `GameSetupManager` - Creates AI controller when scene loads

### Customization

You can adjust AI behavior by modifying these settings in `AIPlayerController.cs`:

```csharp
[SerializeField] private float thinkDelay = 1.0f;  // Time AI waits before rolling
[SerializeField] private int aiPlayerIndex = 1;     // Which player is the AI
```

### Current Limitations

The AI is currently **basic**:
- ✅ Automatically rolls dice when it's its turn
- ❌ No strategic decision-making
- ❌ No difficulty levels
- ❌ Only supports 1 AI opponent (no multi-AI games)

### Future Enhancements

Potential AI improvements:
- Multiple difficulty levels (Easy, Medium, Hard)
- Strategic decision-making for ladder/snake positions
- Multiple AI opponents in one game
- AI personalities with different play styles

### Debugging

If AI doesn't work, check Unity Console for:
- `[GameSetupManager] Added AIPlayerController for single player AI mode`
- `[AIPlayerController] AI is rolling the dice`

If you don't see these messages, ensure:
1. You clicked "Play AI" button (not "Play Local")
2. `GameSetupManager` component exists in GameScene
3. No errors preventing scene setup

## Testing Checklist

- [ ] Main menu loads correctly
- [ ] "Play AI" button is visible
- [ ] Clicking "Play AI" loads the game scene
- [ ] You can roll dice on your turn
- [ ] AI automatically rolls after your turn ends
- [ ] Game alternates between human and AI turns
- [ ] Winner is determined correctly

---

**Files Modified:**
- `Assets/Scripts/Gameplay/GameSetupManager.cs` - Auto-creates AIPlayerController
- `Assets/Scripts/Gameplay/AIPlayerController.cs` - Updated Unity APIs

**Related Files:**
- `Assets/Scripts/UI/MainMenuController.cs` - "Play AI" button handler
- `Assets/Scripts/Networking/NetworkManager.cs` - AI mode flag
