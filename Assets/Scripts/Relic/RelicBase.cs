using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public abstract class RelicBase : MonoBehaviour
{
    protected readonly List<IDisposable> Disposables = new();
    protected RelicUI UI;
    protected bool IsCountable = false;
    protected readonly ReactiveProperty<int> Count = new(0);

    public virtual void Init(RelicUI relicUI)
    {
        UI = relicUI;
        UI.EnableCount(IsCountable);
        UI.SubscribeCount(Count);
        
        SubscribeEffect();
    }
    
    protected abstract void SubscribeEffect();

    protected abstract void EffectImpl(Unit _);
    
    public virtual void RemoveEffect()
    {
        foreach (var disposable in Disposables) disposable?.Dispose();
    }

    private void OnDestroy()
    {
        RemoveEffect();
    }
}