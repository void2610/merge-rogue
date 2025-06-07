using System;
using System.Collections.Generic;
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
    public static readonly ValueProcessor<int> OnPlayerAttack = new();
    public static readonly ValueProcessor<int> OnPlayerDamage = new();
    public static readonly ValueProcessor<int> OnPlayerHeal = new();
    
    // 攻撃タイプと攻撃値を一緒に処理するValueProcessor
    public static readonly ValueProcessor<(AttackType type, int value)> OnAttackProcess = new();
    
    // 休憩・整理関連
    public static readonly ValueProcessor<int> OnRestEnterProcessor = new();
    public static readonly ValueProcessor<int> OnRest = new();
    public static readonly ValueProcessor<int> OnRestExit = new();
    
    // ステージタイプ決定関連
    public static readonly ValueProcessor<StageType> OnStageTypeDecision = new();

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
    public static readonly Subject<StatusEffectType> OnPlayerStatusEffectTriggered = new();
    public static readonly Subject<Unit> OnPlayerStatusEffectAdded = new();
    public static readonly Subject<Unit> OnBallSkip = new();
    public static readonly Subject<Unit> OnOrganise = new();
    public static readonly Subject<StatusEffectType> OnEnemyStatusEffectAdded = new();
    public static readonly Subject<StatusEffectType> OnEnemyStatusEffectTriggered = new();
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


    /// <summary>
    /// 特定のオーナーのプロセッサーをすべて削除
    /// </summary>
    public static void RemoveProcessorsFor(object owner)
    {
        OnCoinGain.RemoveProcessorsFor(owner);
        OnCoinConsume.RemoveProcessorsFor(owner);
        OnPlayerAttack.RemoveProcessorsFor(owner);
        OnAttackProcess.RemoveProcessorsFor(owner);
        OnPlayerDamage.RemoveProcessorsFor(owner);
        OnPlayerHeal.RemoveProcessorsFor(owner);
        OnRest.RemoveProcessorsFor(owner);
    }

    /// <summary>
    /// すべてのプロセッサーをクリア
    /// </summary>
    public static void Clear()
    {
        OnCoinGain.Clear();
        OnCoinConsume.Clear();
        OnPlayerAttack.Clear();
        OnAttackProcess.Clear();
        OnPlayerDamage.Clear();
        OnPlayerHeal.Clear();
        OnRest.Clear();
        
        // 注意: Subjectはdisposeしません。
        // これらは静的なreadonly フィールドなので、アプリケーション全体のライフタイムで生き続けます。
        // 個々のサブスクリプションは、各オブジェクトのDisposeメソッドでクリーンアップされます。
    }
}