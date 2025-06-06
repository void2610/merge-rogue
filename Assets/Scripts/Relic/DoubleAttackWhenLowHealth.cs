using UnityEngine;

/// <summary>
/// HPが20%以下または20以下の時、攻撃力を2倍にする
/// </summary>
public class DoubleAttackWhenLowHealth : RelicBase
{
    protected override void RegisterEffects()
    {
        // 低HP時に攻撃力2倍
        RelicHelpers.RegisterAttackMultiplier(this, 2.0f, 
            condition: () => IsLowHealth());
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