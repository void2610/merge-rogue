using UnityEngine;

public class SmallBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddSingleAttackCount(attack * level, this.transform.position);
    }
}
