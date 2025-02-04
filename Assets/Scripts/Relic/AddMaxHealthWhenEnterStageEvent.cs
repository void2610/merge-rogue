using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AddMaxHealthWhenEnterStageEvent : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnEventStageEnter.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        GameManager.Instance.Player.MaxHealth.Value += 10;
        UI?.ActivateUI();
    }
}
