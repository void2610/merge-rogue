public class DelayGrowBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.Attack(AttackType.Normal, Attack * Rank, this.transform.position);
    }

    protected override void TurnEndEffect()
    {
        base.TurnEndEffect();
        if (elapsedTurns < 2) return;

        // 1つ上のボールを同じ位置に生成する
        MergeManager.Instance.CreateBall(this.Rank + 1, this.transform.position);
        ParticleManager.Instance.MergeBallIconParticle(this.transform.position, this.Size, this.Data.sprite);
        
        Destroy(this.gameObject);
    }
}
