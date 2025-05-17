using UnityEngine;

public class DelayBombBall : BallBase
{
    private GameObject _fireParticle;
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        base.InitBall(d, rank, level);

        _fireParticle = ParticleManager.Instance.GetBombFireParticle();
        _fireParticle.transform.SetParent(this.transform);
        _fireParticle.transform.localPosition = new Vector3(0.2f, 0.8f, 0);
    }
    
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.Attack(AttackType.Normal, Attack * Rank, this.transform.position);
    }
    
    protected override void TurnEndEffect()
    {
        base.TurnEndEffect();
        if (elapsedTurns < 3) return;
        
        // 全体攻撃しつつ、周りのボールを消す
        MergeManager.Instance.Attack(AttackType.All, Attack * Rank, this.transform.position);

        var hitBalls = Utils.GetNearbyBalls(this.gameObject, Size);
        // 取得したボールを破壊
        foreach (var ball in hitBalls)
        {
            ball.isDestroyed = true;
            ball.EffectAndDestroy(this);
        }
        SeManager.Instance.PlaySe("bomb");
        
        // 自分だけでマージ
        this.EffectAndDestroy(null);
        ParticleManager.Instance.MergeBallIconParticle(this.transform.position, this.Size, this.Data.sprite);
    }
}
