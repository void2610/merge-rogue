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
    
    
    protected override void Configure(IContainerBuilder builder)
    {
        // マウス関連サービス（シーンごとに再生成）
        builder.Register<IVirtualMouseService, VirtualMouseService>(Lifetime.Scoped);
        builder.Register<IMouseCursorService, MouseCursorService>(Lifetime.Scoped);
        
        builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
        builder.Register<IRelicService, RelicService>(Lifetime.Singleton);
        
        builder.RegisterInstance(inventoryConfiguration);
        builder.Register<IInventoryService, InventoryService>(Lifetime.Singleton);
        
        builder.RegisterEntryPoint<MouseHoverUISelector>(Lifetime.Singleton);
        
        // インベントリ初期化処理（相互参照設定）
        builder.RegisterEntryPoint<InventoryInitializer>(Lifetime.Singleton);
        
        // MainScene関連コンポーネントの依存注入を有効化
        builder.RegisterComponentInHierarchy<GameManager>();
        builder.RegisterComponentInHierarchy<MergeManager>();
        builder.RegisterComponentInHierarchy<StageManager>();
        builder.RegisterComponentInHierarchy<ScoreDisplayComponent>();
        builder.RegisterComponentInHierarchy<StageEventProcessor>();
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