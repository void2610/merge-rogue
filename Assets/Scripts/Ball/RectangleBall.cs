public class RectangleBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddAttackCount(AttackType.Normal, Attack * Rank, this.transform.position);
    }
}
