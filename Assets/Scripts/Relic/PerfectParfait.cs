using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class PerfectParfait : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {
        var count = MergeManager.Instance.GetBallCount();
        if(count > 0) return;

        var attack = EventManager.OnPlayerAttack.GetValue();
        EventManager.OnPlayerAttack.SetValue((attack.Item1 * 5, attack.Item2 * 5));
        UI?.ActivateUI();
    }
}
