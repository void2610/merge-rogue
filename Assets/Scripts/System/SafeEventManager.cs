using System;
using UnityEngine;
using R3;
using SafeEventSystem;

public static class SafeEventManager
{
    // イベントシステム初期化フラグ
    private static bool _initialized = false;
    
    // 主要なゲームイベント - 新しいValueProcessorシステム
    public static readonly ValueProcessor<int> OnCoinGain = new();
    public static readonly ValueProcessor<int> OnCoinConsume = new();
    public static readonly ValueProcessor<int> OnPlayerExpGain = new();
    public static readonly ValueProcessor<AttackData> OnPlayerAttack = new();
    public static readonly ValueProcessor<int> OnPlayerDamage = new();
    public static readonly ValueProcessor<int> OnPlayerHeal = new();
    
    // 休憩・整理関連
    public static readonly ValueProcessor<int> OnRestEnter = new();
    public static readonly ValueProcessor<int> OnRest = new();
    public static readonly ValueProcessor<int> OnRestExit = new();

    // 非修正イベント（単純な通知用、R3のSubjectを使用）
    public static readonly Subject<Unit> OnBattleStartSimple = new();
    public static readonly Subject<EnemyBase> OnEnemyDefeatedSimple = new();
    public static readonly Subject<(BallBase, BallBase)> OnBallMergedSimple = new();
    public static readonly Subject<StageType> OnEventStageEnterSimple = new();
    public static readonly Subject<Unit> OnShopEnterSimple = new();
    public static readonly Subject<Unit> OnRestEnterSimple = new();
    public static readonly Subject<Unit> OnBallDropSimple = new();
    
    // 追加の簡易イベント
    public static readonly Subject<(BallBase, BallBase)> OnBallMerged = new();
    public static readonly Subject<StageType> OnEventStageEnter = new();
    public static readonly Subject<Unit> OnTreasureSkipped = new();
    public static readonly Subject<Unit> OnPlayerStatusEffectTriggered = new();
    public static readonly Subject<Unit> OnPlayerStatusEffectAdded = new();
    public static readonly Subject<Unit> OnBallSkip = new();
    public static readonly Subject<Unit> OnOrganise = new();
    public static readonly Subject<Unit> OnEnemyStatusEffectAdded = new();
    public static readonly Subject<Unit> OnEnemyStatusEffectTriggered = new();
    public static readonly Subject<Unit> OnRelicObtainedTreasure = new();
    public static readonly Subject<Unit> OnStageEventEnter = new();

