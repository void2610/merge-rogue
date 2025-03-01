using UnityEngine;

public class WildBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        if(Attack > 0) DefaultMergeParticle();
        MergeManager.Instance.AddAttackCount(AttackType.Normal, Attack * Rank, this.transform.position);
    }
    
    protected override void HandleCollision(BallBase b)
    {
        if (isDestroyed || IsFrozen || !isMergable) return;
        
        // どのランクのボールともマージ可能
        if (!b.IsFrozen && !b.isDestroyed && b.isMergable)
        {
            // WildBall同士はシリアルが小さい方がマージする。その他のボールは常にWildBallがマージする。
            var isWild = b is WildBall;
            if (isWild && this.Serial < b.Serial || !isWild)
            {
                var pos = (this.transform.position + b.transform.position) / 2;
                EventManager.OnBallMerged.Trigger((this, b));
                
                var center = (this.transform.position + b.transform.position) / 2;
                var rotation = Quaternion.Lerp(this.transform.rotation, b.transform.rotation, 0.5f);
                MergeManager.Instance.SpawnBallFromLevel(NextRank, center, rotation);

                EffectAndDestroy(b);
                b.EffectAndDestroy(this);
            }
        }
    }
}
