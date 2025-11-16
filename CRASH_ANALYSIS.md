# Crash Log Analysis

## Overview
Multiple Unity Editor crashes occurred on **November 16, 2025** around **15:50:34** and **14:19:34**. The crashes affected both the main Editor and multiple AssetImportWorker processes.

## Critical Issues Found

### 1. **Memory Corruption/Exhaustion** (CRITICAL)
**Location**: `Crash_2025-11-16_155034486/Editor.log:9876`

```
[Worker0] Fatal Error! Could not allocate memory: System out of memory!
Trying to allocate: 11184824B with 16 alignment. MemoryLabel: Texture
```

**Memory Report Anomaly**:
```
[ ALLOC_DEFAULT ] used: 152583984B | reserved: 851931068480B
```
- Reserved memory shows **851GB** (clearly invalid on a 16GB system)
- Indicates severe memory corruption or leak
- Failed to allocate ~11MB for a texture despite only using ~153MB

### 2. **System Resource Exhaustion** (CRITICAL)
**Location**: `Editor.log:10253`

```
mono_os_event_init: CreateEvent failed with error 1450
```
- Windows error 1450: "Insufficient system resources exist to complete the requested service"
- Unable to create system synchronization primitives

### 3. **Shader Compiler Launch Failures** (HIGH)
**Location**: `Editor.log:10237, 10248`

```
Aborting batchmode due to fatal error:
Shader compiler initialization error: Failed to launch UnityShaderCompiler.exe shader compiler!
```
- Likely caused by resource exhaustion from issue #2
- Multiple workers unable to spawn shader compiler subprocess

### 4. **Missing Reference Exception** (MEDIUM)
**Location**: `Editor.log:8127-8138`
**Code**: `Assets/Scripts/Editor/GameSceneSetup.cs:270`

```
MissingReferenceException: The object of type 'UnityEngine.Transform' has been destroyed
but you are still trying to access it.
  at LAS.Editor.GameSceneSetup.CreateDiceButton (UnityEngine.Transform parent)
  at LAS.Editor.GameSceneSetup.CreateGameUI ()
```

**Call Stack**:
1. `SetupScene()` → `SetupSceneInternal()`
2. `CreateGameUI()`
3. `CreateDiceButton(Transform parent)` - crashes at line 270
4. Attempting to add Button component to potentially destroyed GameObject

### 5. **Asset Preview Generation Crashes** (HIGH)
**Location**: Multiple AssetImportWorker logs

```
UnityEditor.AssetPreviewUpdater:CreatePreviewForAsset
UnityEditor.GameObjectInspector:RenderStaticPreview
UnityEngine.Rendering.Universal.UniversalRenderPipeline:Render
```
- Crashes during asset preview thumbnail generation
- Occurs in rendering pipeline culling phase
- Affects multiple import workers simultaneously

## Crash Pattern

All crashes follow this pattern:
1. Editor starts generating asset previews
2. Memory corruption begins (invalid 851GB reservation)
3. System resources get exhausted
4. Shader compiler fails to launch
5. Mono runtime can't create system events
6. Complete system crash

## Root Cause Analysis

**Primary Issue**: Memory corruption/leak in Unity Editor or URP rendering pipeline
- Invalid memory reservations (851GB on 16GB system)
- Cascading failures due to resource exhaustion
- Asset import workers crash during preview generation

**Secondary Issue**: Code issue in GameSceneSetup.cs
- Attempting to access destroyed Transform objects
- May be triggered by the memory corruption state

## Affected Components

1. **Unity Editor Version**: 6000.2.10f1 (d3d30d158480)
2. **URP (Universal Render Pipeline)**: Package `com.unity.render-pipelines.universal@4976252adeb8`
3. **Custom Code**: `GameSceneSetup.cs` in Editor scripts
4. **System**: Windows 11 (10.0.26200) 64bit, 16GB RAM

## Recommendations

### Immediate Actions:
1. **Clear Unity Caches**:
   - Delete `Library/` folder
   - Delete `Temp/` folder
   - Reimport all assets fresh

2. **Disable Asset Preview Generation**:
   - Edit → Preferences → Asset Pipeline → Disable "Auto Refresh"
   - Reduce/disable thumbnail generation in Project window

3. **Fix GameSceneSetup.cs**:
   - Add null checks before accessing Transform components at line 270
   - Use try-catch blocks around GameObject creation
   - Check if objects are being prematurely destroyed

4. **Reduce Memory Pressure**:
   - Close other applications
   - Reduce texture import sizes temporarily
   - Limit number of AssetImportWorkers in Project Settings

### Long-term Solutions:
1. **Update Unity Editor**: Check for patches to 6000.2.x line
2. **Update URP Package**: Update to latest stable version
3. **Add Defensive Coding**:
   ```csharp
   if (buttonGO == null || buttonGO.transform == null) {
       Debug.LogError("Button GameObject was destroyed!");
       return;
   }
   ```
4. **Monitor Memory Usage**: Use Unity Profiler to track memory allocations
5. **Report to Unity**: File bug report with crash dumps

## Files Affected

- `/Assets/Scripts/Editor/GameSceneSetup.cs` - Line 270
- Unity Editor memory management system
- URP rendering pipeline

## Crash Statistics

- **Total Crash Sessions**: 5+ separate crash events
- **AssetImportWorker Crashes**: 8 workers (0,1,3,4,5,7,8,14)
- **Memory Corruption Events**: Multiple instances showing 851GB reserved
- **Error Code 1450 Occurrences**: Multiple

---

**Generated**: 2025-11-16
**Analysis Tool**: Claude Code
