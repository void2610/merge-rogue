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
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<Encyclopedia>()
            .AsSelf()
            .AsImplementedInterfaces();
            
        // 純粋なC#サービスを登録
        builder.Register<ICreditService, CreditService>(Lifetime.Singleton).WithParameter("textAsset", creditTextAsset);
        builder.Register<ILicenseService, LicenseService>(Lifetime.Singleton).WithParameter("licenseManager", licenseManager);
        builder.Register<IVersionService, VersionService>(Lifetime.Singleton).WithParameter("version", gameVersion);
        
        // UI関連サービス（エントリーポイント）
        builder.RegisterEntryPoint<MouseHoverUISelector>();
        builder.RegisterEntryPoint<SettingsPresenter>();
        
        // マウス関連サービス（シーンごとに再生成）
        builder.Register<IVirtualMouseService, VirtualMouseService>(Lifetime.Scoped);
        builder.Register<IMouseCursorService, MouseCursorService>(Lifetime.Scoped);
        
        // DescriptionWindowのVContainer登録（InputProviderServiceの注入を有効化）
        builder.RegisterComponentInHierarchy<DescriptionWindow>();
        builder.RegisterComponentInHierarchy<TitlePresenter>();
    }
}