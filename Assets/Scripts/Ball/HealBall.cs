using UnityEngine;

public class HealBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        
        DefaultMergeParticle();
        GameManager.Instance.player.Heal(this.level);
    }
    
    public override void AltFire(int enemyCount, float playerAttack)
    {
        // 回復
        GameManager.Instance.player.Heal((int)(1 * playerAttack));
        
        base.AltFire(enemyCount, playerAttack);
    }
}
