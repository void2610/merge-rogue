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
        // ランダムな色を設定
        color = new Color(Random.value, Random.value, Random.value);
    }
}
