public class CriticalBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        
        var critical = 1.0f;
        if (GameManager.Instance.RandomRange(0.0f, 1.0f) < 0.33f)
        {
            critical = 3.0f;
            SeManager.Instance.PlaySe("levelUp");
        }
        else
        {
            DefaultMergeParticle();
        }
        MergeManager.Instance.Attack(AttackType.Normal, Attack * Rank * critical, this.transform.position);
    }
}
