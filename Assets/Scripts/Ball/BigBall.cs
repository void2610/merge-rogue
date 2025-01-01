using System.Collections.Generic;
using UnityEngine;

public class BigBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddSingleAttackCount(Attack * Level, this.transform.position);
    }
}
