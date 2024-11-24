using UnityEngine;

public class ShieldBall : BallBase
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
        
        //TODO: シールドを実装する
        
        DefaultMergeParticle();
        MergeManager.Instance.AddSingleAttackCount(attack * level, this.transform.position);
    }
}
