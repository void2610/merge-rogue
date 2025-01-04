using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class PocketMoney : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnShopEnter.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        GameManager.Instance.AddCoin(10);
        UI?.ActivateUI();
    }
}
