using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// コイン10枚で単体攻撃を1.5倍の全体攻撃に変換する
/// 賢者の石（NoConsumeCoinDuringBattle）がある場合はコイン消費なし
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class AllAttackByTenCoin : RelicBase
{
    protected override void RegisterEffects()
    {
        // 攻撃変換処理
        RegisterPlayerAttackModifier(
            current =>
            {
                // 条件チェック
                if (!CanConvertAttack(current)) return current;

                // コイン消費（賢者の石がない場合のみ）
                if (!HasRelicCondition<NoConsumeCoinDuringBattle>()())
                {
                    GameManager.Instance.SubCoin(10);
                }

                // 攻撃変換
                var normalAttack = current.GetAttack(AttackType.Normal);
                if (normalAttack > 0)
                {
                    var convertedAttack = (int)(normalAttack * 1.5f);
                    current = current
                        .SetAttack(AttackType.Normal, 0)
                        .AddAttack(AttackType.All, convertedAttack);
                    ActivateUI();
                }
                
                return current;
            },
            condition: () => CanConvertAttackCondition()
        );

        // 初期化時にも一度実行（元実装の EffectImpl(Unit.Default) に相当）
        // NOTE: これは元実装にあったが、攻撃がない状態での実行なので実質的に何もしない
        // 必要に応じてバトル開始時に実行するように変更可能
    }

    /// <summary>
    /// 攻撃変換が可能な条件をチェック
    /// </summary>
    private bool CanConvertAttackCondition()
    {
        var coin = GameManager.Instance.Coin.Value;
        var enemyCount = GameManager.Instance.EnemyContainer.GetCurrentEnemyCount();
        var hasNoConsumeCoin = HasRelicCondition<NoConsumeCoinDuringBattle>()();

        // コインが足りるか、賢者の石があるか、かつ敵が2体以上いる場合
        return (coin >= 10 || hasNoConsumeCoin) && enemyCount >= 2;
    }

    /// <summary>
    /// 現在の攻撃状態で変換が可能かチェック
    /// </summary>
    private bool CanConvertAttack(AttackData attacks)
    {
        return attacks.GetAttack(AttackType.Normal) > 0 && 
               CanConvertAttackCondition();
    }
}