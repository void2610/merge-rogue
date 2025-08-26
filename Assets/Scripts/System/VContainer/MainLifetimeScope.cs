using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// インベントリ初期化処理を担当するEntryPoint
/// VContainer管理下で相互参照を安全に設定
/// </summary>
public class InventoryInitializer : IStartable
{
    private readonly IInventoryService _inventoryService;
    private readonly InventoryUI _inventoryUI;
    
    public InventoryInitializer(IInventoryService inventoryService, InventoryUI inventoryUI)
    {
        _inventoryService = inventoryService;
        _inventoryUI = inventoryUI;
    }
    
    public void Start()
    {
        // 両方のオブジェクトが生成された後に相互参照を設定
        _inventoryService.SetInventoryUI(_inventoryUI);
        
        // UI設定後にインベントリを初期化
        _inventoryService.Initialize();
    }
}

/// <summary>
/// MainScene用のVContainer LifetimeScope
/// 段階的にDI化するため、現在は空（将来的にMainScene専用サービスを追加予定）
/// マウス関連サービスはGlobalLifetimeScopeで管理
/// VContainerSettingsで親子関係を管理
/// </summary>
public class MainLifetimeScope : LifetimeScope
{
    [Header("コンポーネント参照")]
    [SerializeField] private InventoryConfiguration inventoryConfiguration;
    [SerializeField] private EnemySpawnConfiguration enemySpawnConfiguration;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // マウス関連サービス（シーンごとに再生成）
        builder.Register<IVirtualMouseService, VirtualMouseService>(Lifetime.Scoped);
        builder.Register<IMouseCursorService, MouseCursorService>(Lifetime.Scoped);
        
        // RandomServiceをSettingsManagerのシード値で初期化
        builder.Register<IRandomService>(container =>
        {
            var settingsManager = container.Resolve<SettingsManager>();
            var seedValue = settingsManager.GetSeedValue();
            return new RandomService(seedValue);
        }, Lifetime.Singleton);
        
        // ContentServiceの登録
        builder.Register<IContentService>(container =>
        {
            var data = container.Resolve<ContentProviderData>();
            var enemyConfig = container.Resolve<EnemySpawnConfiguration>();
            var randomService = container.Resolve<IRandomService>();
            return new ContentService(data, enemyConfig, randomService);
        }, Lifetime.Singleton);
        
        builder.RegisterInstance(inventoryConfiguration);
        builder.RegisterInstance(enemySpawnConfiguration);
        builder.Register<IInventoryService, InventoryService>(Lifetime.Singleton);
        builder.Register<IRelicService, RelicService>(Lifetime.Singleton);
        builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
        builder.Register<IEnemyDifficultyService, EnemyDifficultyService>(Lifetime.Singleton);
        builder.Register<EnemySpawnService>(Lifetime.Singleton);
        builder.Register<IStageEventService, StageEventService>(Lifetime.Singleton);
        builder.Register<StageEventPresenter>(Lifetime.Singleton);
        
        builder.RegisterEntryPoint<MouseHoverUISelector>();
        builder.RegisterEntryPoint<InventoryInitializer>();
        
        // MainScene関連コンポーネントの依存注入を有効化
        builder.RegisterComponentInHierarchy<GameManager>();
        builder.RegisterComponentInHierarchy<MergeManager>();
        builder.RegisterComponentInHierarchy<StageManager>();
        builder.RegisterComponentInHierarchy<ScoreDisplayComponent>();
        builder.RegisterComponentInHierarchy<StageEventView>();
        builder.RegisterComponentInHierarchy<DescriptionWindow>();
        builder.RegisterComponentInHierarchy<InventoryUI>();
        builder.RegisterComponentInHierarchy<Treasure>();
        builder.RegisterComponentInHierarchy<Shop>();
        builder.RegisterComponentInHierarchy<AfterBattleUI>();
        builder.RegisterComponentInHierarchy<StatusEffectManager>();
        builder.RegisterComponentInHierarchy<EnemyContainer>();
        builder.RegisterComponentInHierarchy<MapGenerator>();
        builder.RegisterComponentInHierarchy<RelicUIManager>();
        builder.RegisterComponentInHierarchy<Rest>();
        builder.RegisterComponentInHierarchy<SeedText>();
        builder.RegisterComponentInHierarchy<UIManager>();
    }
}