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
        });
    }

    /// <summary>
    /// イベント購読を管理リストに追加
    /// </summary>
    protected void AddSubscription(IDisposable subscription)
    {
        _simpleSubscriptions.Add(subscription);
    }
}

