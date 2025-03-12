using UnityEngine;

public class EnemyActions
{
    public static EnemyBase.ActionData ShieldAction(EnemyBase self, int stack)
    {
        return new EnemyBase.ActionData
        {
            type = ActionType.Buff,
            Action = () => { StatusEffectFactory.AddStatusEffect(self, StatusEffectType.Shield, stack); }
        };
    }
    
    public static EnemyBase.ActionData SpawnAction(EnemyBase self, int stage)
    {
        return new EnemyBase.ActionData
        {
            type = ActionType.Buff,
            Action = () => { EnemyContainer.Instance.SpawnEnemy(1, stage); }
        };
    }
    
    public static EnemyBase.ActionData SelfHealAction(EnemyBase self, int heal)
    {
        return new EnemyBase.ActionData
        {
            type = ActionType.Heal,
            Action = () => { self.Heal(heal); }
        };
    }
    
    public static EnemyBase.ActionData AllHealAction(EnemyBase self, int heal)
    {
        return new EnemyBase.ActionData
        {
            type = ActionType.Heal,
            Action = () => { EnemyContainer.Instance.HealAllEnemies(heal); }
        };
    }
    
    public static EnemyBase.ActionData HealAction(EnemyBase self, int index, int heal)
    {
        return new EnemyBase.ActionData
        {
            type = ActionType.Heal,
            Action = () => { EnemyContainer.Instance.HealEnemy(index, heal); }
        };
    }
    
    public static EnemyBase.ActionData AllDamageAction(EnemyBase self, int damage)
    {
        return new EnemyBase.ActionData
        {
            type = ActionType.Damage,
            Action = () => { EnemyContainer.Instance.DamageAllEnemies(damage); }
        };
    }
    
    public static EnemyBase.ActionData SelfDamageAction(EnemyBase self, int damage)
    {
        return new EnemyBase.ActionData
        {
            type = ActionType.Damage,
            Action = () => { self.Damage(damage); }
        };
    }
    
}
