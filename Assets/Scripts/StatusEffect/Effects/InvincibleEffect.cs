using UnityEngine;

/// <summary>
/// スタックがある限り無敵
/// </summary>
public class InvincibleEffect : StatusEffectBase
{
    protected override int ModifyDamageEffect(int incomingDamage)
    {
        // TODO: シールドよりも優先度を高くする
        PlaySoundEffect();
        return 0;
    }
}