using UnityEngine;

public class RedBombBall : BallBase
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
        // マージさせると全体攻撃できる
        MergeManager.Instance.Attack(AttackType.All, Attack * Rank, this.transform.position);
    }
    
    protected override void TurnEndEffect()
    {
        base.TurnEndEffect();
        ParticleManager.Instance.MergeBallIconParticle(this.transform.position, this.Size, this.Data.sprite);
        if (elapsedTurns < 3) return;
        
        // 周りのボールを消して、プレイヤーにダメージ
        var hitBalls = Utils.GetNearbyBalls(this.gameObject, Size);
        foreach (var ball in hitBalls)
        {
            ball.DestroyWithNoEffect();
        }
        SeManager.Instance.PlaySe("bomb");
        
        // プレイヤーにダメージ
        GameManager.Instance.Player.Damage(AttackType.Normal, (int)(Attack * Rank));
        // 自分を消す
        isDestroyed = true;
        this.DestroyWithNoEffect();
    }
}