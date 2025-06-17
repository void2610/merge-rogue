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
    protected override void Configure(IContainerBuilder builder)
    {
        // TODO: 段階的にMainScene専用のサービスを追加
        // 例: GameManager → IGameService
        // 例: MergeManager → IMergeService
        // 例: InventoryManager → IInventoryService
    }
}