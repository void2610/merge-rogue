using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class DoubleAttackWhenLowHealth : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnBattleStart.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        if (GameManager.Instance.Player.Health.Value <= GameManager.Instance.Player.MaxHealth.Value * 0.2f)
        {
            StatusEffectFactory.AddStatusEffect(GameManager.Instance.Player, StatusEffectType.Rage, 10);
            UI?.ActivateUI();
        }
    }
}
