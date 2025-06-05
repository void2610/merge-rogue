using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// プレイヤーにステータス効果が追加されたときにHPを回復するレリック
/// </summary>
public class HealWhenStatusEffect : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーステータス効果追加時のイベント購読
        var subscription = SafeEventManager.OnPlayerStatusEffectAdded.Subscribe(OnStatusEffectAdded);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnStatusEffectAdded(Unit _)
    {
        GameManager.Instance?.Player?.Heal(1);
        UI?.ActivateUI();
    }
}
