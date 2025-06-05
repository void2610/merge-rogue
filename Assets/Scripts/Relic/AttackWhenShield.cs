using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// シールド発動時に敵に攻撃するレリック
/// </summary>
public class AttackWhenShield : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーのステータス効果発動時のイベント購読
        var subscription = SafeEventManager.OnPlayerStatusEffectTriggered.Subscribe(OnStatusEffectTriggered);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnStatusEffectTriggered(Unit _)
    {
        // シールド効果の具体的な検出は他の手段で行う
        var enemies = EnemyContainer.Instance?.GetAllEnemies();
        if (enemies != null && enemies.Count > 0)
        {
            enemies[0].Damage(AttackType.Normal, 5); // 固定ダメージ
            UI?.ActivateUI();
        }
    }
}
