using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class CreateBombWhenDamage : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    private int damageCount = 0;
    public void ApplyEffect(RelicUI relicUI)
    {
        disposable = EventManager.OnPlayerDamage.Subscribe(Effect).AddTo(this);
        ui = relicUI;

        Effect(Unit.Default);
    }

    public void RemoveEffect()
    {
        disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var x = EventManager.OnPlayerDamage.GetValue();
        damageCount += x;

        var isActivated = false;
        while (damageCount >= 20)
        {
            damageCount -= 20;
            var p = new Vector3(GameManager.Instance.RandomRange(-1f, 1f), 0.8f, 0);
            MergeManager.Instance.CreateBomb(p);
            isActivated = true;
        }
    
        if (isActivated) ui?.ActivateUI();
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
