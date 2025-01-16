using UnityEngine;

public class ShieldBoss : EnemyBase
{
    private bool _isUsedShield = false;
    private const int SHIELD_STACK = 10;
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
    
    public override void Init(float magnification)
    {
        _shieldStack = (int) (magnification * SHIELD_STACK);
        _shieldAction = new ActionData
        {
            type = ActionType.Buff,
            Action = () => { StatusEffectFactory.AddStatusEffect(this, StatusEffectType.Shield, _shieldStack); }
        };
        
        base.Init(magnification);
    }
}
