using VContainer;
using VContainer.Unity;
using UnityEngine;
using SyskenTLib.LicenseMaster;

public class TitleLifetimeScope : LifetimeScope
{
    [SerializeField] private string gameVersion = "0.0.0";
    [SerializeField] private TextAsset creditTextAsset;
    [SerializeField] private LicenseManager licenseManager;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<TitleMenu>()
            .AsSelf()
            .AsImplementedInterfaces();
            
        builder.RegisterComponentInHierarchy<Encyclopedia>()
            .AsSelf()
            .AsImplementedInterfaces();
            
        // 純粋なC#サービスを登録
        builder.Register<ICreditService, CreditService>(Lifetime.Singleton).WithParameter("textAsset", creditTextAsset);
        builder.Register<ILicenseService, LicenseService>(Lifetime.Singleton).WithParameter("licenseManager", licenseManager);
        builder.Register<IVersionService, VersionService>(Lifetime.Singleton).WithParameter("version", gameVersion);
        builder.Register<IGameSettingsService, GameSettingsService>(Lifetime.Singleton);
    }
}