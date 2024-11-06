using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class AddOneToAllAttack : MonoBehaviour, IRelicBehavior
{
    public IRelicBehavior.EffectTiming timing => IRelicBehavior.EffectTiming.OnPlayerAttack;
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
        // var health = GameManager.Instance.player.health.Value;
        // if(health < 40)
        // {
        //     EventManager.OnCoinGain.UpdateValue(x * 2);
        // }
    }
}