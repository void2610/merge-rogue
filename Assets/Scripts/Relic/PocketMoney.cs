using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class PocketMoney : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;
        disposable = EventManager.OnShopEnter.Subscribe(Effect);
    }

    public void RemoveEffect()
    {
        disposable.Dispose();
    }
    
    private void Effect(Unit _)
    {
        GameManager.Instance.AddCoin(10);
        ui?.ActivateUI();
    }
}
