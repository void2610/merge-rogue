using UnityEngine;

public class CriticalBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();

        size = 1.0f;
        attack = 1;
    }

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
}
