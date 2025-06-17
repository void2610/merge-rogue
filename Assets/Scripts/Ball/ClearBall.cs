using System;

public class ClearBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);

        // プレイヤーに付与された全ての状態異常を5スタック減少させる
        if (Level == 0)
        {
            foreach(StatusEffectType effect in Enum.GetValues(typeof(StatusEffectType)))
            {
                StatusEffects.RemoveFromPlayer(effect, 5);
            }
        }
        // 全ての敵に付与された全ての状態異常を5スタック減少させる
        else if (Level == 1)
        {
            foreach(StatusEffectType effect in Enum.GetValues(typeof(StatusEffectType)))
            {
                foreach(var enemy in EnemyContainer.Instance.GetAllEnemies())
                {
                    StatusEffects.RemoveFromEntity(enemy, effect, 5);
                }
            }
        }
        // プレイヤーと全ての敵に付与された全ての状態異常を5スタック減少させる
        else if (Level == 2)
        {
            foreach(StatusEffectType effect in Enum.GetValues(typeof(StatusEffectType)))
            {
                StatusEffects.RemoveFromPlayer(effect, 5);
                foreach(var enemy in EnemyContainer.Instance.GetAllEnemies())
                {
                    StatusEffects.RemoveFromEntity(enemy, effect, 5);
                }
            }
        }
    }
}
