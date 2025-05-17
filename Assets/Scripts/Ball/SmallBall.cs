public class SmallBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        
        // レベル3だけ後ろを攻撃
        var type = Level < 2 ? AttackType.Normal : AttackType.Last;
        MergeManager.Instance.Attack(type, Attack * Rank, this.transform.position);
    }
}