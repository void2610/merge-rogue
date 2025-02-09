using UnityEngine;

public class ShieldBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        
        DefaultMergeParticle();
        StatusEffectFactory.AddStatusEffect(GameManager.Instance.Player, StatusEffectType.Shield, this.Rank);
    }
}
