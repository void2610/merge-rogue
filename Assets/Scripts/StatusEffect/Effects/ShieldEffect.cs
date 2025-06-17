using System;
using UnityEngine;

/// <summary>
/// ダメージを吸収する、スタック数はダメージを受けるたびに減少
/// </summary>
public class ShieldEffect : StatusEffectBase
{
    protected override int ModifyDamageEffect(int incomingDamage)
    {
        if (StackCount <= 0) return incomingDamage;

        var absorbed = Math.Min(StackCount, incomingDamage);
        StackCount -= absorbed;

        PlaySoundEffect();
        return incomingDamage - absorbed;
    }
}