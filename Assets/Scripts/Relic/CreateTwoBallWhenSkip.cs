using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// ボールスキップ時にランダムボールを2個生成するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class CreateTwoBallWhenSkip : RelicBase
{
    protected override void RegisterEffects()
    {
        // ボールスキップ時のイベント購読
        var subscription = SafeEventManager.OnBallSkip.OnProcessed.Subscribe(OnBallSkip);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnBallSkip((int original, int modified) data)
    {
        MergeManager.Instance?.CreateRandomBall();
        MergeManager.Instance?.CreateRandomBall();
        UI?.ActivateUI();
    }
}
