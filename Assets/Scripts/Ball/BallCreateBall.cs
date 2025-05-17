public class BallCreateBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        if(Attack > 0) DefaultMergeParticle();
        // マージ時にランダムなボールを1つ生成する。
        MergeManager.Instance.CreateRandomBall();
        // レベル2以上なら2つ生成する。
        if (Level > 0) MergeManager.Instance.CreateRandomBall();
        
        MergeManager.Instance.Attack(AttackType.Normal, Attack * Rank, this.transform.position);
    }
}
