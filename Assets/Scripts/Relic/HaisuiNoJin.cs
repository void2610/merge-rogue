using UnityEngine;

/// <summary>
/// 最大HPを1/4に削減するが、敵撃破時に無敵状態異常を付与する
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class HaisuiNoJin : RelicBase
{
    public override void Init(RelicUI relicUI)
    {
        // 初期化時に最大HPを削減
        ModifyMaxHealth();
        
        base.Init(relicUI);
    }

    protected override void RegisterEffects()
    {
        // 敵撃破時に無敵状態異常を付与
        SubscribeEnemyDefeated(enemy =>
        {
            StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Invincible, 1);
            ActivateUI();
        });
    }

    /// <summary>
    /// 最大HPを1/4に削減
    /// </summary>
    private void ModifyMaxHealth()
    {
        if (!GameManager.Instance?.Player) return;

        var currentMaxHealth = GameManager.Instance.Player.MaxHealth.Value;
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