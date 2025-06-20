using System;
using UnityEngine;

public class DisturbBall : BallBase
{
    private IDisposable _disposable;
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        base.InitBall(d, rank, level);
        this.isMergable = false;
        _disposable = SubscribeNearbyMerge(_ => this.EffectAndDestroy(null), 1f);
    }
    
    private void OnDestroy()
    {
        _disposable?.Dispose();
    }
}
