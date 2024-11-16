using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class SometimeCopyDropBall : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;
        disposable = EventManager.OnBallDropped.Subscribe(Effect);
    }

    public void RemoveEffect()
    {
        disposable.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var r = GameManager.Instance.RandomRange(0.0f, 1.0f);
        if (r < 0.5f)
        {
            var x = EventManager.OnBallDropped.GetValue();
            EventManager.OnBallDropped.SetValue(x + 1);
            ui?.ActivateUI();
        }
    }
}
