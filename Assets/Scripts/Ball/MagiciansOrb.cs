using UnityEngine;

public class MagiciansOrb : BallBase
{
    public override void InitBall(BallData d, int rank, int level = 0)
    {
        // 通常ではマージ不可
        this.isMergable = false;

        base.InitBall(d, rank, level);
    }
    
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.Attack(AttackType.Random, Attack * Rank, this.transform.position);
    }
}
