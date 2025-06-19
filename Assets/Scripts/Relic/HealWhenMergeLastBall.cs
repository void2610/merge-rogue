using UnityEngine;
using R3;

/// <summary>
/// 最高ランクボール合成時にHPを回復するレリック
/// </summary>
public class HealWhenMergeLastBall : RelicBase
{
    protected override void RegisterEffects()
    {
        // ボール合成時のイベント購読
        var subscription = EventManager.OnBallMerged.Subscribe(OnBallMerged);
        SimpleSubscriptions.Add(subscription);
    }
    
    private void OnBallMerged((BallBase ball1, BallBase ball2) mergeData)
    {
        if (mergeData.ball1?.Rank != InventoryService.InventorySize) return;
        
        if (GameManager.Instance?.Player)
        {
            var heal = GameManager.Instance.Player.MaxHealth.Value / 4;
            GameManager.Instance.Player.Heal(heal);
        }
        UI?.ActivateUI();
    }
}
