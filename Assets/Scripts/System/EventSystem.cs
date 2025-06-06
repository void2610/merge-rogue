using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 安全なイベント発行・購読システム
/// スレッドセーフな実装でゲーム内イベントを管理
/// </summary>
    
// 基本的なイベント発行・購読システム
public interface IEventPublisher<T>
{
    void Trigger(T value);
    IDisposable Subscribe(Action<T> callback);
}

// イベント管理クラス
public class GameEvent<T> : IEventPublisher<T>
{
    private readonly List<Action<T>> _callbacks = new();
    private readonly object _lock = new();

    public void Trigger(T value)
    {
        List<Action<T>> callbacksCopy;
        lock (_lock)
        {
            callbacksCopy = new List<Action<T>>(_callbacks);
        }

        foreach (var callback in callbacksCopy)
        {
            try
            {
                callback?.Invoke(value);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in event callback: {e}");
            }
        }
    }

    public IDisposable Subscribe(Action<T> callback)
    {
        if (callback == null) return null;

        lock (_lock)
        {
            _callbacks.Add(callback);
        }

        return new EventSubscription<T>(this, callback);
    }

    internal void Unsubscribe(Action<T> callback)
    {
        lock (_lock)
        {
            _callbacks.Remove(callback);
        }
    }
}

// 購読管理クラス
public class EventSubscription<T> : IDisposable
{
    private readonly GameEvent<T> _gameEvent;
    private readonly Action<T> _callback;
    private bool _disposed = false;

    public EventSubscription(GameEvent<T> gameEvent, Action<T> callback)
    {
        _gameEvent = gameEvent;
        _callback = callback;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _gameEvent?.Unsubscribe(_callback);
            _disposed = true;
        }
    }
}

/// <summary>
/// 関数チェーンによる値変更システム
/// 複数の処理関数を順次適用して値を変換する
/// </summary>
public class ValueProcessor<T>
{
    private readonly List<(object owner, Func<T, T> processor, Func<bool> condition)> _processors = new();
    private readonly object _lock = new();

    /// <summary>
    /// 値を処理して結果を返す
    /// </summary>
    public T Process(T originalValue)
    {
        List<(object owner, Func<T, T> processor, Func<bool> condition)> processorsCopy;
        lock (_lock)
        {
            processorsCopy = new List<(object, Func<T, T>, Func<bool>)>(_processors);
        }

        var currentValue = originalValue;
        foreach (var (owner, processor, condition) in processorsCopy)
        {
            try
            {
                if (condition?.Invoke() ?? true)
                {
                    currentValue = processor(currentValue);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in value processor for {owner}: {e}");
            }
        }

        return currentValue;
    }

    /// <summary>
    /// 値処理関数を追加
    /// </summary>
    public void AddProcessor(object owner, Func<T, T> processor, Func<bool> condition = null)
    {
        if (owner == null || processor == null) return;

        lock (_lock)
        {
            _processors.Add((owner, processor, condition));
        }
    }

    /// <summary>
    /// 特定のオーナーの処理関数をすべて削除
    /// </summary>
    public void RemoveProcessorsFor(object owner)
    {
        lock (_lock)
        {
            _processors.RemoveAll(p => p.owner == owner);
        }
    }

    /// <summary>
    /// すべての処理関数をクリア
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _processors.Clear();
        }
    }

    /// <summary>
    /// デバッグ用：現在の処理関数数を取得
    /// </summary>
    public int ProcessorCount
    {
        get
        {
            lock (_lock)
            {
                return _processors.Count;
            }
        }
    }
}

/// <summary>
/// よく使用される値処理のヘルパー関数
/// </summary>
public static class ValueProcessors
{
    // int型用の処理関数

    /// <summary>
    /// 値に指定した数を加算する処理関数
    /// </summary>
    public static Func<int, int> Add(int amount) => value => value + amount;

    /// <summary>
    /// 値に指定した倍率を掛ける処理関数
    /// </summary>
    public static Func<int, int> Multiply(float multiplier) => value => (int)(value * multiplier);

    /// <summary>
    /// 値を指定した値で上書きする処理関数
    /// </summary>
    public static Func<int, int> Override(int newValue) => _ => newValue;

    /// <summary>
    /// 値を0に設定する処理関数
    /// </summary>
    public static Func<int, int> SetZero() => _ => 0;

    /// <summary>
    /// 値を2倍にする処理関数
    /// </summary>
    public static Func<int, int> Double() => value => value * 2;

}