using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class CreateBombWhenDamage : RelicBase
{
    private int _damageCount = 0;
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerDamage.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var x = EventManager.OnPlayerDamage.GetValue();
        _damageCount += x;

        var isActivated = false;
        while (_damageCount >= 20)
        {
            _damageCount -= 20;
            var width = MergeManager.Instance.Wall.WallWidth;
            var r = GameManager.Instance.RandomRange(-width / 2 + 0.1f, width / 2 - 0.1f);
            var p = new Vector3(r, 0.8f, 0);
            MergeManager.Instance.CreateBomb(p);
            isActivated = true;
        }
    
        if (isActivated) UI?.ActivateUI();
    }
}
