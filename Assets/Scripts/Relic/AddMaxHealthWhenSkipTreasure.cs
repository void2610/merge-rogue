using UnityEngine;
using R3;

/// <summary>
/// 宝物スキップ時に最大HPを増加するレリック
/// </summary>
public class AddMaxHealthWhenSkipTreasure : RelicBase
{
    public override void RegisterEffects()
    {
        // 宝物スキップ時のイベント購読
        var subscription = EventManager.OnTreasureSkipped.Subscribe(OnTreasureSkipped);
        SimpleSubscriptions.Add(subscription);
    }

    private void OnTreasureSkipped(Unit _)
    {
        if (GameManager.Instance?.Player)
            GameManager.Instance.Player.MaxHealth.Value += 10;
        UI?.ActivateUI();
    }
}
