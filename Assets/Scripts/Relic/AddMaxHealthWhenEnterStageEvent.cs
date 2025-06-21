using UnityEngine;
using R3;

/// <summary>
/// イベントステージ入場時に最大HPを増加するレリック
/// </summary>
public class AddMaxHealthWhenEnterStageEvent : RelicBase
{
    public override void RegisterEffects()
    {
        // イベントステージ入場時のイベント購読
        var subscription = EventManager.OnEventStageEnter.Subscribe(OnEventStageEnter);
        SimpleSubscriptions.Add(subscription);
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
