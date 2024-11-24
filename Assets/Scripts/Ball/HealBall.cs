using UnityEngine;

public class HealBall : BallBase
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
        GameManager.Instance.player.Heal(this.level);
    }
}
