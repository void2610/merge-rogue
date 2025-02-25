public class MagiciansOrb : BallBase
{
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        base.InitBall(d, rank, level);
        // 通常ではマージ不可
        this.isMergable = false;
    }
    
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddAttackCount(AttackType.Random, Attack * Rank, this.transform.position);
    }
}
