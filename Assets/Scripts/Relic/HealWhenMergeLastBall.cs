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
        _simpleSubscriptions.Add(subscription);
    }
    
    private void OnBallMerged((BallBase ball1, BallBase ball2) mergeData)
    {
        var maxRank = InventoryManager.Instance?.InventorySize ?? 0;
        
        if (mergeData.ball1?.Rank == maxRank)
        {
            if (GameManager.Instance?.Player != null)
            {
                int heal = GameManager.Instance.Player.MaxHealth.Value / 4;
                GameManager.Instance.Player.Heal(heal);
            }
            UI?.ActivateUI();
        }
    }
}
