using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// プレイヤーにステータス効果が追加されたときにHPを回復するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class HealWhenStatusEffect : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーステータス効果追加時のイベント購読
        var subscription = SafeEventManager.OnPlayerStatusEffectAdded.OnProcessed.Subscribe(OnStatusEffectAdded);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnStatusEffectAdded(((StatusEffectType type, int stack) original, (StatusEffectType type, int stack) modified) data)
    {
        GameManager.Instance?.Player?.Heal(1);
        UI?.ActivateUI();
    }
}
