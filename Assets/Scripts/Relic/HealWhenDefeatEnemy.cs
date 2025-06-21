using System;

/// <summary>
/// 敵撃破時にHPを回復するレリック
/// </summary>
public class HealWhenDefeatEnemy : RelicBase
{
    public override void RegisterEffects()
    {
        // 敵撃破時のイベント購読
        AddSubscription(RelicHelpers.SubscribeEnemyDefeated(OnEnemyDefeated));
    }

    private void OnEnemyDefeated(EnemyBase enemy)
    {
        if (enemy == null) return;
        
        var heal = enemy.MaxHealth * 0.1f;
        GameManager.Instance?.Player?.Heal((int)Math.Ceiling(heal));
        UI?.ActivateUI();
    }
}
