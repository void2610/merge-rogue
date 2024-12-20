using System.Collections.Generic;
using UnityEngine;

public class NormalBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();

        size = 1;
        attack = 1;
    }
    
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddSingleAttackCount(attack * level, this.transform.position);
    }
    
    public override void AltFire(int enemyCount, float playerAttack)
    {
        // 一番前の敵だけ攻撃
        var isAttacks = new List<bool>(new bool[enemyCount]);
        if (enemyCount > 0)
            isAttacks[0] = true;
        GameManager.Instance.enemyContainer.AttackEnemyBySkill((int)(attack * playerAttack), isAttacks);
        
        base.AltFire(enemyCount, playerAttack);
    }
}
