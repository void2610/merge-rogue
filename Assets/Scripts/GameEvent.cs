using R3;
using System;
using UnityEngine;

public class GameEvent<T>
{
    // イベント発行のためのSubject
    private Subject<T> _subject = new Subject<T>();

    // 複数のデータを保持するための変数
    private T _initialValue;     // 初期値
    public T value { get; private set; } // 現在の値

    // コンストラクタで初期値を設定
    public GameEvent(T initialValue)
    {
        _initialValue = initialValue;
        value = initialValue;
    }

    // イベントを発行
    public void Trigger(T data)
    {
        Debug.Log($"Trigger: {data}");
        value = data;            // 値を更新
        _subject.OnNext(value);  // イベント発行
    }
    
    public void UpdateValue(T data)
    {
        value = data;
        Debug.Log($"new value: {value}");
    }
    
    public T GetAndReset()
    {
        var v = value;
        Reset();
        return v;
    }

    // イベント発行後に変数を初期値に戻す
    public void Reset()
    {
        value = _initialValue;
    }

    // 購読機能
    public IDisposable Subscribe(Action<T> onEvent)
    {
        return _subject.Subscribe(onEvent);
    }
    
    public void ResetAll()
    {
        Reset();
        _subject = new Subject<T>();
    }
}