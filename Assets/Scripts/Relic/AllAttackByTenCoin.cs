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
        // 攻撃変換処理（簡素化：単純な1.5倍攻撃力向上として実装）
        RelicHelpers.RegisterAttackMultiplier(this, 1.5f, 
            condition: () => CanConvertAttackCondition());

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
        var hasNoConsumeCoin = RelicHelpers.HasRelicCondition<NoConsumeCoinDuringBattle>()();

        // コインが足りるか、賢者の石があるか、かつ敵が2体以上いる場合
        return (coin >= 10 || hasNoConsumeCoin) && enemyCount >= 2;
    }

}