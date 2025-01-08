using System;
using UnityEngine;


public abstract class StatusEffectBase
{
    public StatusEffectType Type { get; }
    public int StackCount { get; protected set; }
    private readonly bool _isPermanent;

    protected StatusEffectBase(StatusEffectType type, int initialStack, bool isPermanent = false)
    {
        this.Type = type;
        StackCount = initialStack;
        this._isPermanent = isPermanent;
    }

    public void AddStack(int count)
    {
        StackCount += count;
    }

    public bool ReduceStack()
    {
        if (_isPermanent && StackCount > 0) return false;
        StackCount--;
        return StackCount <= 0;
    }
    
    public abstract void ApplyEffect(IEntity target);
    
    public virtual int ModifyDamage(int incomingDamage)
    {
        return incomingDamage;
    }
}

public static class StatusEffectFactory
{
    public static void AddStatusEffect(IEntity target, StatusEffectType type, int initialStack = 1)
    {
        StatusEffectBase newEffect = type switch
        {
            StatusEffectType.Burn => new BurnEffect(initialStack),
            StatusEffectType.Regeneration => new RegenerationEffect(initialStack),
            StatusEffectType.Shield => new ShieldEffect(initialStack),
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

public class ShieldEffect : StatusEffectBase
{
    public ShieldEffect(int initialStack) : base(StatusEffectType.Shield, initialStack, true) { }

    public override void ApplyEffect(IEntity target) { }
    
    public override int ModifyDamage(int incomingDamage)
    {
        if (StackCount <= 0) return incomingDamage;

        // ダメージを吸収
        var absorbed = Math.Min(StackCount, incomingDamage);
        StackCount -= absorbed;

        Debug.Log($"Shield absorbed {absorbed} damage. Remaining shield: {StackCount}.");
        return incomingDamage - absorbed;
    }
}