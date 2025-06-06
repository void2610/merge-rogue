using UnityEngine;

/// <summary>
/// 最大HPを1/4に削減するが、敵撃破時に無敵状態異常を付与する
/// </summary>
public class HaisuiNoJin : RelicBase
{
    private int _originalMaxHealth;

    protected override void RegisterEffects()
    {
        // 最大HPを1/4に削減
        ModifyMaxHealth();
        
        // 敵撃破時に無敵状態異常を付与
        SubscribeEnemyDefeated(enemy =>
        {
            StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Invincible, 1);
            ActivateUI();
        });
    }

    public override void RemoveAllEffects()
    {
        base.RemoveAllEffects();
        
        // 最大HPを元に戻す
        if (_originalMaxHealth > 0 && GameManager.Instance?.Player != null)
        {
            GameManager.Instance.Player.MaxHealth.Value = _originalMaxHealth;
        }
    }

    /// <summary>
    /// 最大HPを1/4に削減
    /// </summary>
    private void ModifyMaxHealth()
    {
        if (!GameManager.Instance?.Player) return;

        var currentMaxHealth = GameManager.Instance.Player.MaxHealth.Value;
        _originalMaxHealth = currentMaxHealth; // 元の値を保存
        var newMaxHealth = currentMaxHealth / 4;
        
        GameManager.Instance.Player.MaxHealth.Value = newMaxHealth;
        
        // 現在のHPが新しい最大HPを超えている場合は調整
        if (GameManager.Instance.Player.Health.Value > newMaxHealth)
        {
            GameManager.Instance.Player.Health.Value = newMaxHealth;
        }
        
        Debug.Log($"[HaisuiNoJin] MaxHealth reduced: {currentMaxHealth} → {newMaxHealth}");
    }
}