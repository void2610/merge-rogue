using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class DoubleCoinsWhenLowHealth : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;    
    public void ApplyEffect()
    {
        disposable = EventManager.OnCoinGain.Subscribe(Effect);
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
            Debug.Log($"DoubleCoinsWhenLowHealth: Effect {x} -> {x * 2}");
        }
    }
}
