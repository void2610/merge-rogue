public class ShieldBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        
        DefaultMergeParticle();
        StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Shield, this.Rank);
    }
}
