using UnityEngine;
using R3;

/// <summary>
/// シールド発動時に敵に攻撃するレリック
/// </summary>
public class AttackWhenShield : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーのステータス効果発動時のイベント購読
        var subscription = EventManager.OnPlayerStatusEffectTriggered.Subscribe(OnStatusEffectTriggered);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnStatusEffectTriggered(StatusEffectType statusEffectType)
    {
        // Shield状態異常が発動した場合のみ効果発動
        if (statusEffectType == StatusEffectType.Shield)
        {
            var enemies = EnemyContainer.Instance?.GetAllEnemies();
            if (enemies != null && enemies.Count > 0)
            {
                enemies[0].Damage(AttackType.Normal, 3); // 固定ダメージ
                UI?.ActivateUI();
            }
        }
    }
}
