public class PassiveAttackBall : BallBase
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
        
        // 一番前の敵を攻撃
        var enemies = EnemyContainer.Instance.GetAllEnemies();
        if (enemies == null || enemies.Count == 0) return;
        
        enemies[0].Damage(AttackType.Normal, (int)(Attack * Rank));
        ParticleManager.Instance.MergeBallIconParticle(this.transform.position, this.Size, this.Data.sprite);
    } 
}
