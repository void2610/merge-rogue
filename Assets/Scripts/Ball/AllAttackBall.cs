public class AllAttackBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.Attack(AttackType.All, Attack * Rank, this.transform.position);
    }
}
