using UnityEngine;

public class SmallBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();

        size = 0.75f;
        attack = 0.5f;
    }
    
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
    }
}
