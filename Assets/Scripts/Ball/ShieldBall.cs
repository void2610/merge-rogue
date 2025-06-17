public class ShieldBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        
        DefaultMergeParticle();
        StatusEffects.AddToPlayer(StatusEffectType.Shield, this.Rank);
    }
}
