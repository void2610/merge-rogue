# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 🚨 CRITICAL DEVELOPMENT PRINCIPLES 🚨

### エラーハンドリング方針
**絶対に開発者の設定ミスによるランタイムエラーを隠してはならない**

- ❌ **禁止**: `if (relicData != null)` のような開発者設定ミスによるnullチェック
- ❌ **禁止**: 設定不備を警告で済ませる `Debug.LogWarning("設定されていません")`
- ✅ **推奨**: 設定ミスは即座にクラッシュさせ、問題を明確化する

**理由**: 
- ランタイムエラーを隠す実装は不具合の原因となる
- 開発者が問題に気付かず、本番で予期しない動作が発生する
- エラーは早期に発見し、設定時点で修正すべき

**正しいアプローチ**:
```csharp
// ❌ 間違い - 完全に不要で有害
if (relicData != null) 
{
    relicData.DoSomething();
    Debug.Log("何かしました"); // デバッグ文も不要で可読性を下げる
}

// ✅ 正しい - シンプルで直接的
relicData.DoSomething();
```

**追加の重要原則**:
- **不要なif文の完全禁止**: 開発者設定ミスのチェックは一切書かない
- **デバッグ文の禁止**: `Debug.Log()` は可読性を落とすだけの最低な行為
- **最小限の実装**: 本質的でない全てのコードを削除する

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
- **VContainer:** Dependency injection framework (hadashiA/VContainer)
- **DOTween:** Animation and tweening
- **Unity Localization:** Multi-language support (EN/JP)
- **NuGet for Unity:** Package management for external libraries

## Common Development Commands

### Unity Compilation Management
```bash
# Check current compilation errors
./unity-tools/unity-compile.sh check .

# Trigger Unity Editor compilation (macOS only)
./unity-tools/unity-compile.sh trigger .
```

### Build Commands
- **WebGL Build:** Unity Editor → File → Build Settings → WebGL → Build
- **Addressables Build:** Window → Asset Management → Addressables → Groups → Build → New Build → Default Build Script

## Core Architecture

### VContainer Dependency Injection System

The project uses a hierarchical VContainer setup with multiple LifetimeScopes:

#### LifetimeScope Hierarchy
```
RootLifetimeScope (DontDestroyOnLoad)
├── TitleLifetimeScope (TitleScene)
└── MainLifetimeScope (MainScene)
```

#### RootLifetimeScope
- **Purpose**: Manages globally shared services that persist across scene transitions
- **Services**: 
  - `IInputProvider/InputProviderService`: Input system management
  - `CursorConfiguration`: Cursor texture and hotspot data
- **Lifecycle**: Persists throughout application lifetime with `DontDestroyOnLoad`

#### TitleLifetimeScope
- **Purpose**: Title scene-specific services and components
- **Services**:
  - `ICreditService/CreditService`: Credit text management
  - `ILicenseService/LicenseService`: License information display
  - `IVersionService/VersionService`: Version number display
  - `IGameSettingsService/GameSettingsService`: Audio settings and seed management
  - `IVirtualMouseService/VirtualMouseService`: Virtual mouse for gamepad support (Scoped)
  - `IMouseCursorService/MouseCursorService`: Custom cursor management (Scoped)
- **Components**: `TitleMenu`, `Encyclopedia`

#### MainLifetimeScope
- **Purpose**: Main game scene services (future expansion planned)
- **Current Services**:
  - `IVirtualMouseService/VirtualMouseService`: Virtual mouse for gamepad support (Scoped)
  - `IMouseCursorService/MouseCursorService`: Custom cursor management (Scoped)
- **Planned Services**: GameManager, MergeManager, InventoryManager integration

#### Service Lifetime Management
- **Singleton**: Services that maintain state across the scope's lifetime
- **Scoped**: Services that are recreated for each scene (used for scene-specific resources)
- **Key Pattern**: VirtualMouseService and MouseCursorService use `Lifetime.Scoped` to ensure proper GameObject reference management across scene transitions

### Singleton Manager Pattern (Legacy)
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

### Status Effect System
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
│   │   ├── Services/   # VContainer service implementations
│   │   └── VContainer/ # LifetimeScope configurations
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

### VContainer Service Development

**Creating New Services:**
1. Define interface in `Assets/Scripts/System/Services/`
2. Implement service class with proper constructor injection
3. Register in appropriate LifetimeScope
4. Use `[Inject]` attribute for MonoBehaviour dependency injection

**Service Registration Patterns:**
```csharp
// Pure C# service
builder.Register<IMyService, MyService>(Lifetime.Singleton);

// Service with parameters
builder.Register<IMyService, MyService>(Lifetime.Singleton)
    .WithParameter("paramName", paramValue);

// MonoBehaviour component
builder.RegisterComponentInHierarchy<MyComponent>()
    .AsSelf()
    .AsImplementedInterfaces();
```

