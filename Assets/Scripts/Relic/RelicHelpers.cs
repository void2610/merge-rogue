using System;
using UnityEngine;
using R3;

/// <summary>
/// レリック用のヘルパーメソッド集
/// RelicBaseから分離して再利用可能にする
/// </summary>
public static class RelicHelpers
{
    // ===== コイン関連のヘルパーメソッド =====

    /// <summary>
    /// コイン獲得倍率修正を登録
    /// </summary>
    public static void RegisterCoinGainMultiplier(RelicBase relic, float multiplier, Func<bool> condition = null)
    {
        EventManager.OnCoinGain.AddProcessor(relic, ValueProcessors.Multiply(multiplier), condition);
    }

    /// <summary>
    /// コイン獲得加算修正を登録
    /// </summary>
    public static void RegisterCoinGainAddition(RelicBase relic, int amount, Func<bool> condition = null)
    {
        EventManager.OnCoinGain.AddProcessor(relic, ValueProcessors.Add(amount), condition);
    }

    /// <summary>
    /// コイン消費の修正を登録
    /// </summary>
    public static void RegisterCoinConsumeModifier(RelicBase relic, Func<int, int> modifier, Func<bool> condition = null)
    {
        EventManager.OnCoinConsume.AddProcessor(relic, modifier, condition);
    }

    /// <summary>
    /// コイン消費を0にする修正を登録
    /// </summary>
    public static void RegisterCoinConsumeBlock(RelicBase relic, Func<bool> condition = null)
    {
        EventManager.OnCoinConsume.AddProcessor(relic, ValueProcessors.SetZero(), condition);
    }

    // ===== 攻撃関連のヘルパーメソッド =====

    /// <summary>
    /// プレイヤー攻撃の修正を登録
    /// </summary>
    public static void RegisterPlayerAttackModifier(RelicBase relic, Func<int, int> modifier, Func<bool> condition = null)
    {
        EventManager.OnPlayerAttack.AddProcessor(relic, modifier, condition);
    }

    /// <summary>
    /// 攻撃力加算修正を登録
    /// </summary>
    public static void RegisterAttackAddition(RelicBase relic, int amount, Func<bool> condition = null)
    {
        EventManager.OnPlayerAttack.AddProcessor(relic, attack => attack + amount, condition);
    }

    /// <summary>
    /// 攻撃力倍率修正を登録
    /// </summary>
    public static void RegisterAttackMultiplier(RelicBase relic, float multiplier, Func<bool> condition = null)
    {
        EventManager.OnPlayerAttack.AddProcessor(relic, attack => (int)(attack * multiplier), condition);
    }

    // ===== ダメージ関連のヘルパーメソッド =====

    /// <summary>
    /// プレイヤーダメージの修正を登録
    /// </summary>
    public static void RegisterPlayerDamageModifier(RelicBase relic, Func<int, int> modifier, Func<bool> condition = null)
    {
        EventManager.OnPlayerDamage.AddProcessor(relic, modifier, condition);
    }

    // ===== 条件チェックのヘルパーメソッド =====

    /// <summary>
    /// プレイヤーのHP条件（以下）
    /// </summary>
    public static Func<bool> PlayerHealthCondition(float healthPercentage)
    {
        return () =>
        {
            if (!GameManager.Instance?.Player) return false;
            var currentHealth = GameManager.Instance.Player.Health.Value;
            var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
            return currentHealth <= maxHealth * healthPercentage;
        };
    }

    /// <summary>
    /// プレイヤーのHP条件（以上）
    /// </summary>
    public static Func<bool> PlayerHealthConditionAbove(float healthPercentage)
    {
        return () =>
        {
            if (!GameManager.Instance?.Player) return false;
            var currentHealth = GameManager.Instance.Player.Health.Value;
            var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
            return currentHealth > maxHealth * healthPercentage;
        };
    }

    /// <summary>
    /// ゲーム状態条件
    /// </summary>
    public static Func<bool> GameStateCondition(params GameManager.GameState[] states)
    {
        return () => GameManager.Instance && Array.Exists(states, state => GameManager.Instance.state == state);
    }

    /// <summary>
    /// 他のレリック所持条件
    /// </summary>
    public static Func<bool> HasRelicCondition<T>() where T : RelicBase
    {
        return () => RelicService.Instance?.HasRelic(typeof(T)) ?? false;
    }

    // ===== イベント購読のヘルパーメソッド =====

    /// <summary>
    /// バトル開始イベントの購読
    /// </summary>
    public static IDisposable SubscribeBattleStart(Action onBattleStart)
    {
        return EventManager.OnBattleStart.Subscribe(_ => onBattleStart?.Invoke());
    }

    /// <summary>
    /// 敵撃破イベントの購読
    /// </summary>
    public static IDisposable SubscribeEnemyDefeated(Action<EnemyBase> onEnemyDefeated)
    {
        return EventManager.OnEnemyDefeated.Subscribe(onEnemyDefeated);
    }

    /// <summary>
    /// ショップ入店イベントの購読
    /// </summary>
    public static IDisposable SubscribeShopEnter(Action onShopEnter)
    {
        return EventManager.OnShopEnter.Subscribe(_ => onShopEnter?.Invoke());
    }

    /// <summary>
    /// 休憩入室イベントの購読
    /// </summary>
    public static IDisposable SubscribeRestEnter(Action onRestEnter)
    {
        return EventManager.OnRestEnter.Subscribe(_ => onRestEnter?.Invoke());
    }
}