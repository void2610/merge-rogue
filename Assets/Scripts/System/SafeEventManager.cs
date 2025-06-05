using System;
using System.Collections.Generic;
using UnityEngine;
using R3;
using SafeEventSystem;

public static class SafeEventManager
{
    // イベントシステム初期化フラグ
    private static bool _initialized = false;
    
    // 主要なゲームイベント
    public static readonly ModifiableEvent<int> OnCoinGain = new();
    public static readonly ModifiableEvent<int> OnCoinConsume = new();
    public static readonly ModifiableEvent<int> OnPlayerExpGain = new();
    public static readonly ModifiableEvent<AttackData> OnPlayerAttack = new();
    public static readonly ModifiableEvent<int> OnPlayerDamage = new();
    public static readonly ModifiableEvent<int> OnPlayerHeal = new();
    
    // 状態異常関連イベント
    public static readonly ModifiableEvent<(StatusEffectType type, int stack)> OnPlayerStatusEffectAdded = new();
    public static readonly ModifiableEvent<(StatusEffectType type, int stack)> OnPlayerStatusEffectTriggered = new();
    public static readonly ModifiableEvent<(EnemyBase enemy, StatusEffectType type, int stack)> OnEnemyStatusEffectAdded = new();
    public static readonly ModifiableEvent<(EnemyBase enemy, StatusEffectType type, int stack)> OnEnemyStatusEffectTriggered = new();
    
    // 敵関連イベント
    public static readonly ModifiableEvent<int> OnEnemySpawn = new();
    public static readonly ModifiableEvent<float> OnEnemyInit = new();
    public static readonly ModifiableEvent<int> OnEnemyAttack = new();
    public static readonly ModifiableEvent<int> OnEnemyDamage = new();
    public static readonly ModifiableEvent<int> OnEnemyHeal = new();
    
    // ボール関連イベント
    public static readonly ModifiableEvent<BallData> OnBallCreate = new();
    public static readonly ModifiableEvent<int> OnBallDrop = new();
    public static readonly ModifiableEvent<int> OnBallSkip = new();
    public static readonly ModifiableEvent<(BallBase ball1, BallBase ball2)> OnBallMerged = new();
    public static readonly ModifiableEvent<int> OnBallRemove = new();
    
    // ゲーム進行イベント
    public static readonly ModifiableEvent<int> OnBattleStart = new();
    public static readonly ModifiableEvent<bool> OnPlayerDeath = new();
    public static readonly ModifiableEvent<EnemyBase> OnEnemyDefeated = new();
    public static readonly ModifiableEvent<StageType> OnEventStageEnter = new();
    public static readonly ModifiableEvent<StageEventBase> OnStageEventEnter = new();
    
    // ショップ・宝箱関連イベント
    public static readonly ModifiableEvent<int> OnShopEnter = new();
    public static readonly ModifiableEvent<int> OnShopExit = new();
    public static readonly ModifiableEvent<int> OnItemPurchased = new();
    public static readonly ModifiableEvent<int> OnTreasureOpened = new();
    public static readonly ModifiableEvent<RelicData> OnRelicObtainedTreasure = new();
    public static readonly ModifiableEvent<int> OnTreasureSkipped = new();
    
    // 休憩・整理関連イベント
    public static readonly ModifiableEvent<int> OnRestEnter = new();
    public static readonly ModifiableEvent<int> OnRest = new();
    public static readonly ModifiableEvent<int> OnRestExit = new();
    public static readonly ModifiableEvent<int> OnOrganise = new();

    // 非修正イベント（単純な通知用、R3のSubjectを使用）
    public static readonly Subject<Unit> OnBattleStartSimple = new();
    public static readonly Subject<EnemyBase> OnEnemyDefeatedSimple = new();
    public static readonly Subject<(BallBase, BallBase)> OnBallMergedSimple = new();
    public static readonly Subject<StageType> OnEventStageEnterSimple = new();
    public static readonly Subject<Unit> OnShopEnterSimple = new();
    public static readonly Subject<Unit> OnRestEnterSimple = new();

    // 初期化メソッド
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        
        // 既存のEventManagerとの互換性のためのブリッジ設定
        SetupLegacyBridge();
        
