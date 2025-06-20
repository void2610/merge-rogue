using R3;
using UnityEngine;

/// <summary>
/// ボールが10個以上のとき、敵Burn効果発動時に次の敵にもBurnを付与するレリック
/// </summary>
public class StrongBurnWhenManyBalls : RelicBase
{
    public override void RegisterEffects()
    {
        // 敵ステータス効果発動時のイベント購読
        var subscription = EventManager.OnEnemyStatusEffectTriggered.Subscribe(OnEnemyStatusEffectTriggered);
        SimpleSubscriptions.Add(subscription);
    }

    private void OnEnemyStatusEffectTriggered(StatusEffectType statusEffectType)
    {
        // Burn状態異常が発動した場合のみ効果発動
        if (statusEffectType != StatusEffectType.Burn) return;
        
        // ボールが10個以上の場合のみ効果発動
        if (MergeManager.Instance?.GetBallCount() < 10) return;

        var enemyContainer = EnemyContainer.Instance;
        if (enemyContainer == null) return;
        
        var enemies = enemyContainer.GetAllEnemies();
        if (enemies.Count <= 1) return;
        
        // 次の敵（インデックス1）にBurnを付与
        // 敵が複数いる場合は最初の敵以外にBurnを付与
        for (int i = 1; i < enemies.Count; i++)
        {
            StatusEffects.AddToEntity(enemies[i], StatusEffectType.Burn, 1);
        }
        
        UI?.ActivateUI();
    }
}
