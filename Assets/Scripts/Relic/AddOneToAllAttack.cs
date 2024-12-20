using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AddOneToAllAttack : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;    
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        disposable = EventManager.OnPlayerAttack.Subscribe(Effect).AddTo(this);
        ui = relicUI;
    }

    public void RemoveEffect()
    {
        disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var x = EventManager.OnPlayerAttack.GetValue();
        EventManager.OnPlayerAttack.SetValue((x.Item1 + 1, x.Item2));
        ui?.ActivateUI();
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}