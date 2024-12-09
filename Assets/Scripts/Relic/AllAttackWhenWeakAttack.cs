using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AllAttackWhenWeakAttack : MonoBehaviour, IRelicBehavior
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
        disposable.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var x = EventManager.OnPlayerAttack.GetValue();
        if (x.Item1 + x.Item2 < 10)
        {
            // 全体攻撃に変換
            EventManager.OnPlayerAttack.SetValue((0, x.Item1 + x.Item2));
            ui?.ActivateUI();
        }
    }
}
