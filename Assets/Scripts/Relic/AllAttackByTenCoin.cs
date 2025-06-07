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
        // 攻撃処理：Normal → All変換 + 1.5倍攻撃力（条件を満たす場合）
        EventManager.OnAttackProcess.AddProcessor(this, attackData =>
        {
            if (attackData.type == AttackType.Normal && CanConvertAttackCondition())
            {
                ConsumeCoinsForConversion();
                ActivateUI();
                return (AttackType.All, (int)(attackData.value * 1.5f));
            }
            return attackData;
        });
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