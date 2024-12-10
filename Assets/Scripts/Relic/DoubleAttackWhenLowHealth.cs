using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class DoubleAttackWhenLowHealth : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;
        disposable = EventManager.OnPlayerAttack.Subscribe(Effect).AddTo(this);
    }

    public void RemoveEffect()
    {
        disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        if (GameManager.Instance.player.health.Value <= 20)
        {
            var atk = EventManager.OnPlayerAttack.GetValue();
            EventManager.OnPlayerAttack.SetValue((atk.Item1 * 2, atk.Item2 * 2));
            ui?.ActivateUI();
        }
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
