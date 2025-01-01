using UnityEngine;

public class HealBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        
        DefaultMergeParticle();
        GameManager.Instance.player.Heal(this.Level);
    }
}
