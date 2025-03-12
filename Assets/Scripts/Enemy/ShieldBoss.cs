using UnityEngine;

public class ShieldBoss : EnemyBase
{
    private bool _isUsedShield = false;
    private const int SHIELD_STACK = 3;
    private int _shieldStack;
    private ActionData _shieldAction;
    protected override ActionData GetNextAction()
    {
        if (_isUsedShield)
        {
            _isUsedShield = false;
            return NormalAttack;
        }
        
        _isUsedShield = true;
        return _shieldAction;
    }
    
    public override void Init(int stage)
    {
        _shieldStack = (int) ((stage + 1) * 0.6f * SHIELD_STACK);
        _shieldAction = new ActionData
        {
            type = ActionType.Buff,
            Action = () => { StatusEffectFactory.AddStatusEffect(this, StatusEffectType.Shield, _shieldStack); }
        };
        
        base.Init(stage);
    }
}
