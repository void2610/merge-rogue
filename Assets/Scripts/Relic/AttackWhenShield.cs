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

    private void OnStatusEffectTriggered(Unit _)
    {
        // シールド効果が発動したかどうかをチェック
        if (HasShieldEffect())
        {
            var enemies = EnemyContainer.Instance?.GetAllEnemies();
            if (enemies != null && enemies.Count > 0)
            {
                enemies[0].Damage(AttackType.Normal, 3); // 固定ダメージ
                UI?.ActivateUI();
            }
        }
    }
    
    /// <summary>
    /// プレイヤーがシールド状態異常を持っているかチェック
    /// </summary>
    private bool HasShieldEffect()
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return false;
        
        var shieldEffect = player.StatusEffects.Find(e => e.Type == StatusEffectType.Shield);
        return shieldEffect != null;
    }
}
