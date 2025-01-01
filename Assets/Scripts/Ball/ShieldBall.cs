using UnityEngine;

public class ShieldBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        
        //TODO: シールドを実装する
        
        DefaultMergeParticle();
        MergeManager.Instance.AddSingleAttackCount(Attack * Level, this.transform.position);
    }
}
