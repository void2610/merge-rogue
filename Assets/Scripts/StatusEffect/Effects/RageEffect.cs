using UnityEngine;

/// <summary>
/// スタック数に応じて攻撃力に倍率をかける (1 + 0.1 * n)倍
/// </summary>
public class RageEffect : StatusEffectBase
{
    protected override int ModifyAttackEffect(AttackType type, int outgoingAttack)
    {
        var multiplier = (1 + StackCount * 0.1f);
        return (int)(outgoingAttack * multiplier);
    }
}