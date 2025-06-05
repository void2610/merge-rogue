using UnityEngine;

/// <summary>
/// HPが最大HPの80%以上の時、コイン獲得量が2倍になる
/// </summary>
public class DoubleCoinsWhenNearFullHealth : RelicBase
{
    protected override void RegisterEffects()
    {
        // HPが80%以上の時にコイン獲得量を2倍にする
        RegisterCoinGainMultiplier(
            multiplier: 2.0f,
            condition: PlayerHealthConditionAbove(0.8f) // HP > 80%
        );
    }
}