using UnityEngine;

public class BombBall : BallBase
{
    private GameObject _fireParticle;
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        useIcon = false;
        base.InitBall(d, rank, level);
        // ボールの輪郭を消して爆弾っぽく見せる

        _fireParticle = ParticleManager.Instance.GetBombFireParticle();
        _fireParticle.transform.SetParent(this.transform);
        _fireParticle.transform.localPosition = new Vector3(0.2f, 0.8f, 0);
    }
    
    protected override void Effect(BallBase other)
    {
        // 全体攻撃しつつ、周りのボールを消す
        base.Effect(other);
        
        MergeManager.Instance.AddAttackCount(AttackType.All, Attack * Rank, this.transform.position);

        var hitBalls = Utils.GetNearbyBalls(this.gameObject, other.gameObject, Size);

        // 取得したボールを破壊
        foreach (var ball in hitBalls)
        {
            ball.isDestroyed = true;
            ball.EffectAndDestroy(this);
        }
        
        // TODO: 爆発エフェクトを追加
        SeManager.Instance.PlaySe("bomb");
    }
}
