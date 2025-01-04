using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class DoubleAttackWhenLowHealth : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        if (GameManager.Instance.Player.Health.Value <= GameManager.Instance.Player.MaxHealth.Value * 0.2f)
        {
            var atk = EventManager.OnPlayerAttack.GetValue();
            EventManager.OnPlayerAttack.SetValue((atk.Item1 * 2, atk.Item2 * 2));
            UI?.ActivateUI();
        }
    }
}
