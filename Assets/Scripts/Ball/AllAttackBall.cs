using System.Collections.Generic;
using UnityEngine;

public class AllAttackBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddAllAttackCount(attack * level, this.transform.position);
    }
    
    public override void AltFire(int enemyCount, float playerAttack)
    {
        // 全ての敵を攻撃
        var isAttacks = new List<bool>(new bool[enemyCount]);
        for (var i = 0; i < enemyCount; i++)
            isAttacks[i] = true;
        GameManager.Instance.enemyContainer.AttackEnemyBySkill((int)(1 * playerAttack), isAttacks);
        
        base.AltFire(enemyCount, playerAttack);
    }
}
