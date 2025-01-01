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
            var width = MergeManager.Instance.Wall.WallWidth;
            var r = GameManager.Instance.RandomRange(-width / 2 + 0.1f, width / 2 - 0.1f);
            var p = new Vector3(r, 0.8f, 0);
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
