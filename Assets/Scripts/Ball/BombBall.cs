using UnityEngine;

public class BombBall : BallBase
{
    private GameObject _fireParticle;
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        base.InitBall(d, rank, level);
        // ボールの輪郭を消して爆弾っぽく見せる
        var icon = this.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite;
        this.GetComponent<SpriteRenderer>().sprite = icon;
        _fireParticle = ParticleManager.Instance.GetBombFireParticle();
        _fireParticle.transform.SetParent(this.transform);
        _fireParticle.transform.localPosition = new Vector3(0.2f, 0.8f, 0);
    }
    
    protected override void Effect(BallBase other)
    {
        // 全体攻撃しつつ、周りのボールを消す
        base.Effect(other);
        
        MergeManager.Instance.AddAllAttackCount(Attack * Rank, this.transform.position);

        var hitColliders = Physics2D.OverlapCircleAll(this.transform.position, Size);

        // 取得したコライダーをリストに変換
        foreach (var col in hitColliders)
        {
            // 自身を無視
            if (col.gameObject == this.gameObject) continue;
            // merge相手を無視
            if (other && col.gameObject == other.gameObject) continue;
            
            var ball = col.gameObject.GetComponent<BallBase>();
            if (ball == null) continue;
            if (ball.IsFrozen || ball.isDestroyed) continue;
            
            ball.isDestroyed = true;
            ball.EffectAndDestroy(this);
        }
        
        // TODO: 爆発エフェクトを追加
        SeManager.Instance.PlaySe("bomb");
    }
}
