using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// シールド発動時に敵に攻撃するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class AttackWhenShield : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーのステータス効果発動時のイベント購読
        var subscription = SafeEventManager.OnPlayerStatusEffectTriggered.OnProcessed.Subscribe(OnStatusEffectTriggered);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnStatusEffectTriggered(((StatusEffectType type, int stack) original, (StatusEffectType type, int stack) modified) data)
    {
        if (data.modified.type == StatusEffectType.Shield)
        {
            var enemies = EnemyContainer.Instance?.GetAllEnemies();
            if (enemies != null && enemies.Count > 0)
            {
                enemies[0].Damage(AttackType.Normal, data.modified.stack);
                UI?.ActivateUI();
            }
        }
    }
}
