using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// コイン10枚で単体攻撃を1.5倍の全体攻撃に変換する
/// 賢者の石（NoConsumeCoinDuringBattle）がある場合はコイン消費なし
/// </summary>
public class AllAttackByTenCoin : RelicBase
{
    protected override void RegisterEffects()
    {
        // 攻撃タイプ変換：Normal → All（条件を満たす場合）
        EventManager.RegisterAttackTypeConverter(this, attackType =>
        {
            if (attackType == AttackType.Normal && CanConvertAttackCondition())
            {
                ConsumeCoinsForConversion();
                return AttackType.All;
            }
            return attackType;
        });
        
        // 攻撃力1.5倍（変換時のみ）
        RelicHelpers.RegisterAttackMultiplier(this, 1.5f, 
            condition: () => CanConvertAttackCondition());
    }

    /// <summary>
    /// 攻撃変換が可能な条件をチェック
    /// </summary>
    private bool CanConvertAttackCondition()
    {
        var coin = GameManager.Instance.Coin.Value;
        var enemyCount = GameManager.Instance.EnemyContainer.GetCurrentEnemyCount();
        var hasNoConsumeCoin = RelicHelpers.HasRelicCondition<NoConsumeCoinDuringBattle>()();

        // コインが足りるか、賢者の石があるか、かつ敵が2体以上いる場合
        return (coin >= 10 || hasNoConsumeCoin) && enemyCount >= 2;
    }
    
    /// <summary>
    /// 攻撃変換時にコインを消費
    /// </summary>
    private void ConsumeCoinsForConversion()
    {
        var hasNoConsumeCoin = RelicHelpers.HasRelicCondition<NoConsumeCoinDuringBattle>()();
        if (!hasNoConsumeCoin)
        {
            GameManager.Instance.SubCoin(10);
        }
    }

}