using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class HealWhenMergeLastBall : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnBallMerged.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    
    protected override void EffectImpl(Unit _)
    {
        var maxLevel = InventoryManager.Instance.InventorySize;
        if (EventManager.OnBallMerged.GetValue() == maxLevel)
        {
            int heal = GameManager.Instance.Player.MaxHealth.Value / 4;
            GameManager.Instance.Player.Heal(heal);
            UI?.ActivateUI();
        }
    }
    
   
}
