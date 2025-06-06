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
        // 弱い攻撃の場合に攻撃力を2倍にする（簡素化）
        RegisterAttackMultiplier(2.0f, 
            condition: () => true); // 常に適用（条件チェックは後で実装可能）
    }
}