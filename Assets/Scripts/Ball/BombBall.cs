using UnityEngine;

public class BombBall : BallBase
{
    private CircleCollider2D circleCollider2D;
    protected override void Awake()
    {
        base.Awake();
        
        size = 1.25f;
        attack = 2;
    }

    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        // 全体攻撃しつつ、周りのボールを消す

        var hitColliders = Physics2D.OverlapCircleAll(this.transform.position, size);

        // 取得したコライダーをリストに変換
        for(var i = 0; i < hitColliders.Length; i++)
        {
            var col = hitColliders[i];
            // 自身を無視する処理を追加
            if (col.gameObject == this.gameObject) continue;
            // merge相手を無視する処理を追加
            if (col.gameObject == other.gameObject) continue;
            
            var ball = col.gameObject.GetComponent<BallBase>();
            if (ball == null) continue;
            if (ball.isFrozen || ball.isDestroyed) continue;
            
            Debug.Log($"Hit object: {col.gameObject.name}");
            ball.EffectAndDestroy(this);
        }
        
        // TODO: 爆発エフェクトを追加
        DefaultMergeParticle();
    }
}
