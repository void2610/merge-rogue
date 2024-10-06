using UnityEngine;

public class SmallBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();
        ballName = "チビボール";
        description = "攻撃力が低いが扱いやすい";
        rarity = BallRarity.Common;
        size = 0.75f;
        attack = 0.5f;
    }
}
