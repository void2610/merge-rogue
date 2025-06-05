using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// イベントステージ入場時に最大HPを増加するレリック
/// </summary>
public class AddMaxHealthWhenEnterStageEvent : RelicBase
{
    protected override void RegisterEffects()
    {
        // イベントステージ入場時のイベント購読
        var subscription = SafeEventManager.OnEventStageEnter.Subscribe(OnEventStageEnter);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnEventStageEnter(StageType stageType)
    {
        if (GameManager.Instance?.Player != null)
        {
            GameManager.Instance.Player.MaxHealth.Value += 10;
        }
        UI?.ActivateUI();
    }
}
