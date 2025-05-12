using UnityEngine;
using System;

[Serializable]
public class ActionData
{
    public string name;
    public ActionType type;
    public Action Action;
}

public class EnemyActionFactory
{
    public static ActionData ShieldAction(EnemyBase self, int stack)
    {
        return new ActionData
        {
            name = "シールド",
            type = ActionType.Buff,
            Action = () => { StatusEffectFactory.AddStatusEffect(self, StatusEffectType.Shield, stack); }
        };
    }
    
    public static ActionData SpawnAction(EnemyBase self, int stage)
    {
        return new ActionData
        {
            name = "増援",
            type = ActionType.Buff,
            Action = () => { EnemyContainer.Instance.SpawnEnemy(1, stage); }
        };
    }
    
    public static ActionData SelfHealAction(EnemyBase self, int heal)
    {
        return new ActionData
        {
            name = "自己回復",
            type = ActionType.Heal,
            Action = () => { self.Heal(heal); }
        };
    }
    
    public static ActionData AllHealAction(EnemyBase self, int heal)
    {
        return new ActionData
        {
            name = "全体回復",
            type = ActionType.Heal,
            Action = () => { EnemyContainer.Instance.HealAllEnemies(heal); }
        };
    }
    
    public static ActionData HealAction(EnemyBase self, int index, int heal)
    {
        return new ActionData
        {
            name = "回復",
            type = ActionType.Heal,
            Action = () => { EnemyContainer.Instance.HealEnemy(index, heal); }
        };
    }
    
    public static ActionData AllDamageAction(EnemyBase self, int damage)
    {
        return new ActionData
        {
            name = "全体攻撃",
            type = ActionType.Damage,
            Action = () => { EnemyContainer.Instance.DamageAllEnemies(damage); }
        };
    }
    
    public static ActionData SelfDamageAction(EnemyBase self, int damage)
    {
        return new ActionData
        {
            name = "自傷",
            type = ActionType.Damage,
            Action = () => { self.Damage(damage); }
        };
    }
    
}
