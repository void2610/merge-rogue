using VContainer;
using VContainer.Unity;
using UnityEngine;

/// <summary>
/// 全体で共有するサービスを管理するGlobalLifetimeScope
/// マウス関連サービスなど、TitleとMainで共通利用するサービスを登録
/// VContainerSettingsのRoot LifetimeScopeとして使用
/// </summary>
public class RootLifetimeScope : LifetimeScope
{
    [Header("カーソル設定")]
    [SerializeField] private CursorConfiguration cursorConfiguration;
    
    [Header("コンテンツ設定")]
    [SerializeField] private ContentProviderData contentProviderData;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(cursorConfiguration);
        builder.Register<IInputProvider, InputProviderService>(Lifetime.Singleton);
        builder.Register<IGameSettingsService, GameSettingsService>(Lifetime.Singleton);
        builder.Register<IEnemyDifficultyService, EnemyDifficultyService>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<BgmManager>();
        builder.RegisterComponentInHierarchy<SeManager>();
        
        // ContentService関連の登録
        RegisterContentServices(builder);
        // SetMouseCursorコンポーネントの依存性注入（全シーン共通）
        RegisterSetMouseCursorComponents(builder);
        // StatusEffectUIコンポーネントの依存性注入（全シーン共通）
        RegisterStatusEffectUIComponents(builder);
        
        // 解決が終わったらDontDestroyOnLoadを適用
        DontDestroyOnLoad(this.gameObject);
    }
    
    /// <summary>
    /// ContentService関連の共通サービス登録
    /// </summary>
    private void RegisterContentServices(IContainerBuilder builder)
    {
        builder.RegisterInstance(contentProviderData);
        
        // 共通して使用するサービス
        builder.Register<IRandomService, RandomService>(Lifetime.Singleton);
        
        // ContentServiceの登録（シーン固有の依存関係は各シーンで解決）
        builder.Register<IContentService>(container =>
        {
            var data = container.Resolve<ContentProviderData>();
            var randomService = container.Resolve<IRandomService>();
            return new ContentService(data, randomService);
        }, Lifetime.Singleton);
    }
    
    /// <summary>
    /// SetMouseCursorコンポーネントの依存性注入設定（全シーン共通）
    /// </summary>
    private void RegisterSetMouseCursorComponents(IContainerBuilder builder)
    {
        // BuildCallbackを使用して手動で依存性を注入
        builder.RegisterBuildCallback(container =>
        {
            var setMouseCursors = FindObjectsByType<SetMouseCursor>(FindObjectsSortMode.None);

            if (!container.TryResolve<IMouseCursorService>(out var mouseCursorService)) return;
            foreach (var setMouseCursor in setMouseCursors)
                setMouseCursor.InjectDependencies(mouseCursorService);
        });
    }
    
    /// <summary>
    /// StatusEffectUIコンポーネントの依存性注入設定（全シーン共通）
    /// </summary>
    private void RegisterStatusEffectUIComponents(IContainerBuilder builder)
    {
        builder.RegisterBuildCallback(container =>
        {
            var statusEffectUIs = FindObjectsByType<StatusEffectUI>(FindObjectsSortMode.None);

            if (!container.TryResolve<IContentService>(out var contentService)) return;
            foreach (var statusEffectUI in statusEffectUIs)
                statusEffectUI.InjectDependencies(contentService);
        });
    }
}