using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class MonsterEnergy : RelicBase
{
    public override void Init(RelicUI relicUI)
    {
        IsCountable = true;
        Count.Value = 3;
        base.Init(relicUI);
    }
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnRest.Subscribe(EffectImpl).AddTo(this);
        MergeManager.Instance.LevelUpBallAmount();
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        if(Count.Value <= 0) return;
        
        EventManager.OnRest.SetValue(0);
        Count.Value--;
        Debug.Log("MonsterEnergy: " + Count.Value);
        UI?.ActivateUI();
    }
}
