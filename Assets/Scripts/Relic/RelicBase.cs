using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

/// <summary>
/// レリックベースクラス
/// イベントプロセッサーを使用してゲーム値を変更
/// IDisposableパターンでリソース管理を行う
/// </summary>
public abstract class RelicBase : IDisposable
{
    // UI関連
    protected RelicUI UI;
    protected bool IsCountable = false;
    protected readonly ReactiveProperty<int> Count = new(0);
    
    // イベント購読管理
    protected readonly List<IDisposable> _simpleSubscriptions = new();

    // ライフサイクル管理
    private bool _isInitialized = false;
    private bool _isDisposed = false;

    // 初期化
    public virtual void Init(RelicUI relicUI)
    {
        if (_isInitialized)
        {
            Debug.LogWarning($"[Relic] {GetType().Name} is already initialized");
            return;
        }

        UI = relicUI;
        UI?.EnableCount(IsCountable);
        UI?.SubscribeCount(Count);
        
        // レリック固有の効果を登録
        RegisterEffects();
        
        _isInitialized = true;
        Debug.Log($"[Relic] {GetType().Name} initialized");
    }

    // レリック固有の効果登録（派生クラスで実装）
    protected abstract void RegisterEffects();

    // 全ての効果を削除
    public virtual void RemoveAllEffects()
    {
        if (_isDisposed) return;

        // 登録されたモディファイアを全て削除
        EventManager.RemoveProcessorsFor(this);
        
        // シンプルなイベント購読も削除
        foreach (var subscription in _simpleSubscriptions)
        {
            subscription?.Dispose();
        }
        _simpleSubscriptions.Clear();
        
        // 修正リストは削除されました
        
        Debug.Log($"[Relic] {GetType().Name} effects removed");
    }

    // IDisposable実装
    public void Dispose()
    {
        if (_isDisposed) return;
        
        RemoveAllEffects();
        
        // ReactivePropertyも解放
        Count?.Dispose();
        
        _isDisposed = true;
        Debug.Log($"[Relic] {GetType().Name} disposed");
    }

    // ===== コイン関連のヘルパーメソッド =====


    /// <summary>
    /// コイン獲得倍率修正を登録
    /// </summary>
    protected void RegisterCoinGainMultiplier(float multiplier, Func<bool> condition = null)
    {
        EventManager.RegisterCoinGainModifier(this, ValueProcessors.Multiply(multiplier), condition);
    }

    /// <summary>
    /// コイン獲得加算修正を登録
    /// </summary>
    protected void RegisterCoinGainAddition(int amount, Func<bool> condition = null)
    {
        EventManager.RegisterCoinGainModifier(this, ValueProcessors.Add(amount), condition);
    }

    /// <summary>
    /// コイン消費の修正を登録
    /// </summary>
    protected void RegisterCoinConsumeModifier(
        Func<int, int> modifier,
        Func<bool> condition = null)
    {
        EventManager.RegisterCoinConsumeModifier(this, modifier, condition);
    }

    /// <summary>
    /// コイン消費を0にする修正を登録
    /// </summary>
    protected void RegisterCoinConsumeBlock(Func<bool> condition = null)
    {
        EventManager.RegisterCoinConsumeModifier(this, ValueProcessors.SetZero(), condition);
    }

    // ===== 攻撃関連のヘルパーメソッド =====

    /// <summary>
    /// プレイヤー攻撃の修正を登録
    /// </summary>
    protected void RegisterPlayerAttackModifier(
        Func<AttackData, AttackData> modifier,
        Func<bool> condition = null)
    {
        EventManager.RegisterPlayerAttackModifier(this, modifier, condition);
    }

    /// <summary>
    /// 攻撃力加算修正を登録
    /// </summary>
    protected void RegisterAttackAddition(AttackType attackType, int amount, Func<bool> condition = null)
    {
        EventManager.RegisterPlayerAttackModifier(this, ValueProcessors.AddAttack(attackType, amount), condition);
    }

