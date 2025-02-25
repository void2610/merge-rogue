public class SkipBall : BallBase
{
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        base.InitBall(d, rank, level);
        // マージ時にランクが2つ上のボールを生成する。
        if (level < 2)
        {
            NextRank = rank + 2;
        }
        // マージ時にランクが3つ上のボールを生成する。
        else
        {
            NextRank = rank + 3;
        }
    }
    
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddAttackCount(AttackType.Normal, Attack * Rank, this.transform.position);
    }
}
