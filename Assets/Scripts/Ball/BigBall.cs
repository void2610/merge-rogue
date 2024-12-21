using System.Collections.Generic;
using UnityEngine;

public class BigBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        DefaultMergeParticle();
        MergeManager.Instance.AddSingleAttackCount(attack * level, this.transform.position);
    }
    
    public override void AltFire(int enemyCount, float playerAttack)
    {
        // 一番前とその後ろの敵だけ攻撃
        var isAttacks = new List<bool>(new bool[enemyCount]);
        if (enemyCount > 0)
            isAttacks[0] = true;
        if (enemyCount > 1)
            isAttacks[1] = true;
        GameManager.Instance.enemyContainer.AttackEnemyBySkill((int)(level * playerAttack), isAttacks);
        
        base.AltFire(enemyCount, playerAttack);
    }
}