    /// <summary>
    /// 単体攻撃を全体攻撃に変換する修正を登録
    /// </summary>
    protected void RegisterNormalToAllAttackConversion(Func<bool> condition = null, float multiplier = 1.0f)
    {
        EventManager.RegisterPlayerAttackModifier(this, ValueProcessors.ConvertAttackType(AttackType.Normal, AttackType.All, multiplier), condition);
    }

    // ===== ダメージ関連のヘルパーメソッド =====

    /// <summary>
    /// プレイヤーダメージの修正を登録
    /// </summary>
    protected void RegisterPlayerDamageModifier(
        Func<int, int> modifier,
        Func<bool> condition = null)
    {
        EventManager.RegisterPlayerDamageModifier(this, modifier, condition);
    }

    /// <summary>
    /// ダメージ蓄積カウンター（ReverseAlchemy、CreateBombWhenDamage用）
    /// </summary>
    protected void RegisterDamageAccumulator(int threshold, Action onThresholdReached)
    {
        RegisterPlayerDamageModifier(
            current =>
            {
                Count.Value += current;
                Debug.Log($"[Relic] {GetType().Name} Count updated: {Count.Value}");
                var activations = Count.Value / threshold;
                if (activations > 0)
                {
                    Count.Value %= threshold;
                    for (int i = 0; i < activations; i++)
                    {
                        onThresholdReached?.Invoke();
                    }
                    UI?.ActivateUI();
                }
                return current; // 値は変更しない
            }
        );
    }

    // ===== シンプルなイベント購読のヘルパーメソッド =====

    /// <summary>
    /// バトル開始イベントの購読
    /// </summary>
    protected void SubscribeBattleStart(Action onBattleStart)
    {
        var subscription = EventManager.OnBattleStartSimple.Subscribe(_ => onBattleStart?.Invoke());
        _simpleSubscriptions.Add(subscription);
    }

    /// <summary>
    /// 敵撃破イベントの購読
    /// </summary>
    protected void SubscribeEnemyDefeated(Action<EnemyBase> onEnemyDefeated)
    {
        var subscription = EventManager.OnEnemyDefeatedSimple.Subscribe(onEnemyDefeated);
        _simpleSubscriptions.Add(subscription);
    }

    /// <summary>
    /// ショップ入店イベントの購読
    /// </summary>
    protected void SubscribeShopEnter(Action onShopEnter)
    {
        var subscription = EventManager.OnShopEnterSimple.Subscribe(_ => onShopEnter?.Invoke());
        _simpleSubscriptions.Add(subscription);
    }

    /// <summary>
    /// 休憩入室イベントの購読
    /// </summary>
    protected void SubscribeRestEnter(Action onRestEnter)
    {
        var subscription = EventManager.OnRestEnterSimple.Subscribe(_ => onRestEnter?.Invoke());
        _simpleSubscriptions.Add(subscription);
    }

    // ===== UI関連ヘルパーメソッド =====

    /// <summary>
    /// UIをアクティブ化（エフェクト発動時の視覚的フィードバック）
    /// </summary>
    protected void ActivateUI()
    {
        UI?.ActivateUI();
    }

    /// <summary>
    /// 条件チェックのヘルパーメソッド
    /// </summary>
    protected Func<bool> PlayerHealthCondition(float healthPercentage)
    {
        return () =>
        {
            if (!GameManager.Instance?.Player) return false;
            var currentHealth = GameManager.Instance.Player.Health.Value;
            var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
            return currentHealth <= maxHealth * healthPercentage;
        };
    }

    protected Func<bool> PlayerHealthConditionAbove(float healthPercentage)
    {
        return () =>
        {
            if (!GameManager.Instance?.Player) return false;
            var currentHealth = GameManager.Instance.Player.Health.Value;
            var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
            return currentHealth > maxHealth * healthPercentage;
        };
    }

    protected Func<bool> GameStateCondition(params GameManager.GameState[] states)
    {
        return () => GameManager.Instance && Array.Exists(states, state => GameManager.Instance.state == state);
    }

    protected Func<bool> HasRelicCondition<T>() where T : RelicBase
    {
        return () => RelicManager.Instance.HasRelic(typeof(T));
    }
}

