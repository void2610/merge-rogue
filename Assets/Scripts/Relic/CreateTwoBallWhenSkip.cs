using UnityEngine;
using R3;

/// <summary>
/// ボールスキップ時にランダムボールを2個生成するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class CreateTwoBallWhenSkip : RelicBase
{
    protected override void RegisterEffects()
    {
        // ボールスキップ時のイベント購読
        var subscription = EventManager.OnBallSkip.Subscribe(OnBallSkip);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnBallSkip(Unit _)
    {
        MergeManager.Instance?.CreateRandomBall();
        MergeManager.Instance?.CreateRandomBall();
        UI?.ActivateUI();
    }
}
