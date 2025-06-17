using VContainer;
using VContainer.Unity;
using UnityEngine;

public class TitleLifetimeScope : LifetimeScope
{
    [Header("バージョン設定")]
    [SerializeField] private string gameVersion = "0.0.0";
    
    protected override void Configure(IContainerBuilder builder)
    {
        // TitleMenu専用コンポーネント（プレゼンター機能を含む）
        builder.RegisterComponentInHierarchy<TitleMenu>()
            .AsSelf()
            .AsImplementedInterfaces();
            
        // Encyclopediaコンポーネント
        builder.RegisterComponentInHierarchy<Encyclopedia>()
            .AsSelf()
            .AsImplementedInterfaces();
            
        // 純粋なC#サービスを登録
        builder.Register<ICreditService, CreditService>(Lifetime.Singleton);
        builder.Register<ILicenseService, LicenseService>(Lifetime.Singleton);
        
        // 設定されたバージョンでVersionServiceを登録
        builder.Register<IVersionService>(container => new VersionService(gameVersion), Lifetime.Singleton);
    }
}