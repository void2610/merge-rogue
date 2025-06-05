using System;
using System.Collections.Generic;
using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// 新しい安全なレリックベースクラス
/// パイプライン型修正システムを使用して、競合状態を防ぐ
/// </summary>
public abstract class SafeRelicBase : MonoBehaviour
{
    // UI関連
    protected RelicUI UI;
    protected bool IsCountable = false;
    protected readonly ReactiveProperty<int> Count = new(0);
    
    // モディファイア管理
    protected readonly List<IModifier<int>> _intModifiers = new();
    protected readonly List<IModifier<AttackData>> _attackModifiers = new();
    protected readonly List<IModifier<BallData>> _ballDataModifiers = new();
    protected readonly List<IDisposable> _simpleSubscriptions = new();

    // 初期化
    public virtual void Init(RelicUI relicUI)
    {
        UI = relicUI;
        UI.EnableCount(IsCountable);
        UI.SubscribeCount(Count);
        
        // レリック固有の効果を登録
        RegisterEffects();
        
        // ゲームオブジェクト破棄時の自動クリーンアップは手動で行う
        
        Debug.Log($"[SafeRelic] {GetType().Name} initialized");
    }

    // レリック固有の効果登録（派生クラスで実装）
    protected abstract void RegisterEffects();

    // 全ての効果を削除
    public virtual void RemoveAllEffects()
    {
        // 登録されたモディファイアを全て削除
        SafeEventManager.RemoveModifiersFor(this);
        
        // シンプルなイベント購読も削除
        foreach (var subscription in _simpleSubscriptions)
        {
            subscription?.Dispose();
        }
        _simpleSubscriptions.Clear();
        
        // ローカルリストもクリア
        _intModifiers.Clear();
        _attackModifiers.Clear();
        _ballDataModifiers.Clear();
        
        Debug.Log($"[SafeRelic] {GetType().Name} effects removed");
    }

    // ===== コイン関連のヘルパーメソッド =====

    /// <summary>
    /// コイン獲得時の修正を登録
    /// </summary>
    protected void RegisterCoinGainModifier(
        ModificationPhase phase,
        Func<int, int, int> modifier,
        Func<bool> condition = null,
        int priority = 0)
    {
        var mod = new FunctionalModifier<int>(phase, priority, this, 
            (original, current) => modifier(original, current), condition);
        _intModifiers.Add(mod);
        SafeEventManager.RegisterCoinGainModifier(mod);
    }

    /// <summary>
    /// コイン獲得倍率修正を登録
    /// </summary>
    protected void RegisterCoinGainMultiplier(float multiplier, Func<bool> condition = null, int priority = 0)
    {
        var mod = new MultiplicationModifier(multiplier, this, condition, priority);
        _intModifiers.Add(mod);
        SafeEventManager.RegisterCoinGainModifier(mod);
    }

    /// <summary>
    /// コイン獲得加算修正を登録
    /// </summary>
    protected void RegisterCoinGainAddition(int amount, Func<bool> condition = null, int priority = 0)
    {
        var mod = new AdditionModifier(amount, this, condition, priority);
        _intModifiers.Add(mod);
        SafeEventManager.RegisterCoinGainModifier(mod);
    }

    /// <summary>
    /// コイン消費の修正を登録
    /// </summary>
    protected void RegisterCoinConsumeModifier(
        ModificationPhase phase,
        Func<int, int, int> modifier,
        Func<bool> condition = null,
        int priority = 0)
    {
        var mod = new FunctionalModifier<int>(phase, priority, this,
            (original, current) => modifier(original, current), condition);
        _intModifiers.Add(mod);
        SafeEventManager.RegisterCoinConsumeModifier(mod);
    }

    /// <summary>
    /// コイン消費を0にする修正を登録
    /// </summary>
    protected void RegisterCoinConsumeBlock(Func<bool> condition = null, int priority = 0)
    {
        var mod = new OverrideModifier(0, this, condition, priority);
        _intModifiers.Add(mod);
        SafeEventManager.RegisterCoinConsumeModifier(mod);
    }

    // ===== 攻撃関連のヘルパーメソッド =====

