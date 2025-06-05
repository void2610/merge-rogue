using UnityEngine;

/// <summary>
/// プレイヤーの攻撃時に敵に感電状態異常を付与する
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class ShockRing : RelicBase
{
    protected override void RegisterEffects()
    {
        // 攻撃時に感電状態異常を付与
        RegisterPlayerAttackModifier(
            SafeEventSystem.ModificationPhase.PostProcess, // 攻撃処理後に状態異常付与
            (original, current) =>
            {
                var enemies = EnemyContainer.Instance.GetAllEnemies();
                if (enemies.Count > 0)
                {
                    StatusEffectFactory.AddStatusEffect(enemies[0], StatusEffectType.Shock, 1);
                    ActivateUI();
                }
                return current; // 値は変更しない
            },
            condition: () => EnemyContainer.Instance.GetCurrentEnemyCount() > 0
        );
    }
}