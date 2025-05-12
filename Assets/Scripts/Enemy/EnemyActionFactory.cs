using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class EnemyActionAttribute : Attribute { }

[Serializable]
public class EnemyActionData
{
    public string name;
    public ActionType type;
    public Action Action;
}

public static class EnemyActionFactory
{
    private static readonly Dictionary<string, Func<EnemyBase, int, EnemyActionData>> _actionMap;

    static EnemyActionFactory()
    {
        // メソッドを取得して、EnemyActionAttributeが付いているものをフィルタリング
        // それをDictionaryに変換
        _actionMap = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t == typeof(EnemyActionFactory))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<EnemyActionAttribute>() != null)
            .ToDictionary(
                m => m.Name.Replace("Action", ""), // メソッド名から ID を抽出
                m => (Func<EnemyBase, int, EnemyActionData>)Delegate.CreateDelegate(
                    typeof(Func<EnemyBase, int, EnemyActionData>), m));
        
        foreach (var action in _actionMap)
        {
            Debug.Log($"Action: {action.Key}");
        }
    }

    /// <summary>
    /// EnemyActionDataを名前から生成する
    /// </summary>
    public static EnemyActionData CreateActionByName(string name, EnemyBase self, int value)
    {
        if (_actionMap.TryGetValue(name, out var func))
            return func(self, value);

        throw new ArgumentException($"Unknown action: {name}");
    }

    // ────────────────────────────────────────────────
    
    [EnemyAction]
    public static EnemyActionData NormalAttackAction(EnemyBase self, int damage)
    {
        return new EnemyActionData
        {
            name = "通常攻撃",
            type = ActionType.Attack,
            Action = () => { GameManager.Instance.Player.Damage(damage); }
        };
    }
    
    [EnemyAction]
    public static EnemyActionData ShieldAction(EnemyBase self, int stack)
    {
        return new EnemyActionData
        {
            name = "シールド",
            type = ActionType.Buff,
            Action = () => { StatusEffectFactory.AddStatusEffect(self, StatusEffectType.Shield, stack); }
        };
    }
    
    [EnemyAction]
    public static EnemyActionData SpawnAction(EnemyBase self, int stage)
    {
        return new EnemyActionData
        {
            name = "増援",
            type = ActionType.Buff,
            Action = () => { EnemyContainer.Instance.SpawnEnemy(1, stage); }
        };
    }
    
    [EnemyAction]
    public static EnemyActionData SelfHealAction(EnemyBase self, int heal)
    {
        return new EnemyActionData
        {
            name = "自己回復",
            type = ActionType.Heal,
            Action = () => { self.Heal(heal); }
        };
    }
    
    [EnemyAction]
    public static EnemyActionData AllHealAction(EnemyBase self, int heal)
    {
        return new EnemyActionData
        {
            name = "全体回復",
            type = ActionType.Heal,
            Action = () => { EnemyContainer.Instance.HealAllEnemies(heal); }
        };
    }
    
    // [EnemyAction]
    // public static EnemyActionData HealAction(EnemyBase self, int index, int heal)
    // {
    //     return new EnemyActionData
    //     {
    //         name = "回復",
    //         type = ActionType.Heal,
    //         Action = () => { EnemyContainer.Instance.HealEnemy(index, heal); }
    //     };
    // }
    
    [EnemyAction]
    public static EnemyActionData AllDamageAction(EnemyBase self, int damage)
    {
        return new EnemyActionData
        {
            name = "全体自傷",
            type = ActionType.SelfDamage,
            Action = () => { EnemyContainer.Instance.DamageAllEnemies(damage); }
        };
    }
    
    [EnemyAction]
    public static EnemyActionData SelfDamageAction(EnemyBase self, int damage)
    {
        return new EnemyActionData
        {
            name = "自傷",
            type = ActionType.SelfDamage,
            Action = () => { self.Damage(damage); }
        };
    }
    
}
