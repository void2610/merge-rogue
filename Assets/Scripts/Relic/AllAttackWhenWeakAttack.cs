using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 通常攻撃力が30以下の時、全体攻撃に変換する
/// </summary>
public class AllAttackWhenWeakAttack : RelicBase
{
    private const int WEAK_ATTACK_THRESHOLD = 30;

    protected override void RegisterEffects()
    {
        // 攻撃タイプ変換：Normal → All（攻撃力30以下の場合）
        EventManager.OnAttackProcess.AddProcessor(this, attackData =>
        {
            if (attackData.type == AttackType.Normal && attackData.value <= WEAK_ATTACK_THRESHOLD)
            {
                ActivateUI();
                return (AttackType.All, attackData.value);
            }
            return attackData;
        });
    }
}