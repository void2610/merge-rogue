using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class DoubleCoinsWhenNearFullHealth : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        disposable = EventManager.OnCoinGain.Subscribe(Effect).AddTo(this);
        ui = relicUI;
    }

    public void RemoveEffect()
    {
        disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var health = GameManager.Instance.Player.Health.Value;
        var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
        if(health > maxHealth * 0.8f)
        {
            var x = EventManager.OnCoinGain.GetValue();
            EventManager.OnCoinGain.SetValue(x * 2);
            ui?.ActivateUI();
        }
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
