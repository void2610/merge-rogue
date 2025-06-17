using UnityEngine;
using R3;

/// <summary>
/// 敌にShock付与時にプレイヤーにPowerを付与するレリック
/// </summary>
public class ShockTherapy : RelicBase
{
    protected override void RegisterEffects()
    {
        // 敵ステータス効果追加時のイベント購読
        var subscription = EventManager.OnEnemyStatusEffectAdded.Subscribe(OnEnemyStatusEffectAdded);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnEnemyStatusEffectAdded(StatusEffectType statusEffectType)
    {
        // Shock状態異常が付与された場合のみ効果発動
        if (statusEffectType == StatusEffectType.Shock)
        {
            StatusEffects.AddToPlayer(StatusEffectType.Power, 1);
            UI?.ActivateUI();
        }
    }
}
