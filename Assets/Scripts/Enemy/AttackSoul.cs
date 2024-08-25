using UnityEngine;
using DG.Tweening;

public class AttackSoul : EnemyBase
{
    private bool Wait(Player player)
    {
        return false;
    }

    protected override void Awake()
    {
        enemyName = "あかい\nタマシイ";
        hMax = 6;
        hMin = 4;
        attack = 3;
        defense = 0;
        gold = 3;
        exp = 20;
        enemyActions.Add(new AttackData
        {
            name = "まつ",
            action = Wait,
            probability = 0.3f,
            color = Color.black,
            description = "何もしない",
        });

        base.Awake();

        // ゆらゆらと動かす
        transform.DOLocalMoveY(0.2f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
