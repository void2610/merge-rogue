using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AddMaxHealth : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;

        Effect(Unit.Default);
    }

    public void RemoveEffect()
    {
        if (GameManager.Instance.Player == null) return;
        GameManager.Instance.Player.MaxHealth.Value -= 10;
    }
    
    private void Effect(Unit _)
    {
        GameManager.Instance.Player.MaxHealth.Value += 10;
        ui?.AlwaysActive();
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
