using UnityEngine;

public class AllAttackBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();

        size = 1.5f;
        attack = 0.5f;
    }

    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddAllAttackCount(attack * level, this.transform.position);
    }
}
