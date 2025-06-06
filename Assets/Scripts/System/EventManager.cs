using System;
using UnityEngine;
using R3;

public static class EventManager
{
    // イベントシステム初期化フラグ
    private static bool _initialized = false;
    
    // 値を変更するゲームイベント（ValueProcessorを使用）
    public static readonly ValueProcessor<int> OnCoinGain = new();
    public static readonly ValueProcessor<int> OnCoinConsume = new();
    public static readonly ValueProcessor<int> OnPlayerExpGain = new();
    public static readonly ValueProcessor<AttackData> OnPlayerAttack = new();
    public static readonly ValueProcessor<int> OnPlayerDamage = new();
    public static readonly ValueProcessor<int> OnPlayerHeal = new();
    
    // 休憩・整理関連
    public static readonly ValueProcessor<int> OnRestEnterProcessor = new();
    public static readonly ValueProcessor<int> OnRest = new();
    public static readonly ValueProcessor<int> OnRestExit = new();

    // 通知専用イベント（R3のSubjectを使用）
    public static readonly Subject<Unit> OnBattleStart = new();
    public static readonly Subject<EnemyBase> OnEnemyDefeated = new();
    public static readonly Subject<Unit> OnShopEnter = new();
    public static readonly Subject<Unit> OnRestEnter = new();
    public static readonly Subject<Unit> OnBallDrop = new();
    
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
        
        Debug.Log("[EventManager] Initialized successfully");
    }

    // ===== イベントプロセッサー使用例 =====
    // var modifiedValue = EventManager.OnCoinGain.Process(originalValue);
    // EventManager.OnBattleStart.OnNext(Unit.Default);

    // ===== 値変更プロセッサーの登録メソッド =====

    /// <summary>
    /// コイン獲得値を変更するプロセッサーを登録
    /// </summary>
    public static void RegisterCoinGainModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
        => OnCoinGain.AddProcessor(owner, processor, condition);

    /// <summary>
    /// コイン消費値を変更するプロセッサーを登録
    /// </summary>
    public static void RegisterCoinConsumeModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
        => OnCoinConsume.AddProcessor(owner, processor, condition);

    /// <summary>
    /// プレイヤー攻撃データを変更するプロセッサーを登録
    /// </summary>
    public static void RegisterPlayerAttackModifier(object owner, Func<AttackData, AttackData> processor, Func<bool> condition = null)
        => OnPlayerAttack.AddProcessor(owner, processor, condition);

    /// <summary>
    /// プレイヤーダメージ値を変更するプロセッサーを登録
    /// </summary>
    public static void RegisterPlayerDamageModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
        => OnPlayerDamage.AddProcessor(owner, processor, condition);

    /// <summary>
    /// 休憩効果値を変更するプロセッサーを登録
    /// </summary>
    public static void RegisterRestModifier(object owner, Func<int, int> processor, Func<bool> condition = null)
        => OnRest.AddProcessor(owner, processor, condition);

    /// <summary>
    /// 特定のオーナーのプロセッサーをすべて削除
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
    /// すべてのプロセッサーとイベントをクリア
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
        OnBattleStart?.Dispose();
        OnEnemyDefeated?.Dispose();
        OnShopEnter?.Dispose();
        OnRestEnter?.Dispose();
        OnBallDrop?.Dispose();
        
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