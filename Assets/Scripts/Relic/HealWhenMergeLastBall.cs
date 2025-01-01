using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class HealWhenMergeLastBall : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;
        disposable = EventManager.OnBallMerged.Subscribe(Effect).AddTo(this);
    }

    public void RemoveEffect()
    {
        disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var maxLevel = InventoryManager.Instance.InventorySize;
        if (EventManager.OnBallMerged.GetValue() == maxLevel)
        {
            int heal = GameManager.Instance.player.MaxHealth.Value / 4;
            GameManager.Instance.player.Heal(heal);
            ui?.ActivateUI();
        }
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
