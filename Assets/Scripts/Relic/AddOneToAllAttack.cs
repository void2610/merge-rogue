using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AddOneToAllAttack : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;    
    public void ApplyEffect()
    {
        disposable = EventManager.OnPlayerAttack.Subscribe(Effect);
    }

    public void RemoveEffect()
    {
        disposable.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var x = EventManager.OnPlayerAttack.GetValue();
        EventManager.OnPlayerAttack.SetValue(x + 1);
        Debug.Log($"AddOneToAllAttack: Effect {x} -> {EventManager.OnPlayerAttack.GetValue()}");
    }
}