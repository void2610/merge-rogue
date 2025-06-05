using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// 宝物スキップ時に最大HPを増加するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class AddMaxHealthWhenSkipTreasure : RelicBase
{
    protected override void RegisterEffects()
    {
        // 宝物スキップ時のイベント購読
        var subscription = SafeEventManager.OnTreasureSkipped.OnProcessed.Subscribe(OnTreasureSkipped);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnTreasureSkipped((int original, int modified) data)
    {
        if (GameManager.Instance?.Player != null)
        {
            GameManager.Instance.Player.MaxHealth.Value += 10;
        }
        UI?.ActivateUI();
    }
}
