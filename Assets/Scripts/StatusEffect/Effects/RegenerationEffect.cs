using UnityEngine;

/// <summary>
/// 毎ターン、スタック数に応じてHPを回復する
/// </summary>
public class RegenerationEffect : StatusEffectBase
{
    protected override void OnTurnEndEffect()
    {
        var heal = StackCount;
        Owner.Heal(heal);
        PlaySoundEffect();
    }
}