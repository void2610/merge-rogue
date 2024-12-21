using System.Collections.Generic;
using UnityEngine;

public class CriticalBall : BallBase
{
    protected override void Effect(BallBase other)
    {
        base.Effect(other);
        
        var critical = 1.0f;
        if (GameManager.Instance.RandomRange(0.0f, 1.0f) < 0.33f)
        {
            critical = 3.0f;
            SeManager.Instance.PlaySe("levelUp");
        }
        else
        {
            DefaultMergeParticle();
        }
        MergeManager.Instance.AddSingleAttackCount(attack * level * critical, this.transform.position);
    }
    
    public override void AltFire(int enemyCount, float playerAttack)
    {
        // 一番後ろの敵だけ攻撃、たまにクリティカル
        var isAttacks = new List<bool>(new bool[enemyCount]);
        if (enemyCount > 0)
            isAttacks[^1] = true;
        var critical = 1.0f;
        if (GameManager.Instance.RandomRange(0.0f, 1.0f) < 0.33f)
        {
            critical = 3.0f;
            SeManager.Instance.PlaySe("levelUp");
        }
        GameManager.Instance.enemyContainer.AttackEnemyBySkill((int)(critical * playerAttack), isAttacks);
        
        base.AltFire(enemyCount, playerAttack);
    }
}
