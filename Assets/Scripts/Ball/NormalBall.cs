using UnityEngine;

public class NormalBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();

        size = 1;
        attack = 1;
    }
    
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
    }
}
