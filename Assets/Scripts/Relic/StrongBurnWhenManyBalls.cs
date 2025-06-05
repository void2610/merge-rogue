using R3;
using UnityEngine;

/// <summary>
/// ボールが10個以上のとき、敌Burn効果発動時に次の敌にもBurnを付与するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class StrongBurnWhenManyBalls : RelicBase
{
    protected override void RegisterEffects()
    {
        // 敌ステータス効果発動時のイベント購読
        var subscription = EventManager.OnEnemyStatusEffectTriggered.Subscribe(OnEnemyStatusEffectTriggered);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnEnemyStatusEffectTriggered(Unit _)
    {
        var effectData = EventManager.OnEnemyStatusEffectTriggered.GetValue();
        if (effectData.Item2 != StatusEffectType.Burn) return;
        if (MergeManager.Instance?.GetBallCount() < 10) return;

        var enemyContainer = EnemyContainer.Instance;
        if (enemyContainer == null) return;
        
        var idx = enemyContainer.GetEnemyIndex(effectData.Item1);
        if (enemyContainer.GetCurrentEnemyCount() < idx + 2) return;
        
        var nextEnemy = enemyContainer.GetAllEnemies()[idx + 1];
        StatusEffectFactory.AddStatusEffect(nextEnemy, StatusEffectType.Burn);
        
        UI?.ActivateUI();
    }
}
