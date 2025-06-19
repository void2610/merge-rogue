using UnityEngine;
using R3;

/// <summary>
/// ボールドロップ時に50%の確率で同じボールを複製するレリック
/// </summary>
public class SometimeCopyDropBall : RelicBase
{
    protected override void RegisterEffects()
    {
        // ボールドロップ時のイベント購読
        var subscription = EventManager.OnBallDrop.Subscribe(OnBallDrop);
        SimpleSubscriptions.Add(subscription);
    }
    
    private void OnBallDrop(Unit _)
    {
        var r = GameManager.Instance?.RandomRange(0.0f, 1.0f) ?? 0f;
        if (r < 0.5f)
        {
            var currentBall = MergeManager.Instance?.CurrentBall?.GetComponent<BallBase>();
            if (currentBall != null)
            {
                var level = currentBall.Rank;
                var p = new Vector3(GameManager.Instance.RandomRange(-1f, 1f), 0.8f, 0);
                MergeManager.Instance.SpawnBallFromLevel(level, p, Quaternion.identity);
                UI?.ActivateUI();
            }
        }
    }
}
