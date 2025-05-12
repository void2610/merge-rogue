using UnityEngine;

public class ShieldBoss : EnemyBase
{
    private bool _isUsedShield = false;
    private const int SHIELD_STACK = 3;
    private int _shieldStack;
    protected override EnemyActionData GetNextAction()
    {
        if (_isUsedShield)
        {
            _isUsedShield = false;
            return NormalAttack;
        }
        
        _isUsedShield = true;
        return EnemyActionFactory.ShieldAction(this, _shieldStack);
    }
    
    public override void Init(EnemyData d, int stage)
    {
        _shieldStack = (int) ((stage + 1) * 0.6f * SHIELD_STACK);
        base.Init(d, stage);
    }
}
