using UnityEngine;

public class HealBall : BallBase
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
        GameManager.Instance.player.Heal(this.level);
    }
}
