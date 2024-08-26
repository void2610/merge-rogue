using UnityEngine;
using DG.Tweening;

public class AttackSoul1 : EnemyBase
{
    private bool SlimeAttack(Player player)
    {
        return true;
    }

    protected override void Awake()
    {
        enemyName = "きいろい\nタマシイ";
        hMax = 15;
        hMin = 12;
        attack = 1;
        defense = 0;
        gold = 3;
        exp = 20;
        enemyActions.Add(new AttackData
        {
            name = "すいとる",
            action = SlimeAttack,
            probability = 0.2f,
            color = Color.blue,
            description = "ためた値を1へらす",
        });

        base.Awake();

        // ゆらゆらと動かす
        transform.DOLocalMoveY(0.2f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
