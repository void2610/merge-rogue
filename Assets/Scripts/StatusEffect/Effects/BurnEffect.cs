using UnityEngine;

/// <summary>
/// 毎ターン、スタック数に応じたダメージを受ける
/// </summary>
public class BurnEffect : StatusEffectBase
{
    protected override void OnTurnEndEffect()
    {
        var damage = StackCount;
        SeManager.Instance.PlaySe("playerAttack");
        Owner.Damage(AttackType.Normal, damage);
        PlaySoundEffect();
    }
}