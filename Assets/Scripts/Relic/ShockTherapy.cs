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

    private void OnEnemyStatusEffectAdded(Unit _)
    {
        // 敵にShockが付与されたかどうかをチェック
        if (HasShockBeenAddedToAnyEnemy())
        {
            StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Power, 1);
            UI?.ActivateUI();
        }
    }
    
    /// <summary>
    /// いずれかの敵にShock状態異常が付与されているかチェック
    /// </summary>
    private bool HasShockBeenAddedToAnyEnemy()
    {
        var enemies = EnemyContainer.Instance?.GetAllEnemies();
        if (enemies == null) return false;
        
        foreach (var enemy in enemies)
        {
            var shockEffect = enemy.StatusEffects.Find(e => e.Type == StatusEffectType.Shock);
            if (shockEffect != null)
            {
                return true;
            }
        }
        return false;
    }
}
