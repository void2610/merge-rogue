using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class DoubleCoinsWhenLowHealth : MonoBehaviour, IRelicBehavior
{
    public IRelicBehavior.EffectTiming timing => IRelicBehavior.EffectTiming.OnCoinGain;
    public List<IDisposable> disposables { get; set; } = new ();
    
    public void ApplyEffect()
    {
        var d = EventManager.OnCoinGain.Subscribe(Effect);
        disposables.Add(d);
    }

    public void RemoveEffect()
    {
        foreach(var d in disposables)
        {
            d.Dispose();
        }
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
