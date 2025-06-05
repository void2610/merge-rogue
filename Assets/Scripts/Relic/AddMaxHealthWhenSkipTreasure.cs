using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// 宝物スキップ時に最大HPを増加するレリック
/// </summary>
public class AddMaxHealthWhenSkipTreasure : RelicBase
{
    protected override void RegisterEffects()
    {
        // 宝物スキップ時のイベント購読
        var subscription = SafeEventManager.OnTreasureSkipped.Subscribe(OnTreasureSkipped);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnTreasureSkipped(Unit _)
    {
        if (GameManager.Instance?.Player != null)
        {
            GameManager.Instance.Player.MaxHealth.Value += 10;
        }
        UI?.ActivateUI();
    }
}
