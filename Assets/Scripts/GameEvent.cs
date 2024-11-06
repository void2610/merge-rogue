using R3;
using System;
using UnityEngine;

public class GameEvent<T>
{
    // イベント発行のためのSubject
    private Subject<Unit> subject = new Subject<Unit>();

    // 複数のデータを保持するための変数
    private readonly T initialValue;     // 初期値
    public T value { get; private set; } // 現在の値

    // コンストラクタで初期値を設定
    public GameEvent(T initialValue)
    {
        this.initialValue = initialValue;
        value = initialValue;
    }

    // イベントを発行
    public void Trigger(T data)
    {
        value = data;
        subject.OnNext(Unit.Default);
    }
    
    public void SetValue(T data)
    {
        value = data;
    }
    
    public T GetValue()
    {
        return value;
    }
    
    public T GetAndResetValue()
    {
        var v = value;
        Reset();
        return v;
    }
    
    public void Reset()
    {
        value = initialValue;
    }

    // 購読機能
    public IDisposable Subscribe(Action<Unit> onEvent)
    {
        return subject.Subscribe(onEvent);
    }
    
    public void ResetAll()
    {
        Reset();
        subject = new Subject<Unit>();
    }
}