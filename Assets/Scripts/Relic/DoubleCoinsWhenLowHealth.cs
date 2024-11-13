using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class DoubleCoinsWhenLowHealth : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        disposable = EventManager.OnCoinGain.Subscribe(Effect);
        ui = relicUI;
    }

    public void RemoveEffect()
    {
        disposable.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var health = GameManager.Instance.player.health.Value;
        if(health > 40)
        {
            var x = EventManager.OnCoinGain.GetValue();
            EventManager.OnCoinGain.SetValue(x * 2);
            ui.ActivateUI();
        }
    }
}
