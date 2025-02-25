using System;
using R3;
using UnityEngine;

public class DisturbBall : BallBase
{
    private IDisposable _disposable;
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        base.InitBall(d, rank, level);
        this.isMergable = false;
        _disposable = EventManager.OnBallMerged.Subscribe(CheckNearMerge).AddTo(this);
    }
    
    private void CheckNearMerge(Unit _)
    {
        // 周りのボールがマージされたら消滅する
        var (b1, b2) = EventManager.OnBallMerged.GetValue();
        var pos = (b2.transform.position - b1.transform.position) / 2;
        var distance = Vector3.Distance(this.transform.position, pos);
        if (distance < 3f)
        {
            this.EffectAndDestroy(null);
        }
    }
    
    private void OnDestroy()
    {
        _disposable?.Dispose();
    }
}
