using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// ボールスキップ時にランダムボールを2個生成するレリック
/// </summary>
public class CreateTwoBallWhenSkip : RelicBase
{
    protected override void RegisterEffects()
    {
        // ボールスキップ時のイベント購読
        var subscription = SafeEventManager.OnBallSkip.Subscribe(OnBallSkip);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnBallSkip(Unit _)
    {
        MergeManager.Instance?.CreateRandomBall();
        MergeManager.Instance?.CreateRandomBall();
        UI?.ActivateUI();
    }
}
