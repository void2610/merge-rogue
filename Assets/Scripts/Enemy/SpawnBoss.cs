using UnityEngine;

public class SpawnBoss : EnemyBase
{
    private bool _isUsedSpawn = false;
    private const int SHIELD_STACK = 3;
    private ActionData _spawnAction;
    protected override ActionData GetNextAction()
    {
        if (_isUsedSpawn)
        {
            _isUsedSpawn = false;
            return NormalAttack;
        }
        
        _isUsedSpawn = true;
        return _spawnAction;
    }
    
    public override void Init(int stage)
    {
        _spawnAction = new ActionData
        {
            type = ActionType.Buff,
            Action = () => { EnemyContainer.Instance.SpawnEnemy(1, Stage); }
        };
        
        base.Init(stage);
    }
}
