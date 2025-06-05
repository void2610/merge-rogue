using R3;
using UnityEngine;
using SafeEventSystem;

/// <summary>
/// ボールが10個以上のとき、敌Burn効果発動時に次の敌にもBurnを付与するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class StrongBurnWhenManyBalls : RelicBase
{
    protected override void RegisterEffects()
    {
        // 敌ステータス効果発動時のイベント購読
        var subscription = SafeEventManager.OnEnemyStatusEffectTriggered.Subscribe(OnEnemyStatusEffectTriggered);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnEnemyStatusEffectTriggered(Unit _)
    {
        // 簡略化版では詳細な情報を取得できないため、ボール数と敵の状態のみチェック
        if (MergeManager.Instance?.GetBallCount() < 10) return;

        var enemyContainer = EnemyContainer.Instance;
        if (enemyContainer == null) return;
        
        // 最初の敵にバーンを付与（簡略化版）
        var enemies = enemyContainer.GetAllEnemies();
        if (enemies.Count > 1)
        {
            StatusEffectFactory.AddStatusEffect(enemies[1], StatusEffectType.Burn);
        }
        
        UI?.ActivateUI();
    }
}
