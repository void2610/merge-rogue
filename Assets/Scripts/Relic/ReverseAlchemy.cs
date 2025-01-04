using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class ReverseAlchemy : RelicBase
{
    private int _damageCount = 0;
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerDamage.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
        
    protected override void EffectImpl(Unit _)    
    {
        var x = EventManager.OnPlayerDamage.GetValue();
        _damageCount += x;

        var isActivated = false;
        while (_damageCount >= 5)
        {
            _damageCount -= 5;
            GameManager.Instance.AddCoin(1);
            isActivated = true;
        }
        
        if (isActivated) UI?.ActivateUI();
    }
}