using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class Fukiya : MonoBehaviour, IRelicBehavior
{
    private IDisposable _disposable;
    private RelicUI _ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        _disposable = EventManager.OnOrganise.Subscribe(Effect).AddTo(this);
        _ui = relicUI;
    }

    public void RemoveEffect()
    {
        _disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        GameManager.Instance.Player.TakeDamage(10);
        _ui?.ActivateUI();
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}