using R3;
using System;
using UnityEngine;

public class GameEvent<T>
{
    // イベント発行のためのSubject
    private Subject<T> _subject = new Subject<T>();

    // 複数のデータを保持するための変数
    private T _initialValue;     // 初期値
    public T Value { get; private set; } // 現在の値

    // コンストラクタで初期値を設定
    public GameEvent(T initialValue)
    {
        _initialValue = initialValue;
        Value = initialValue;
    }

    // イベントを発行
    public void Trigger(T data)
    {
        Value = data;            // 値を更新
        _subject.OnNext(Value);  // イベント発行
    }

    // イベント発行後に変数を初期値に戻す
    public void Reset()
    {
        Value = _initialValue;
    }

    // 購読機能
    public IDisposable Subscribe(Action<T> onEvent)
    {
        return _subject.Subscribe(onEvent);
    }
}