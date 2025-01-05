using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class Fukiya : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnOrganise.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        GameManager.Instance.Player.Damage(10);
        UI?.ActivateUI();
    }
}