    // 初期化メソッド
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        
        Debug.Log("[SafeEventManager] Initialized successfully");
    }

    // ===== メインイベント発行メソッド =====

    public static int TriggerCoinGain(int baseAmount)
    {
        var result = OnCoinGain.Process(baseAmount);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] CoinGain: {baseAmount} → {result}");
        #endif
        return result;
    }

    public static int TriggerCoinConsume(int baseAmount)
    {
        var result = OnCoinConsume.Process(baseAmount);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] CoinConsume: {baseAmount} → {result}");
        #endif
        return result;
    }

    public static AttackData TriggerPlayerAttack(AttackData baseAttack)
    {
        var result = OnPlayerAttack.Process(baseAttack);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] PlayerAttack: {baseAttack} → {result}");
        #endif
        return result;
    }

    public static int TriggerPlayerDamage(int baseDamage)
    {
        var result = OnPlayerDamage.Process(baseDamage);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] PlayerDamage: {baseDamage} → {result}");
        #endif
        return result;
    }

    public static int TriggerPlayerHeal(int baseHeal)
    {
        var result = OnPlayerHeal.Process(baseHeal);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] PlayerHeal: {baseHeal} → {result}");
        #endif
        return result;
    }

    public static int TriggerRest(int baseRest)
    {
        var result = OnRest.Process(baseRest);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] Rest: {baseRest} → {result}");
        #endif
        return result;
    }

    // ===== 追加のトリガーメソッド =====

    public static void TriggerBallMerged(BallBase ball1, BallBase ball2)
    {
        OnBallMerged.OnNext((ball1, ball2));
        OnBallMergedSimple.OnNext((ball1, ball2));
    }

    public static void TriggerEnemyDefeated(EnemyBase enemy)
    {
        OnEnemyDefeatedSimple.OnNext(enemy);
    }

    public static void TriggerBallDrop()
    {
        OnBallDropSimple.OnNext(Unit.Default);
    }

    public static void TriggerBallSkip()
    {
        OnBallSkip.OnNext(Unit.Default);
    }

    public static void TriggerBallRemove()
    {
        // ボール削除イベント（新規）
    }

    public static void TriggerTreasureSkipped()
    {
        OnTreasureSkipped.OnNext(Unit.Default);
    }

    public static void TriggerBallCreate()
    {
        // ボール作成イベント（新規）
    }

    public static void TriggerBattleStart()
    {
        OnBattleStartSimple.OnNext(Unit.Default);
    }

    public static void TriggerShopEnter()
    {
        OnShopEnterSimple.OnNext(Unit.Default);
    }

    public static void TriggerRestEnter()
    {
        OnRestEnterSimple.OnNext(Unit.Default);
    }

    // ===== 簡易登録メソッド =====

    /// <summary>
    /// コイン獲得修正を登録
    /// </summary>
    public static void RegisterCoinGainModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
    {
        OnCoinGain.AddProcessor(owner, processor, condition);
    }

    /// <summary>
    /// コイン消費修正を登録
    /// </summary>
    public static void RegisterCoinConsumeModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
    {
        OnCoinConsume.AddProcessor(owner, processor, condition);
    }

    /// <summary>
    /// プレイヤー攻撃修正を登録
    /// </summary>
    public static void RegisterPlayerAttackModifier(object owner, Func<AttackData, AttackData> processor, Func<bool> condition = null)
    {
        OnPlayerAttack.AddProcessor(owner, processor, condition);
    }

    /// <summary>
    /// プレイヤーダメージ修正を登録
    /// </summary>
    public static void RegisterPlayerDamageModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
    {
        OnPlayerDamage.AddProcessor(owner, processor, condition);
    }

    /// <summary>
    /// 休憩修正を登録
    /// </summary>
    public static void RegisterRestModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
    {
        OnRest.AddProcessor(owner, processor, condition);
    }

    /// <summary>
    /// 特定のオーナーの全修正を削除
    /// </summary>
    public static void RemoveProcessorsFor(object owner)
    {
        OnCoinGain.RemoveProcessorsFor(owner);
        OnCoinConsume.RemoveProcessorsFor(owner);
        OnPlayerAttack.RemoveProcessorsFor(owner);
        OnPlayerDamage.RemoveProcessorsFor(owner);
        OnPlayerHeal.RemoveProcessorsFor(owner);
        OnRest.RemoveProcessorsFor(owner);
    }

    /// <summary>
    /// すべての修正をクリア
    /// </summary>
    public static void Clear()
    {
        OnCoinGain.Clear();
        OnCoinConsume.Clear();
        OnPlayerAttack.Clear();
        OnPlayerDamage.Clear();
        OnPlayerHeal.Clear();
        OnRest.Clear();
        
        // Subjectも必要に応じてクリア
        OnBattleStartSimple?.Dispose();
        OnEnemyDefeatedSimple?.Dispose();
        OnBallMergedSimple?.Dispose();
        OnEventStageEnterSimple?.Dispose();
        OnShopEnterSimple?.Dispose();
        OnRestEnterSimple?.Dispose();
        OnBallDropSimple?.Dispose();
        
        // 追加のSubject
        OnBallMerged?.Dispose();
        OnEventStageEnter?.Dispose();
        OnTreasureSkipped?.Dispose();
        OnPlayerStatusEffectTriggered?.Dispose();
        OnPlayerStatusEffectAdded?.Dispose();
        OnBallSkip?.Dispose();
        OnOrganise?.Dispose();
        OnEnemyStatusEffectAdded?.Dispose();
        OnEnemyStatusEffectTriggered?.Dispose();
        OnRelicObtainedTreasure?.Dispose();
        OnStageEventEnter?.Dispose();
    }
}