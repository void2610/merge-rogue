using UnityEngine;

public class HealBall : BallBase
{
    protected override void Effect()
    {
        GameManager.instance.player.Heal(this.level);
    }
}
