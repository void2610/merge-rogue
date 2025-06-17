# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Merge Rogue is a Unity 6 physics-based puzzle roguelike game where players merge balls to create powerful attacks against enemies. The game features a relic system, procedural map generation, and localization support.

**Live game:** https://void2610.github.io/merge-rogue/  
**Unity Room:** https://unityroom.com/games/merge-rogue

## Development Environment

- **Unity Version:** 6000.0.42f1 
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Input System:** Unity Input System
- **Platform Targets:** WebGL, Desktop

## Key Dependencies

- **R3:** Reactive programming framework (Cysharp/R3)
- **UniTask:** Async/await support for Unity (Cysharp/UniTask)
- **DOTween:** Animation and tweening
- **Unity Localization:** Multi-language support (EN/JP)
- **NuGet for Unity:** Package management for external libraries

## Core Architecture

### Singleton Manager Pattern
- `GameManager`: Central state machine (Merge → EnemyAttack → AfterBattle → LevelUp → MapSelect)
- `UIManager`: Canvas group management and navigation
- `MergeManager`: Physics-based merge mechanics
- `InventoryManager`: Ball spawning and collection (10-slot fixed array)
- `RelicManager`: Power-up system with dynamic effects
- `StageManager`: Procedural map generation
- `EnemyContainer`: Battle system management

### Event-Driven Architecture
- **EventManager**: Central event system using R3 ReactiveX
- **GameEvent<T>**: Custom typed event wrapper with subscription/triggering
- **ReactiveProperty<T>**: Automatic UI data binding for health, coins, experience
- Event flow: Input → Effect → EventManager → StatusEffects/Relics → Apply

### Component-Based Entity System
- **BallBase**: Abstract base with 18+ specialized types (NormalBall, BombBall, etc.)
- **EnemyBase**: Base enemy with AI behavior via EnemyActionData
- **RelicBase**: Effect system with Init() → SubscribeEffect() → EffectImpl() pattern
- **IEntity**: Simplified interface for status effects with Dictionary<StatusEffectType, int> management

### Status Effect System (Refactored)
- **StatusEffectProcessor**: Static class managing all status effect logic via switch statements
- **Dictionary-based**: Uses `Dictionary<StatusEffectType, int>` for memory-efficient stack management
- **Centralized Logic**: All 10 status effects (Burn, Shield, Freeze, etc.) processed in single class
- **Data-Driven Timing**: StatusEffectTiming enum controls when effects trigger (OnTurnEnd, OnDamage, OnAttack, OnBattleEnd)

### Data-Driven Design
All game content uses ScriptableObjects with reflection-based instantiation:
- **BallData**: className, attacks[], sizes[], weights[], rarity, localization keys
- **RelicData**: className, sprite, rarity, localization keys
- **EnemyData**: sprites[], actions[], stats, AI parameters

## Key Systems

### Merge System
- Physics-based collision detection with Rigidbody2D
- Same-rank balls merge into next rank
- Serial number system prevents merge conflicts
- Freeze/Unfreeze mechanism during spawning

### Ball System
- 3 levels per ball type with scaling stats
- Override `Effect(BallBase other)` and `TurnEndEffect()` for custom behavior
- Level-based color coding system

### Relic System  
- Composition-based effects that subscribe to game events
- Dynamic loading via `Type.GetType(className)`
- UI integration with count display and interaction

### Status Effect Processing
- **Burn/Regeneration**: Turn-end damage/healing based on stack count
- **Shield/Invincible**: Damage absorption and immunity systems
- **Freeze**: Probability-based action skipping (stack × 10%, max 90%)
- **Confusion**: Player-only cursor control disruption during merge
- **Shock**: Enemy-only chain damage to other enemies
- **Curse**: Player-only disturbance ball generation
- **Power/Rage**: Attack modification (additive vs multiplicative)

### Localization
- Unity Localization package with Google Sheets integration
- Naming convention: `{className}_N` (name), `{className}_D` (description)
- Supported languages: English, Japanese

## File Organization

```
Assets/
├── Scripts/
│   ├── Ball/           # Ball type implementations
│   ├── Enemy/          # Enemy AI and behaviors  
│   ├── Merge/          # Physics merge system
│   ├── Player/         # Player stats and management
│   ├── Relic/          # Power-up effect system
│   ├── Shop/           # Shop and economy
│   ├── StageEvent/     # Map events and encounters
│   ├── StatusEffect/   # Temporary effect system
│   ├── System/         # Core managers and utilities
│   └── UI/             # User interface components
├── ScriptableObjects/  # Data definitions
│   ├── BallData/
│   ├── RelicData/
│   └── EnemyData/
├── Prefabs/           # Reusable game objects
├── Scenes/            # MainScene and TitleScene
└── Localization/      # String tables and settings
```

## Development Workflow

### Creating New Content

