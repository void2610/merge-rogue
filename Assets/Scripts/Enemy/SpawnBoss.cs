using UnityEngine;

public class SpawnBoss : EnemyBase
{
    protected override ActionData GetNextAction()
    {
        var r = GameManager.Instance.RandomRange(0, 2);
        switch (r)
        {
            case 0:
                return EnemyActions.AllHealAction(this, (int)Stage+1);
            case 1:
                return EnemyActions.SpawnAction(this, Stage);
            default:
                return EnemyActions.AllHealAction(this, (int)Stage+1);
        }
    }
}
