public class BigBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.Attack(AttackType.Normal, Attack * Rank, this.transform.position);
    }
}
