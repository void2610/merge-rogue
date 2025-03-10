public class TriangleBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddAttackCount(AttackType.Random, Attack * Rank, this.transform.position);
    }
}
