using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class HaisuiNoJin : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnEnemyDefeated.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);

        var max = GameManager.Instance.Player.MaxHealth.Value;
        GameManager.Instance.Player.MaxHealth.Value = max / 4;
    }

    protected override void EffectImpl(Unit _)
    {   
        StatusEffectFactory.AddStatusEffect(GameManager.Instance.Player, StatusEffectType.Invincible, 1);
        UI?.ActivateUI();
    }
}
