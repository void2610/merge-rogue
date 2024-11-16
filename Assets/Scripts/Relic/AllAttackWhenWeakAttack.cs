using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AllAttackWhenWeakAttack : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;
        disposable = EventManager.OnPlayerAttack.Subscribe(Effect);
    }

    public void RemoveEffect()
    {
        disposable.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var x = EventManager.OnPlayerAttack.GetValue();
        if (x.Item1 < 10)
        {
            EventManager.OnPlayerAttack.SetValue((x.Item1, true));
            ui?.ActivateUI();
        }
    }
}
