using System;
using UnityEngine;

public interface IStatusEffect
{
    string Name { get; }
    int StackCount { get; }
    void ApplyEffect(IEntity target); // 毎ターンの効果適用
    void AddStack(int count);           // スタックを追加
    bool ReduceStack();                 // スタックを減らし、0になったらtrueを返す
}

public abstract class StatusEffectBase : IStatusEffect
{
    public string Name { get; protected set; }
    public int StackCount { get; private set; }

    public StatusEffectBase(string name, int initialStack)
    {
        Name = name;
        StackCount = initialStack;
    }

    public void AddStack(int count)
    {
        StackCount += count;
    }

    public bool ReduceStack()
    {
        StackCount--;
        return StackCount <= 0;
    }

    public abstract void ApplyEffect(IEntity target);
}

public static class StatusEffectFactory
{
    public static void AddStatusEffect(IEntity target, StatusEffectType type, int initialStack = 1)
    {
        IStatusEffect newEffect = type switch
        {
            StatusEffectType.Burn => new BurnEffect(initialStack),
            StatusEffectType.Regeneration => new RegenerationEffect(initialStack),
            _ => throw new ArgumentException("Invalid StatusEffectType")
        };
        
        target.AddStatusEffect(newEffect);
    }
}

public class BurnEffect : StatusEffectBase
{
    public BurnEffect(int initialStack) : base("Burn", initialStack) { }

    public override void ApplyEffect(IEntity target)
    {
        var damage = StackCount; // スタック数に応じたダメージ
        SeManager.Instance.PlaySe("playerAttack");
        target.Damage(damage);
        Debug.Log($"{target} took {damage} damage from Burn.");
    }
}

public class RegenerationEffect : StatusEffectBase
{
    public RegenerationEffect(int initialStack) : base("Regeneration", initialStack) { }

    public override void ApplyEffect(IEntity target)
    {
        var heal = StackCount; // スタック数に応じた回復
        target.Heal(heal);
        Debug.Log($"{target} healed {heal} HP from Regeneration.");
    }
}
