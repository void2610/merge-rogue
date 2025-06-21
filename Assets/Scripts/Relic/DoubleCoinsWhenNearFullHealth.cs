using UnityEngine;

/// <summary>
/// HPが最大HPの80%以上の時、コイン獲得量が2倍になる
/// </summary>
public class DoubleCoinsWhenNearFullHealth : RelicBase
{
    public override void RegisterEffects()
    {
        // HPが80%以上の時にコイン獲得量を2倍にする
        EventManager.OnCoinGain.AddProcessor(this, current =>
        {
            if (IsPlayerHealthAbove(0.8f))
            {
                ActivateUI();
                return (int)(current * 2.0f);
            }
            return current;
        });
    }
}