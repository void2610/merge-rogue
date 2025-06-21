using UnityEngine;
using R3;

/// <summary>
/// プレイヤーにステータス効果が追加されたときにHPを回復するレリック
/// </summary>
public class HealWhenStatusEffect : RelicBase
{
    public override void RegisterEffects()
    {
        // プレイヤーステータス効果追加時のイベント購読
        var subscription = EventManager.OnPlayerStatusEffectAdded.Subscribe(OnStatusEffectAdded);
        SimpleSubscriptions.Add(subscription);
    }

    private void OnStatusEffectAdded(Unit _)
    {
        GameManager.Instance?.Player?.Heal(1);
        UI?.ActivateUI();
    }
}
