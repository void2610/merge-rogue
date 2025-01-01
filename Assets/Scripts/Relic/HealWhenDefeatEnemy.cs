using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class HealWhenDefeatEnemy : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;
        disposable = EventManager.OnEnemyDefeated.Subscribe(Effect).AddTo(this);
    }

    public void RemoveEffect()
    {
        disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var enemy = EventManager.OnEnemyDefeated.GetValue();
        if(!enemy) return;
        
        var heal = enemy.MaxHealth * 0.1f;
        GameManager.Instance.player.Heal(Mathf.CeilToInt(heal));
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
