using System;
using System.Collections.Generic;
using UnityEngine;
using R3;
using VContainer;

/// <summary>
/// レリックベースクラス
/// イベントプロセッサーを使用してゲーム値を変更
/// IDisposableパターンでリソース管理を行う
/// </summary>
public abstract class RelicBase : IDisposable
{
    // UI関連
    protected RelicUI UI;
    protected bool IsCountable;
    protected readonly ReactiveProperty<int> Count = new(0);
    
    // イベント購読管理
    protected readonly List<IDisposable> SimpleSubscriptions = new();

    // ライフサイクル管理
    private bool _isDisposed;
    
    // 依存性注入されたサービス
    protected IRandomService RandomService;
    protected IContentService ContentService;
    protected IInventoryService InventoryService;
    public RelicService RelicService { get; private set; }
    
    /// <summary>
    /// 依存性注入メソッド
    /// </summary>
    public void InjectDependencies(IRandomService randomService, IContentService contentService, IInventoryService inventoryService, RelicService relicService)
    {
        RandomService = randomService;
        ContentService = contentService;
        InventoryService = inventoryService;
        RelicService = relicService;
    }

    /// <summary>
    /// UIを設定する（後から設定可能）
    /// </summary>
    /// <param name="relicUI">関連付けるRelicUI</param>
    public void SetUI(RelicUI relicUI)
    {
        UI = relicUI;
        UI?.EnableCount(IsCountable);
        UI?.SubscribeCount(Count);
    }

    // レリック固有の効果登録（派生クラスで実装）
    public abstract void RegisterEffects();

    // 全ての効果を削除
    public virtual void RemoveAllEffects()
    {
        if (_isDisposed) return;

        // 登録されたモディファイアを全て削除
        EventManager.RemoveProcessorsFor(this);
        
        // シンプルなイベント購読も削除
        foreach (var subscription in SimpleSubscriptions)
        {
            subscription?.Dispose();
        }
        SimpleSubscriptions.Clear();
    }

    // IDisposable実装
    public void Dispose()
    {
        if (_isDisposed) return;
        
        RemoveAllEffects();
        
        // ReactivePropertyも解放
        Count?.Dispose();
        
        _isDisposed = true;
    }

    // ===== 便利メソッド =====

    /// <summary>
    /// UIをアクティブ化（エフェクト発動時の視覚的フィードバック）
    /// </summary>
    protected void ActivateUI()
    {
        UI?.ActivateUI();
    }

    /// <summary>
    /// ダメージ蓄積カウンター（特殊なパターン用）
    /// </summary>
    protected void RegisterDamageAccumulator(int threshold, Action onThresholdReached)
    {
        RelicHelpers.RegisterPlayerDamageModifier(this, current =>
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
        });
    }

    /// <summary>
    /// イベント購読を管理リストに追加
    /// </summary>
    protected void AddSubscription(IDisposable subscription)
    {
        SimpleSubscriptions.Add(subscription);
    }
    
    // ===== 便利な条件チェックメソッド =====
    
    /// <summary>
    /// プレイヤーのHP条件をチェック（以下）
    /// </summary>
    protected bool IsPlayerHealthBelow(float healthPercentage)
    {
        if (!GameManager.Instance?.Player) return false;
        var currentHealth = GameManager.Instance.Player.Health.Value;
        var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
        return currentHealth <= maxHealth * healthPercentage;
    }
    
    /// <summary>
    /// プレイヤーのHP条件をチェック（以上）
    /// </summary>
    protected bool IsPlayerHealthAbove(float healthPercentage)
    {
        if (!GameManager.Instance?.Player) return false;
        var currentHealth = GameManager.Instance.Player.Health.Value;
        var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
        return currentHealth > maxHealth * healthPercentage;
    }
    
    /// <summary>
    /// ゲーム状態をチェック
    /// </summary>
    protected bool IsGameState(params GameManager.GameState[] states)
    {
        return GameManager.Instance && Array.Exists(states, state => GameManager.Instance.state == state);
    }
    
    /// <summary>
    /// 他のレリックを所持しているかチェック
    /// </summary>
    protected bool HasRelic<T>() where T : RelicBase
    {
        return RelicService?.HasRelic(typeof(T)) ?? false;
    }
}

