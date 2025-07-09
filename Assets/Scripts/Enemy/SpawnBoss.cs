using UnityEngine;

public class SpawnBoss : EnemyBase
{
    protected override EnemyActionData GetNextAction()
    {
        switch (RandomService.RandomRange(0, 2))
        {
            case 0:
                return EnemyActionFactory.AllHealAction(this, (int)Stage+1);
            case 1:
                return EnemyActionFactory.SpawnAction(this, Stage);
            default:
                return EnemyActionFactory.AllHealAction(this, (int)Stage+1);
        }
    }
}