    /// <summary>
    /// プレイヤー攻撃の修正を登録
    /// </summary>
    protected void RegisterPlayerAttackModifier(
        ModificationPhase phase,
        Func<AttackData, AttackData, AttackData> modifier,
        Func<bool> condition = null,
        int priority = 0)
    {
        var mod = new AttackDataModifier(modifier, phase, this, condition, priority);
        _attackModifiers.Add(mod);
        SafeEventManager.RegisterPlayerAttackModifier(mod);
    }

    /// <summary>
    /// 攻撃力加算修正を登録
    /// </summary>
    protected void RegisterAttackAddition(AttackType attackType, int amount, Func<bool> condition = null, int priority = 0)
    {
        var mod = new AttackAdditionModifier(attackType, amount, this, condition, priority);
        _attackModifiers.Add(mod);
        SafeEventManager.RegisterPlayerAttackModifier(mod);
    }

    /// <summary>
    /// 単体攻撃を全体攻撃に変換する修正を登録
    /// </summary>
    protected void RegisterNormalToAllAttackConversion(Func<bool> condition = null, float multiplier = 1.0f, int priority = 0)
    {
        var mod = new AttackConversionModifier(AttackType.Normal, AttackType.All, this, multiplier, condition, priority);
        _attackModifiers.Add(mod);
        SafeEventManager.RegisterPlayerAttackModifier(mod);
    }

    // ===== ダメージ関連のヘルパーメソッド =====

    /// <summary>
    /// プレイヤーダメージの修正を登録
    /// </summary>
    protected void RegisterPlayerDamageModifier(
        ModificationPhase phase,
        Func<int, int, int> modifier,
        Func<bool> condition = null,
        int priority = 0)
    {
        var mod = new FunctionalModifier<int>(phase, priority, this,
            (original, current) => modifier(original, current), condition);
        _intModifiers.Add(mod);
        SafeEventManager.RegisterPlayerDamageModifier(mod);
    }

    /// <summary>
    /// ダメージ蓄積カウンター（ReverseAlchemy、CreateBombWhenDamage用）
    /// </summary>
    protected void RegisterDamageAccumulator(int threshold, Action onThresholdReached)
    {
        RegisterPlayerDamageModifier(
            ModificationPhase.PostProcess,
            (_, current) =>
            {
                Count.Value += current;
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
        var subscription = SafeEventManager.OnBattleStartSimple.Subscribe(_ => onBattleStart?.Invoke());
        _simpleSubscriptions.Add(subscription);
    }

    /// <summary>
    /// 敵撃破イベントの購読
    /// </summary>
    protected void SubscribeEnemyDefeated(Action<EnemyBase> onEnemyDefeated)
    {
        var subscription = SafeEventManager.OnEnemyDefeatedSimple.Subscribe(onEnemyDefeated);
        _simpleSubscriptions.Add(subscription);
    }

    /// <summary>
    /// ショップ入店イベントの購読
    /// </summary>
    protected void SubscribeShopEnter(Action onShopEnter)
    {
        var subscription = SafeEventManager.OnShopEnterSimple.Subscribe(_ => onShopEnter?.Invoke());
        _simpleSubscriptions.Add(subscription);
    }

    /// <summary>
    /// 休憩入室イベントの購読
    /// </summary>
    protected void SubscribeRestEnter(Action onRestEnter)
    {
        var subscription = SafeEventManager.OnRestEnterSimple.Subscribe(_ => onRestEnter?.Invoke());
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

    protected Func<bool> HasRelicCondition<T>() where T : SafeRelicBase
    {
        return () => RelicManager.Instance.HasRelic(typeof(T));
    }
}

// 汎用的な関数型モディファイア
public class FunctionalModifier<T> : ModifierBase<T>
{
    private readonly Func<T, T, T> _modifier;
    private readonly Func<bool> _condition;

    public FunctionalModifier(
        ModificationPhase phase,
        int priority,
        object owner,
        Func<T, T, T> modifier,
        Func<bool> condition = null) : base(phase, priority, owner)
    {
        _modifier = modifier;
        _condition = condition ?? (() => true);
    }

    public override bool CanApply(T originalValue, T currentValue) => _condition();
    public override T Apply(T originalValue, T currentValue) => _modifier(originalValue, currentValue);
}