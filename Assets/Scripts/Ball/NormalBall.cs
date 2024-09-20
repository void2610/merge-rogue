using UnityEngine;

public class NormalBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();
        ballName = "ふつうのボール";
        description = "ふつう";
        rarity = BallRarity.Common;
        size = 1;
        attack = 1;
    }
}
