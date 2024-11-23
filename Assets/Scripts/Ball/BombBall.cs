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
        // 全体攻撃しつつ、周りのボールを消す
        base.Effect(other);
        
        var r = new Vector3(Random.Range(-0.75f, 0.75f), Random.Range(-0.75f, 0.75f), 0);
        MergeManager.Instance.AddAllAttackCount(attack * level, this.transform.position + r);

        var hitColliders = Physics2D.OverlapCircleAll(this.transform.position, size);

        // 取得したコライダーをリストに変換
        // TODO: たまにフリーズする
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
            
            ball.EffectAndDestroy(this);
        }
        
        // TODO: 爆発エフェクトを追加
        DefaultMergeParticle();
    }
}