**New Ball Type:**
1. Create BallData ScriptableObject in `Assets/ScriptableObjects/BallData/`
2. Implement ball class inheriting from BallBase in `Assets/Scripts/Ball/`
3. Add to AllBallDataList asset
4. Add localization entries to string tables

**New Relic:**
1. Create RelicData ScriptableObject in `Assets/ScriptableObjects/RelicData/`
2. Implement relic class inheriting from RelicBase in `Assets/Scripts/Relic/`
3. Add to AllRelicDataList asset
4. Add localization entries to string tables

**New Status Effect:**
1. Add new enum value to `StatusEffectType` in `Assets/Scripts/Enums.cs`
2. Add case statement to appropriate method in `StatusEffectProcessor.cs`
3. Create StatusEffectData ScriptableObject with timing configuration
4. Add localization entries for effect name and description

### Code Patterns

**Event Subscription:**
```csharp
EventManager.OnPlayerDamaged.Subscribe(damage => { /* effect logic */ }).AddTo(this);
```

**Status Effect Management:**
```csharp
// Add status effect
StatusEffectProcessor.AddStatusEffect(entity, StatusEffectType.Burn, 3);

// Process turn end effects
await StatusEffectProcessor.ProcessTurnEnd(entity);

// Check specific conditions
if (StatusEffectProcessor.CheckFreeze(enemy)) return; // Skip action
```

**Data Access:**
```csharp
var ballData = ContentProvider.Instance.GetBallData(className);
var component = gameObject.AddComponent(Type.GetType(ballData.className));
```

**UI Navigation:**
```csharp
UIManager.Instance.EnableCanvasGroup("WindowName", true);
```

## Build Notes

- WebGL builds use custom template in `WebGLTemplates/`
- Uses Addressable Assets for content management
- NuGet packages may require manual setup after Unity version changes
- Localization assets are bundled with Addressables system

## Performance Considerations

- Object pooling patterns used for inventory management
- Composite Disposables for automatic R3 subscription cleanup
- Physics-based systems require careful performance monitoring
- UI uses DOTween for smooth animations without blocking gameplay
- **Status Effect Optimization**: Dictionary-based system reduces memory usage by ~70% vs object-based approach
- **Centralized Processing**: Switch statements eliminate virtual method calls and LINQ overhead

## Development Tools

### Unity Tools

Located in `unity-tools/` directory, these are simple and focused tools for Unity development workflow:

#### unity-compile.sh

A lightweight script for Unity compilation management with only two essential functions:

**Basic Usage:**
```bash
# Check current compilation errors
./unity-tools/unity-compile.sh check .

# Trigger Unity Editor compilation
./unity-tools/unity-compile.sh trigger .
```

**Key Features:**
- **Real-time Error Detection**: Analyzes Unity Editor.log for current compilation errors only
- **Compilation Triggering**: Sends Cmd+R to Unity Editor via AppleScript to trigger recompile
- **Recent Log Analysis**: Checks only the latest 100 lines to avoid stale error messages
- **Simple Interface**: Two commands only - `check` and `trigger`

**Output Examples:**

Success:
```
📋 Checking Unity log: /Users/user/Library/Logs/Unity/Editor.log
✅ No recent compilation errors detected
📝 Last compile status: CompileScripts: 1.603ms
```

Errors detected:
```
📋 Checking Unity log: /Users/user/Library/Logs/Unity/Editor.log
❌ Recent compilation errors found:
Assets/Scripts/Example.cs(11,9): error CS0103: The name 'NonExistentMethod' does not exist in the current context
```

**Design Philosophy:**
- **Simplicity**: 89 lines vs. previous 657 lines (86% reduction)
- **Accuracy**: Only analyzes recent log entries to avoid false positives
- **Speed**: Minimal overhead with focused functionality
- **Reliability**: Robust error detection patterns for Unity Editor logs

**Requirements:**
- macOS (for AppleScript compilation triggering)
- Unity Editor must be running for `trigger` command
- Unity Editor.log must be accessible for `check` command

This tool enables efficient compilation error detection and resolution during development without complex configuration or verbose output.

## Recent Major Refactoring

### Status Effect System Overhaul
The status effect system was completely refactored from an object-oriented to a data-oriented approach:

**Before**: Individual StatusEffectBase classes with inheritance hierarchy
**After**: Single StatusEffectProcessor static class with Dictionary<StatusEffectType, int> management

**Benefits**:
- 75% code reduction (1,200 → 300 lines)
- 70% memory usage reduction
- Simplified debugging and maintenance
- Easier addition of new status effects (2 locations vs 4+ files)

**Key Changes**:
- All status effect logic centralized in `StatusEffectProcessor.cs`
- Enums consolidated in `Enums.cs` (StatusEffectType, StatusEffectTiming)
- IEntity interface simplified to Dictionary-based approach
- Collection modification exceptions resolved with safe iteration patterns