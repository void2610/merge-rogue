using UnityEngine;
using System;

[Serializable]
public class EnemyActionData
{
    public string name;
    public ActionType type;
    public Action Action;
}

public static class EnemyActionFactory
{
    public static EnemyActionData ShieldAction(EnemyBase self, int stack)
    {
        return new EnemyActionData
        {
            name = "シールド",
            type = ActionType.Buff,
            Action = () => { StatusEffectFactory.AddStatusEffect(self, StatusEffectType.Shield, stack); }
        };
    }
    
    public static EnemyActionData SpawnAction(EnemyBase self, int stage)
    {
        return new EnemyActionData
        {
            name = "増援",
            type = ActionType.Buff,
            Action = () => { EnemyContainer.Instance.SpawnEnemy(1, stage); }
        };
    }
    
    public static EnemyActionData SelfHealAction(EnemyBase self, int heal)
    {
        return new EnemyActionData
        {
            name = "自己回復",
            type = ActionType.Heal,
            Action = () => { self.Heal(heal); }
        };
    }
    
    public static EnemyActionData AllHealAction(EnemyBase self, int heal)
    {
        return new EnemyActionData
        {
            name = "全体回復",
            type = ActionType.Heal,
            Action = () => { EnemyContainer.Instance.HealAllEnemies(heal); }
        };
    }
    
    public static EnemyActionData HealAction(EnemyBase self, int index, int heal)
    {
        return new EnemyActionData
        {
            name = "回復",
            type = ActionType.Heal,
            Action = () => { EnemyContainer.Instance.HealEnemy(index, heal); }
        };
    }
    
    public static EnemyActionData AllDamageAction(EnemyBase self, int damage)
    {
        return new EnemyActionData
        {
            name = "全体攻撃",
            type = ActionType.Damage,
            Action = () => { EnemyContainer.Instance.DamageAllEnemies(damage); }
        };
    }
    
    public static EnemyActionData SelfDamageAction(EnemyBase self, int damage)
    {
        return new EnemyActionData
        {
            name = "自傷",
            type = ActionType.Damage,
            Action = () => { self.Damage(damage); }
        };
    }
    
}
