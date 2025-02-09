using System.Collections.Generic;
using UnityEngine;

public class AllAttackBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddAllAttackCount(Attack * Rank, this.transform.position);
    }
}
