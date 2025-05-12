using UnityEngine;

public class SpawnBoss : EnemyBase
{
    // protected override EnemyActionData GetNextAction()
    // {
    //     var r = GameManager.Instance.RandomRange(0, 2);
    //     switch (r)
    //     {
    //         case 0:
    //             return EnemyActionFactory.AllHealAction(this, (int)Stage+1);
    //         case 1:
    //             return EnemyActionFactory.SpawnAction(this, Stage);
    //         default:
    //             return EnemyActionFactory.AllHealAction(this, (int)Stage+1);
    //     }
    // }
}
