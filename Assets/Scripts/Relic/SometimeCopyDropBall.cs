using UnityEngine;
using R3;

/// <summary>
/// ボールドロップ時に50%の確率で同じボールを複製するレリック
/// </summary>
public class SometimeCopyDropBall : RelicBase
{
    public override void RegisterEffects()
    {
        // ボールドロップ時のイベント購読
        var subscription = EventManager.OnBallDrop.Subscribe(OnBallDrop);
        SimpleSubscriptions.Add(subscription);
    }
    
    private void OnBallDrop(Unit _)
    {
        if (RandomService.RandomRange(0.0f, 1.0f) < 0.5f)
        {
            var currentBall = MergeManager.Instance?.CurrentBall?.GetComponent<BallBase>();
            if (currentBall)
            {
                var level = currentBall.Rank;
                var p = new Vector3(RandomService?.RandomRange(-1f, 1f) ?? 0f, 0.8f, 0);
                MergeManager.Instance.SpawnBallFromLevel(level, p, Quaternion.identity);
                UI?.ActivateUI();
            }
        }
    }
}
