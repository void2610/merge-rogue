using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通常攻撃力が30以下の時、全体攻撃に変換する
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class AllAttackWhenWeakAttack : RelicBase
{
    private const int WEAK_ATTACK_THRESHOLD = 30;

    protected override void RegisterEffects()
    {
        // 弱い単体攻撃を全体攻撃に変換
        RegisterNormalToAllAttackConversion(
            condition: () => true, // 条件は攻撃値チェック内で行う
            multiplier: 1.0f
        );

        // より詳細な制御が必要な場合は個別に実装
        RegisterPlayerAttackModifier(
            (original, current) =>
            {
                var normalAttack = current.GetAttack(AttackType.Normal);
                if (normalAttack > 0 && normalAttack <= WEAK_ATTACK_THRESHOLD)
                {
                    // 全体攻撃に変換
                    current = current
                        .SetAttack(AttackType.Normal, 0)
                        .AddAttack(AttackType.All, normalAttack);
                    ActivateUI();
                }
                return current;
            }
        );
    }
}