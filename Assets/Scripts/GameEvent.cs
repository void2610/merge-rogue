using R3;
using System;
using UnityEngine;

public class GameEvent<T>
{
    // イベント発行のためのSubject
    private Subject<Unit> _subject = new Subject<Unit>();

    // 複数のデータを保持するための変数
    private readonly T _initialValue;     // 初期値
    private T _value;

    // コンストラクタで初期値を設定
    public GameEvent(T initialValue)
    {
        this._initialValue = initialValue;
        _value = initialValue;
    }

    // イベントを発行
    public void Trigger(T data)
    {
        _value = data;
        _subject.OnNext(Unit.Default);
    }
    
    public void SetValue(T data)
    {
        _value = data;
    }
    
    public T GetValue()
    {
        return _value;
    }
    
    public T GetAndResetValue()
    {
        var v = _value;
        Reset();
        return v;
    }

    private void Reset()
    {
        _value = _initialValue;
    }

    // 購読機能
    public IDisposable Subscribe(Action<Unit> onEvent)
    {
        return _subject.Subscribe(onEvent);
    }
    
    public void ResetAll()
    {
        Reset();
        _subject = new Subject<Unit>();
    }
}