**Dependency Injection Patterns:**
```csharp
// Constructor injection (pure C# services)
public MyService(IDependency dependency)
{
    _dependency = dependency;
}

// Method injection (MonoBehaviour components)
[Inject]
public void InjectDependencies(IMyService myService)
{
    _myService = myService;
}
```

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

**VContainer Service Registration:**
```csharp
// In TitleLifetimeScope or MainLifetimeScope
builder.Register<IGameSettingsService, GameSettingsService>(Lifetime.Singleton);
builder.Register<IVirtualMouseService, VirtualMouseService>(Lifetime.Scoped);
builder.RegisterComponentInHierarchy<TitleMenu>();
```

**VContainer Dependency Injection:**
```csharp
// Constructor injection for services
public GameSettingsService(IInputProvider inputProvider)

// Method injection for MonoBehaviours
[Inject]
public void InjectDependencies(IGameSettingsService gameSettingsService)
```

**Manual Service Resolution (Fallback):**
```csharp
// In SetMouseCursor.cs pattern for components that need manual resolution
private VContainer.Unity.LifetimeScope FindChildLifetimeScope()
{
    var allScopes = FindObjectsByType<VContainer.Unity.LifetimeScope>(FindObjectsSortMode.None);
    
    foreach (var scope in allScopes)
    {
        // Skip RootLifetimeScope (DontDestroyOnLoad)
        if (scope.gameObject.scene.name == "DontDestroyOnLoad") continue;
        
        // Find scope with required service
        if (scope.Container != null && scope.Container.TryResolve<IMouseCursorService>(out _))
        {
            return scope;
        }
    }
    
    return null;
}
```

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
- **VContainer Optimization**: Scoped services ensure proper cleanup and recreation for scene-specific resources
- **Service Lifetime Management**: Careful balance between Singleton (global state) and Scoped (scene-specific) lifetimes

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

## Current VContainer Migration

### Architecture Overview

The project is currently undergoing a systematic migration from Singleton pattern to VContainer dependency injection:

**Completed:**
- **RootLifetimeScope**: Global services (InputProvider, CursorConfiguration)
- **TitleLifetimeScope**: Title scene services (Credit, License, Version, GameSettings, Mouse services)
- **MainLifetimeScope**: Basic setup with Mouse services
- **Service Layer**: Interface-based service implementations
- **Component Integration**: SetMouseCursor with fallback manual resolution

**Key Design Decisions:**
1. **Service Lifetime Strategy**: 
   - `Singleton` for stateful services that should persist within their scope
   - `Scoped` for services that need to be recreated per scene (e.g., services referencing scene-specific GameObjects)

2. **Mouse Service Architecture**:
   - `VirtualMouseService` and `MouseCursorService` use `Scoped` lifetime
   - Ensures proper GameObject reference management across scene transitions
   - Each scene gets fresh instances that reference the correct scene's UI elements

3. **Fallback Resolution Pattern**:
   - Components like `SetMouseCursor` include manual service resolution as fallback
   - `FindChildLifetimeScope()` method excludes RootLifetimeScope and finds appropriate scene scope
   - Ensures robustness when automatic injection fails

**Migration Benefits:**
- Clear separation of concerns between UI and business logic
- Improved testability through interface-based design
- Centralized dependency configuration
- Proper lifecycle management for scene-specific resources
- Support for both MonoBehaviour components and pure C# services

**Future Migration Plans:**
- GameManager → IGameService
- MergeManager → IMergeService  
- InventoryManager → IInventoryService
- Progressive replacement of singleton pattern throughout the codebase

## Unity Development Guidelines

### Unity Object Null Checks

When checking Unity objects (GameObjects, Components) for null, use implicit boolean conversion instead of explicit null comparison to avoid Rider warnings about "implicitly checking Unity object lifetime":

```csharp
// ❌ Avoid - Causes Rider warning
if (titleText != null)
{
    titleText.text = "Hello";
}

// ✅ Preferred - No warning
if (titleText)
{
    titleText.text = "Hello";
}

// ✅ Also good for negation
if (!tutorialImage || !imageContainer) return;
```

This applies to all Unity object types including:
- MonoBehaviour components (Text, Image, Button, etc.)
- GameObject
- Transform
- Any Unity.Object derived types

## Asynchronous Programming Guidelines

- **Coroutine Usage**: 
  - **Rule:** Coroutines are absolutely prohibited
  - **Replacement:** Use UniTask for all asynchronous processing

## Additional Development Guidelines

- **Coroutineは絶対に使用せず、非同期処理にはUniTaskを使用すること。**
```

</invoke>