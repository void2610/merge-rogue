using UnityEngine;

/// <summary>
/// HPが20%以下または20以下の時、バトル開始時に怒り状態異常を10スタック付与する
/// </summary>
public class DoubleAttackWhenLowHealth : RelicBase
{
    public override void RegisterEffects()
    {
        // バトル開始時に低HP条件をチェックして怒り状態異常を付与
        AddSubscription(RelicHelpers.SubscribeBattleStart(() =>
        {
            if (IsLowHealth())
            {
                StatusEffects.AddToPlayer(StatusEffectType.Rage, 10);
                ActivateUI();
            }
        }));
    }

    /// <summary>
    /// 低HP条件をチェック
    /// </summary>
    private bool IsLowHealth()
    {
        if (!GameManager.Instance?.Player) return false;
        
        var currentHealth = GameManager.Instance.Player.Health.Value;
        var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
        
        // 最大HPの20%以下 または 絶対値20以下
        return currentHealth <= maxHealth * 0.2f || currentHealth <= 20;
    }
}