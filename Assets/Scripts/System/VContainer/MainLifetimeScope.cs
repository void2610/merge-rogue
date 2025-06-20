using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// MainScene用のVContainer LifetimeScope
/// 段階的にDI化するため、現在は空（将来的にMainScene専用サービスを追加予定）
/// マウス関連サービスはGlobalLifetimeScopeで管理
/// VContainerSettingsで親子関係を管理
/// </summary>
public class MainLifetimeScope : LifetimeScope
{
    [Header("コンポーネント参照")]
    [SerializeField] private ScoreDisplayComponent scoreDisplayComponent;
    
    
    protected override void Configure(IContainerBuilder builder)
    {
        // マウス関連サービス（シーンごとに再生成）
        builder.Register<IVirtualMouseService, VirtualMouseService>(Lifetime.Scoped);
        builder.Register<IMouseCursorService, MouseCursorService>(Lifetime.Scoped);
        builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
        
        // InventoryServiceの登録（InventoryManagerに依存）
        builder.Register<IInventoryService>(container => new InventoryService(container.Resolve<InventoryManager>()), Lifetime.Singleton);
        
        // RelicServiceの登録
        builder.Register<IRelicService, RelicService>(Lifetime.Singleton);
        
        builder.RegisterEntryPoint<MouseHoverUISelector>(Lifetime.Singleton);
        builder.RegisterComponent(scoreDisplayComponent);
        
        // MainScene関連コンポーネントの依存注入を有効化
        builder.RegisterComponentInHierarchy<GameManager>();
        builder.RegisterComponentInHierarchy<MergeManager>();
        builder.RegisterComponentInHierarchy<StageManager>();
        builder.RegisterComponentInHierarchy<StageEventProcessor>();
        builder.RegisterComponentInHierarchy<DescriptionWindow>();
        builder.RegisterComponentInHierarchy<InventoryManager>();
        builder.RegisterComponentInHierarchy<InventoryUI>();
        builder.RegisterComponentInHierarchy<Treasure>();
        builder.RegisterComponentInHierarchy<Shop>();
        builder.RegisterComponentInHierarchy<AfterBattleUI>();
        builder.RegisterComponentInHierarchy<StatusEffectManager>();
        builder.RegisterComponentInHierarchy<EnemyContainer>();
        builder.RegisterComponentInHierarchy<MapGenerator>();
        builder.RegisterComponentInHierarchy<RelicUIManager>();
        builder.RegisterComponentInHierarchy<Rest>();
    }
}