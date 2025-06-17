using UnityEngine;

/// <summary>
/// スタック数に応じて通常攻撃で追加ダメージを与える
/// </summary>
public class PowerEffect : StatusEffectBase
{
    protected override int ModifyAttackEffect(AttackType type, int outgoingAttack)
    {
        if (type == AttackType.Normal)
        {
            return outgoingAttack + StackCount;
        }
        return outgoingAttack;
    }
}