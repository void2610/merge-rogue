using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class IceRing : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {
        var enemies = EnemyContainer.Instance.GetAllEnemies();
        if (enemies.Count == 0) return;
        
        StatusEffectFactory.AddStatusEffect(enemies[0], StatusEffectType.Freeze, 1);
        UI?.ActivateUI();
    }
}