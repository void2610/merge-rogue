using UnityEngine;

public class BigBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();
        ballName = "デカいボール";
        description = "攻撃力が高い";
        rarity = BallRarity.Common;
        size = 1.5f;
        attack = 2;
    }
}
