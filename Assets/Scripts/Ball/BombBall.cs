using UnityEngine;

public class BombBall : BallBase
{
    private CircleCollider2D circleCollider2D;

    protected override void Effect(BallBase other)
    {
        // 全体攻撃しつつ、周りのボールを消す
        base.Effect(other);
        
        MergeManager.Instance.AddAllAttackCount(Attack * Level, this.transform.position);

        var hitColliders = Physics2D.OverlapCircleAll(this.transform.position, Size);

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
            if (ball.IsFrozen || ball.isDestroyed) continue;
            
            ball.isDestroyed = true;
            ball.EffectAndDestroy(this);
        }
        
        // TODO: 爆発エフェクトを追加
        DefaultMergeParticle();
    }
}
