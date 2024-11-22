using UnityEngine;

public class BigBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();

        size = 1.5f;
        attack = 2;
    }

    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
    }
}
