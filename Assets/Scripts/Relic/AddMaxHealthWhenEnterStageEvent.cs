using UnityEngine;
using R3;

/// <summary>
/// イベントステージ入場時に最大HPを増加するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class AddMaxHealthWhenEnterStageEvent : RelicBase
{
    protected override void RegisterEffects()
    {
        // イベントステージ入場時のイベント購読
        var subscription = EventManager.OnEventStageEnter.Subscribe(OnEventStageEnter);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnEventStageEnter(Unit _)
    {
        if (GameManager.Instance?.Player != null)
        {
            GameManager.Instance.Player.MaxHealth.Value += 10;
        }
        UI?.ActivateUI();
    }
}
