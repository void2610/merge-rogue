using UnityEngine;

/// <summary>
/// プレイヤーの攻撃時に敵に感電状態異常を付与する
/// </summary>
public class ShockRing : RelicBase
{
    protected override void RegisterEffects()
    {
        // 攻撃時に感電状態異常を付与
        RelicHelpers.RegisterPlayerAttackModifier(this,
            current =>
            {
                var enemies = EnemyContainer.Instance.GetAllEnemies();
                if (enemies.Count > 0)
                {
                    StatusEffects.AddToEntity(enemies[0], StatusEffectType.Shock, 1);
                    ActivateUI();
                }
                return current; // 値は変更しない
            },
            condition: () => EnemyContainer.Instance.GetCurrentEnemyCount() > 0
        );
    }
}