using UnityEngine;
using DG.Tweening;

public class NormalSoul : EnemyBase
{
    private bool Absorb(Player player)
    {
        player.AddSaveFromEnemy(-attack);
        return true;
    }

    protected override void Awake()
    {
        enemyName = "あおい\nタマシイ";
        hMax = 15;
        hMin = 5;
        attack = 1;
        defense = 0;
        gold = 1;
        exp = 15;
        enemyActions.Add(new AttackData
        {
            name = "すいとる",
            action = Absorb,
            probability = 0.2f,
            color = Color.blue,
            description = "ためた値を1へらす",
        });

        base.Awake();

        // ゆらゆらと動かす
        transform.DOLocalMoveY(0.2f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