        Debug.Log("[SafeEventManager] Initialized successfully");
    }

    // レガシーEventManagerとの互換性ブリッジ
    private static void SetupLegacyBridge()
    {
        // 新システムのイベントが発生したら、必要に応じて既存システムにも通知
        // これにより段階的移行が可能
    }

    // ===== メインイベント発行メソッド =====

    public static int TriggerCoinGain(int baseAmount)
    {
        var result = OnCoinGain.ProcessModifications(baseAmount);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] CoinGain: {baseAmount} → {result}");
        #endif
        return result;
    }

    public static int TriggerCoinConsume(int baseAmount)
    {
        var result = OnCoinConsume.ProcessModifications(baseAmount);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] CoinConsume: {baseAmount} → {result}");
        #endif
        return result;
    }

    public static AttackData TriggerPlayerAttack(AttackData baseAttack)
    {
        var result = OnPlayerAttack.ProcessModifications(baseAttack);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] PlayerAttack: {baseAttack} → {result}");
        #endif
        return result;
    }
    
    // Dictionary版の互換性メソッド
    public static Dictionary<AttackType, int> TriggerPlayerAttack(Dictionary<AttackType, int> baseAttack)
    {
        var attackData = AttackData.FromDictionary(baseAttack);
        var result = TriggerPlayerAttack(attackData);
        return result.ToDictionary();
    }

    public static int TriggerPlayerDamage(int baseDamage)
    {
        var result = OnPlayerDamage.ProcessModifications(baseDamage);
        // 状態異常処理トリガー（必要に応じてStatusEffectType.Burnなど有効な値を使用）
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] PlayerDamage: {baseDamage} → {result}");
        #endif
        return result;
    }

    public static int TriggerPlayerHeal(int baseHeal)
    {
        var result = OnPlayerHeal.ProcessModifications(baseHeal);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] PlayerHeal: {baseHeal} → {result}");
        #endif
        return result;
    }

    public static BallData TriggerBallCreate(BallData baseBallData)
    {
        var result = OnBallCreate.ProcessModifications(baseBallData);
        return result;
    }

    public static void TriggerBattleStart()
    {
        OnBattleStart.ProcessModifications(0);
        OnBattleStartSimple.OnNext(Unit.Default);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log("[SafeEvent] BattleStart triggered");
        #endif
    }

    public static void TriggerEnemyDefeated(EnemyBase enemy)
    {
        OnEnemyDefeated.ProcessModifications(enemy);
        OnEnemyDefeatedSimple.OnNext(enemy);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] EnemyDefeated: {enemy.EnemyName}");
        #endif
    }

    public static void TriggerBallMerged(BallBase ball1, BallBase ball2)
    {
        OnBallMerged.ProcessModifications((ball1, ball2));
        OnBallMergedSimple.OnNext((ball1, ball2));
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] BallMerged: {ball1.Data.className} + {ball2.Data.className}");
        #endif
    }

    public static void TriggerShopEnter()
    {
        OnShopEnter.ProcessModifications(0);
        OnShopEnterSimple.OnNext(Unit.Default);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log("[SafeEvent] ShopEnter triggered");
        #endif
    }

    public static void TriggerRestEnter()
    {
        OnRestEnter.ProcessModifications(0);
        OnRestEnterSimple.OnNext(Unit.Default);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log("[SafeEvent] RestEnter triggered");
        #endif
    }

    public static int TriggerRest(int baseRestAmount)
    {
        var result = OnRest.ProcessModifications(baseRestAmount);
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] Rest: {baseRestAmount} → {result}");
        #endif
        return result;
    }

    // ===== モディファイア管理メソッド =====

    public static void RegisterCoinGainModifier(IModifier<int> modifier)
    {
        OnCoinGain.AddModifier(modifier);
    }

    public static void RegisterCoinConsumeModifier(IModifier<int> modifier)
    {
        OnCoinConsume.AddModifier(modifier);
    }

    public static void RegisterPlayerAttackModifier(IModifier<AttackData> modifier)
    {
        OnPlayerAttack.AddModifier(modifier);
    }

    public static void RegisterPlayerDamageModifier(IModifier<int> modifier)
    {
        OnPlayerDamage.AddModifier(modifier);
    }

    public static void RegisterPlayerHealModifier(IModifier<int> modifier)
    {
        OnPlayerHeal.AddModifier(modifier);
    }

    public static void RegisterBallCreateModifier(IModifier<BallData> modifier)
    {
        OnBallCreate.AddModifier(modifier);
    }

    public static void RegisterRestModifier(IModifier<int> modifier)
    {
        OnRest.AddModifier(modifier);
    }

    // オーナー指定でのモディファイア削除
    public static void RemoveModifiersFor(object owner)
    {
        OnCoinGain.RemoveModifiersFor(owner);
        OnCoinConsume.RemoveModifiersFor(owner);
        OnPlayerAttack.RemoveModifiersFor(owner);
        OnPlayerDamage.RemoveModifiersFor(owner);
        OnPlayerHeal.RemoveModifiersFor(owner);
        OnBallCreate.RemoveModifiersFor(owner);
        OnBattleStart.RemoveModifiersFor(owner);
        OnEnemyDefeated.RemoveModifiersFor(owner);
        OnBallMerged.RemoveModifiersFor(owner);
        OnShopEnter.RemoveModifiersFor(owner);
        OnRestEnter.RemoveModifiersFor(owner);
        OnRest.RemoveModifiersFor(owner);
        
        #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
        Debug.Log($"[SafeEvent] Removed all modifiers for: {owner?.GetType().Name}");
        #endif
    }

    // 全イベントのリセット
    public static void ResetAllEvents()
    {
        OnCoinGain.Clear();
        OnCoinConsume.Clear();
        OnPlayerAttack.Clear();
        OnPlayerDamage.Clear();
        OnPlayerHeal.Clear();
        OnBallCreate.Clear();
        OnBattleStart.Clear();
        OnEnemyDefeated.Clear();
        OnBallMerged.Clear();
        OnShopEnter.Clear();
        OnRestEnter.Clear();
        OnRest.Clear();
        
        Debug.Log("[SafeEventManager] All events reset");
    }

    // デバッグ用：全モディファイアの情報表示
    #if UNITY_EDITOR
    public static void DebugPrintAllModifiers()
    {
        Debug.Log("=== SafeEventManager Modifiers ===");
        Debug.Log($"CoinGain: {OnCoinGain.GetModifiers().Count} modifiers");
        Debug.Log($"CoinConsume: {OnCoinConsume.GetModifiers().Count} modifiers");
        Debug.Log($"PlayerAttack: {OnPlayerAttack.GetModifiers().Count} modifiers");
        Debug.Log($"PlayerDamage: {OnPlayerDamage.GetModifiers().Count} modifiers");
        Debug.Log("=====================================");
    }
    #endif
}