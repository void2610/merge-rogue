public class ShockBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        var type = Level < 2 ? AttackType.Normal : AttackType.All;
        MergeManager.Instance.AddAttackCount(type, Attack * Rank, this.transform.position);
        
        var max = Level < 2 ? 2 : 4;
        var count = 0;
        var hitBalls = Utils.GetNearbyBalls(this.gameObject, other.gameObject, Size);
        // 取得したボールを破壊
        foreach (var ball in hitBalls)
        {
            ball.isDestroyed = true;
            ball.EffectAndDestroy(this);
            count++;
            if (count >= max) break;
        }
        ParticleManager.Instance.ThunderParticle(this.transform.position);
        SeManager.Instance.PlaySe("shock");
    }
}
