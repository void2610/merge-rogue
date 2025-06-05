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
- **IEntity**: Shared interface for status effects (Player and EnemyBase)

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

### Code Patterns

**Event Subscription:**
```csharp
EventManager.OnPlayerDamaged.Subscribe(damage => { /* effect logic */ }).AddTo(this);
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

## Development Tools

### Unity Compile Check Script

A custom shell script `unity-compile-check.sh` is available for automated compilation error detection:

**Basic Usage:**
```bash
# Standard compile check
./unity-compile-check.sh .

# Check editor logs only (when Unity is running)
./unity-compile-check.sh -e .

# Force Unity recompile + check editor logs
./unity-compile-check.sh -ef .

# Quit Unity first, then compile check
./unity-compile-check.sh -q .

# Verbose output with detailed logs
./unity-compile-check.sh -v .
```

**Key Features:**
- **Editor Log Analysis**: Reads Unity Editor.log directly for real-time error detection
- **Automatic Unity Detection**: Finds project-specific Unity version automatically
- **Force Recompile**: Triggers Unity Editor recompilation via AppleScript (macOS)
- **Multiple Fallback Methods**: File system watching, process signals, keyboard shortcuts
- **CI/CD Ready**: Suitable for automated build pipelines

**Options:**
- `-e, --editor-only`: Check editor logs without running Unity (fast)
- `-f, --force-compile`: Force Unity Editor to recompile before checking
- `-q, --quit`: Quit Unity Editor before running compile check (reliable)
- `-v, --verbose`: Show detailed compilation logs and debug information

**Error Detection Patterns:**
- C# compiler errors (`error CS####:`)
- Unity-specific compilation failures
- Script compilation timeouts
- Assembly loading issues

This tool enables Claude Code to automatically detect and fix compilation errors without manual Unity Editor interaction.