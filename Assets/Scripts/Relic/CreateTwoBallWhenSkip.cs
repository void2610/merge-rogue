using UnityEngine;
using R3;

/// <summary>
/// ボールスキップ時にランダムボールを2個生成するレリック
/// </summary>
public class CreateTwoBallWhenSkip : RelicBase
{
    public override void RegisterEffects()
    {
        // ボールスキップ時のイベント購読
        var subscription = EventManager.OnBallSkip.Subscribe(OnBallSkip);
        SimpleSubscriptions.Add(subscription);
    }

    private void OnBallSkip(Unit _)
    {
        MergeManager.Instance?.CreateRandomBall();
        MergeManager.Instance?.CreateRandomBall();
        UI?.ActivateUI();
    }
}
