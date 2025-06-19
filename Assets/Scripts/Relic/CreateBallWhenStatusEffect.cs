using UnityEngine;
using R3;

/// <summary>
/// プレイヤーにステータス効果が追加されたときにランダムボールを生成するレリック
/// </summary>
public class CreateBallWhenStatusEffect : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーステータス効果追加時のイベント購読
        var subscription = EventManager.OnPlayerStatusEffectAdded.Subscribe(OnStatusEffectAdded);
        SimpleSubscriptions.Add(subscription);
    }

    private void OnStatusEffectAdded(Unit _)
    {
        MergeManager.Instance?.CreateRandomBall();
        UI?.ActivateUI();
    }
}
