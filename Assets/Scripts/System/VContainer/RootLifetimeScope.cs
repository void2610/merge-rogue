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
    
    protected override void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        base.Awake();
    }
    
    protected override void Configure(IContainerBuilder builder)
    {
        // 共通サービスの登録
        RegisterSharedServices(builder);
        
        // ContentService関連の登録
        RegisterContentServices(builder);
        
        // SetMouseCursorコンポーネントの依存性注入（全シーン共通）
        RegisterSetMouseCursorComponents(builder);
    }
    
    /// <summary>
    /// 全シーンで共有するサービスの登録
    /// </summary>
    private void RegisterSharedServices(IContainerBuilder builder)
    {
        builder.RegisterInstance(cursorConfiguration);
        builder.Register<IInputProvider, InputProviderService>(Lifetime.Singleton);
        // MouseCursorServiceは各シーンのLifetimeScopeで登録（VirtualMouseServiceとの依存関係のため）
    }
    
    /// <summary>
    /// ContentService関連の共通サービス登録
    /// </summary>
    private void RegisterContentServices(IContainerBuilder builder)
    {
        // ContentProviderDataのnullチェックと登録
        if (contentProviderData == null)
        {
            Debug.LogError("ContentProviderDataがRootLifetimeScopeで設定されていません。Inspector内で設定してください。");
            return;
        }
        
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
        // SetMouseCursorコンポーネントを依存性注入対象に登録
        // 複数のSetMouseCursorコンポーネントに対応するため、
        // BuildCallbackを使用して手動で依存性を注入
        builder.RegisterBuildCallback(container =>
        {
            var setMouseCursors = FindObjectsByType<SetMouseCursor>(FindObjectsSortMode.None);
            
            // MouseCursorServiceが登録されているかチェック
            if (container.TryResolve<IMouseCursorService>(out var mouseCursorService))
            {
                foreach (var setMouseCursor in setMouseCursors)
                {
                    setMouseCursor.InjectDependencies(mouseCursorService);
                }
            }
        });
    }
    
}