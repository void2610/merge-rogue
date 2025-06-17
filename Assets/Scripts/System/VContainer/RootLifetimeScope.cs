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
    
    protected override void Configure(IContainerBuilder builder)
    {
        // 共通サービスの登録
        RegisterSharedServices(builder);
        
        // SetMouseCursorコンポーネントの依存性注入（全シーン共通）
        RegisterSetMouseCursorComponents(builder);
    }
    
    /// <summary>
    /// 全シーンで共有するサービスの登録
    /// </summary>
    private void RegisterSharedServices(IContainerBuilder builder)
    {
        // カーソル設定のインスタンス登録
        if (cursorConfiguration != null)
        {
            builder.RegisterInstance(cursorConfiguration);
        }
        
        // 入力プロバイダーの登録（全シーン共通）
        builder.Register<IInputProvider, InputProviderService>(Lifetime.Singleton);
        
        // 仮想マウスサービスの登録（全シーン共通）
        builder.Register<IVirtualMouseService, VirtualMouseService>(Lifetime.Singleton);
        
        // マウスカーソルサービスの登録（全シーン共通）
        builder.Register<IMouseCursorService, MouseCursorService>(Lifetime.Singleton);
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