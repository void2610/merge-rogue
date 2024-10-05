using UnityEngine;
using DG.Tweening;

public class Boss : EnemyBase
{
    private bool CallSoul(Player player)
    {
        GameManager.Instance.enemyContainer.SpawnEnemyByBoss(1);
        return false;
    }

    protected virtual bool PiercingAttack(Player player)
    {
        player.TakeDamage(attack / 2);
        return true;
    }

    protected override void Awake()
    {
        enemyName = "???";
        hMax = 100;
        hMin = 80;
        attack = 5;
        defense = 0;
        gold = 0;
        exp = 0;
        enemyActions.Add(new AttackData
        {
            name = "かんつう\nこうげき",
            action = PiercingAttack,
            probability = 0.5f,
            color = Color.yellow,
            description = "ためることができない\n" + (attack / 2) + "ダメージ",
        });
        enemyActions.Add(new AttackData
        {
            name = "よぶ",
            action = CallSoul,
            probability = 0.5f,
            //水色 #82dcff
            color = new Color(0.509f, 0.863f, 1f),
            description = "タマシイをよぶ",
        });

        base.Awake();

        // ゆらゆらと動かす
        transform.DOLocalMoveY(0.2f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
