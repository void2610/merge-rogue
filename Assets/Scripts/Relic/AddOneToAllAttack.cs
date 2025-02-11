using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AddOneToAllAttack : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var x = EventManager.OnPlayerAttack.GetValue();
        EventManager.OnPlayerAttack.SetValue((x.Item1 + 1, x.Item2));
        UI?.ActivateUI();
    }
}