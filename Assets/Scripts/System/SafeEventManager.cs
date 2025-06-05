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

    // ===== 直接アクセス可能なイベントプロセッサー =====
    // 使用例：
    // var modifiedValue = SafeEventManager.OnCoinGain.Process(originalValue);
    // SafeEventManager.OnBattleStartSimple.OnNext(Unit.Default);

    // ===== レガシー互換性のための簡易登録メソッド =====
    // 注意: 直接 OnXXX.AddProcessor() を使用することを推奨

    /// <summary>
    /// コイン獲得修正を登録 - 直接 OnCoinGain.AddProcessor() を使用することを推奨
    /// </summary>
    public static void RegisterCoinGainModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
        => OnCoinGain.AddProcessor(owner, processor, condition);

    /// <summary>
    /// コイン消費修正を登録 - 直接 OnCoinConsume.AddProcessor() を使用することを推奨
    /// </summary>
    public static void RegisterCoinConsumeModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
        => OnCoinConsume.AddProcessor(owner, processor, condition);

    /// <summary>
    /// プレイヤー攻撃修正を登録 - 直接 OnPlayerAttack.AddProcessor() を使用することを推奨
    /// </summary>
    public static void RegisterPlayerAttackModifier(object owner, Func<AttackData, AttackData> processor, Func<bool> condition = null)
        => OnPlayerAttack.AddProcessor(owner, processor, condition);

    /// <summary>
    /// プレイヤーダメージ修正を登録 - 直接 OnPlayerDamage.AddProcessor() を使用することを推奨
    /// </summary>
    public static void RegisterPlayerDamageModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
        => OnPlayerDamage.AddProcessor(owner, processor, condition);

    /// <summary>
    /// 休憩修正を登録 - 直接 OnRest.AddProcessor() を使用することを推奨
    /// </summary>
    public static void RegisterRestModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
        => OnRest.AddProcessor(owner, processor, condition);

    /// <summary>
    /// 特定のオーナーの全修正を削除 - 各プロセッサーで直接 RemoveProcessorsFor() を呼ぶことも可能
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