using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class SometimeCopyDropBall : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnBallDrop.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var r = GameManager.Instance.RandomRange(0.0f, 1.0f);
        if (r < 0.5f)
        {
            var level = MergeManager.Instance.CurrentBall.GetComponent<BallBase>().Level;
            var p = new Vector3(GameManager.Instance.RandomRange(-1f, 1f), 0.8f, 0);
            MergeManager.Instance.SpawnBallFromLevel(level, p, Quaternion.identity);
            UI?.ActivateUI();
        }
    }
}
