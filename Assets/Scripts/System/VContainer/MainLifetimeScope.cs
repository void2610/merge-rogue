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
    
    [Header("ContentService設定データ")]
    [SerializeField] private ContentProviderData contentProviderData;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // マウス関連サービス（シーンごとに再生成）
        builder.Register<IVirtualMouseService, VirtualMouseService>(Lifetime.Scoped);
        builder.Register<IMouseCursorService, MouseCursorService>(Lifetime.Scoped);
        
        // スコア関連サービス
        builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
        
        // ContentService関連の依存関係を登録
        builder.Register<IRandomService, RandomService>(Lifetime.Singleton);
        
        // StageEventFactoryの登録（ContentProviderのGameObjectを使用）
        builder.Register<IStageEventFactory>(container =>
        {
            var contentProvider = ContentProvider.Instance;
            if (contentProvider == null)
            {
                throw new System.InvalidOperationException("ContentProvider instance is required for StageEventFactory");
            }
            return new StageEventFactory(contentProvider.gameObject);
        }, Lifetime.Singleton);
        
        // ContentServiceの登録（完全なpure C#実装）
        builder.Register<IContentService>(container =>
        {
            var data = contentProviderData;
            var stageEventFactory = container.Resolve<IStageEventFactory>();
            var randomService = container.Resolve<IRandomService>();
            
            return new ContentService(data, stageEventFactory, randomService);
        }, Lifetime.Singleton);
        
        // UI関連サービス（エントリーポイント）
        builder.RegisterEntryPoint<MouseHoverUISelector>(Lifetime.Singleton);
        
        builder.RegisterComponent(scoreDisplayComponent);
        
        // MainScene関連コンポーネントの依存注入を有効化
        builder.RegisterComponentInHierarchy<GameManager>();
        builder.RegisterComponentInHierarchy<MergeManager>();
        builder.RegisterComponentInHierarchy<StageEventProcessor>();
        builder.RegisterComponentInHierarchy<DescriptionWindow>();
        builder.RegisterComponentInHierarchy<StatusEffectUI>();
        builder.RegisterComponentInHierarchy<Treasure>();
        builder.RegisterComponentInHierarchy<Shop>();
        builder.RegisterComponentInHierarchy<AfterBattleUI>();
        
        // TODO: 段階的にMainScene専用のサービスを追加
        // 例: GameManager → IGameService
        // 例: MergeManager → IMergeService
        // 例: InventoryManager → IInventoryService
    }
}