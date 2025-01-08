using System;
using UnityEngine;

public interface IStatusEffect
{
    StatusEffectType Type { get; }
    int StackCount { get; }
    bool isPermanent { get; }
    void ApplyEffect(IEntity target); // 毎ターンの効果適用
    void AddStack(int count);           // スタックを追加
    bool ReduceStack();                 // スタックを減らし、0になったらtrueを返す
}

public abstract class StatusEffectBase : IStatusEffect
{
    public StatusEffectType Type { get; }
    public int StackCount { get; private set; }
    public bool isPermanent { get; }

    public StatusEffectBase(StatusEffectType type, int initialStack, bool isPermanent = false)
    {
        this.Type = type;
        StackCount = initialStack;
        this.isPermanent = isPermanent;
    }

    public void AddStack(int count)
    {
        StackCount += count;
    }

    public bool ReduceStack()
    {
        if (isPermanent) return false;
        StackCount--;
        return StackCount <= 0;
    }

    public abstract void ApplyEffect(IEntity target);
}

public static class StatusEffectFactory
{
    public static void AddStatusEffect(IEntity target, StatusEffectType type, int initialStack = 1)
    {
        StatusEffectBase newEffect = type switch
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
    public BurnEffect(int initialStack) : base(StatusEffectType.Burn, initialStack, false) { }

    public override void ApplyEffect(IEntity target)
    {
        var damage = StackCount;
        SeManager.Instance.PlaySe("playerAttack");
        target.Damage(damage);
    }
}

public class RegenerationEffect : StatusEffectBase
{
    public RegenerationEffect(int initialStack) : base(StatusEffectType.Regeneration, initialStack, false) { }

    public override void ApplyEffect(IEntity target)
    {
        var heal = StackCount;
        target.Heal(heal);
    }
}
