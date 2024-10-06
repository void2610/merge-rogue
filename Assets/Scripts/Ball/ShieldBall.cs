using UnityEngine;

public class ShieldBall : BallBase
{
    protected override void Awake()
    {
        base.Awake();
        ballName = "ヒールボール";
        description = "消すと回復する";
        rarity = BallRarity.Rare;
        size = 1;
        attack = 1;
    }

    protected override void Effect()
    {
        base.Effect();
        // TODO: シールド
    }
}
