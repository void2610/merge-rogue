using VContainer;
using VContainer.Unity;
using UnityEngine;
using SyskenTLib.LicenseMaster;

public class TitleLifetimeScope : LifetimeScope
{
    [Header("バージョン設定")]
    [SerializeField] private string gameVersion = "0.0.0";
    
    [Header("コンテンツ設定")]
    [SerializeField] private TextAsset creditTextAsset;
    [SerializeField] private LicenseManager licenseManager;
    
    [Header("カーソル設定")]
    [SerializeField] private CursorConfiguration cursorConfiguration;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<TitleMenu>()
            .AsSelf()
            .AsImplementedInterfaces();
            
        builder.RegisterComponentInHierarchy<Encyclopedia>()
            .AsSelf()
            .AsImplementedInterfaces();
            
        // InputProvider登録
        builder.RegisterComponentInHierarchy<InputProvider>()
            .AsSelf()
            .AsImplementedInterfaces();
            
        // 純粋なC#サービスを登録
        builder.Register<ICreditService, CreditService>(Lifetime.Singleton).WithParameter("textAsset", creditTextAsset);
        builder.Register<ILicenseService, LicenseService>(Lifetime.Singleton).WithParameter("licenseManager", licenseManager);
        builder.Register<IVersionService, VersionService>(Lifetime.Singleton).WithParameter("version", gameVersion);
        builder.Register<IGameSettingsService, GameSettingsService>(Lifetime.Singleton);
        
        // カーソル・仮想マウス関連サービスを登録
        builder.RegisterInstance(cursorConfiguration);
        builder.Register<IVirtualMouseService, VirtualMouseService>(Lifetime.Singleton);
        builder.Register<IMouseCursorService, MouseCursorService>(Lifetime.Singleton);
        
        // SetMouseCursorコンポーネントを依存性注入対象に登録
        // 複数のSetMouseCursorコンポーネントに対応するため、
        // BuildCallbackを使用して手動で依存性を注入
        builder.RegisterBuildCallback(container =>
        {
            var setMouseCursors = FindObjectsByType<SetMouseCursor>(FindObjectsSortMode.None);
            var mouseCursorService = container.Resolve<IMouseCursorService>();
            
            foreach (var setMouseCursor in setMouseCursors)
            {
                setMouseCursor.InjectDependencies(mouseCursorService);
            }
        });
    }
}