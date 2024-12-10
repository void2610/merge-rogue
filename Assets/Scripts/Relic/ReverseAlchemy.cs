using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class ReverseAlchemy : MonoBehaviour, IRelicBehavior
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

    bool isActivated = false;
    while (damageCount >= 5)
    {
        damageCount -= 5;
        GameManager.Instance.AddCoin(1);
        isActivated = true;
    }
    
    if (isActivated) ui?.ActivateUI();
}
    
private void OnDestroy()
{
    RemoveEffect();
}
}