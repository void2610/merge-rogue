using System;
using UnityEngine;


public abstract class StatusEffectBase
{
    public StatusEffectType Type { get; }
    public int StackCount { get; protected set; }
    private readonly bool _isPermanent;
    private bool _isPlayer = false;
    private Vector3 _entityPosition;

    protected StatusEffectBase(StatusEffectType type, int initialStack, bool isPermanent = false)
    {
        this.Type = type;
        StackCount = initialStack;
        this._isPermanent = isPermanent;
    }
    
    public void SetEntityPosition(Vector3 position, bool isPlayer = false)
    {
        _entityPosition = position;
        _isPlayer = isPlayer;
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
    
    // ターン経過時の処理、スタック数が0になったらtrueを返す
    public abstract void OnTurnEnd(IEntity target);
    
    public virtual int ModifyDamage(int incomingDamage)
    {
        return incomingDamage;
    }
    
    // 戦闘終了時の処理、スタック数が0になったらtrueを返す
    public virtual bool OnBattleEnd()
    {
        StackCount = 0;
        return true; 
    }

    protected void ShowEffectText()
    {
        var effectText = Type switch
        {
            StatusEffectType.Burn => "Burn",
            StatusEffectType.Regeneration => "Regeneration",
            StatusEffectType.Shield => "Guard",
            _ => throw new ArgumentException("Invalid StatusEffectType")
        };
        
        var textColor = Type switch
        {
            StatusEffectType.Burn => new Color(1, 0.4f, 0),
            StatusEffectType.Regeneration => new Color(0, 1, 0.3f),
            StatusEffectType.Shield => new Color(0, 0.8f, 1f),
            _ => throw new ArgumentException("Invalid StatusEffectType")
        };
        
        var isP = _isPlayer ? 1 : -1;
        ParticleManager.Instance.WavyText(effectText, _entityPosition + new Vector3(0.8f * isP, 0.5f, 0), textColor);
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

// 毎ターン、スタック数に応じたダメージを受ける
public class BurnEffect : StatusEffectBase
{
    public BurnEffect(int initialStack) : base(StatusEffectType.Burn, initialStack, false) { }

    public override void OnTurnEnd(IEntity target)
    {
        var damage = StackCount;
        SeManager.Instance.PlaySe("playerAttack");
        target.Damage(damage);
        ShowEffectText();
        SeManager.Instance.PlaySe("burn");
    }
}

// 毎ターン、スタック数に応じてHPを回復する
public class RegenerationEffect : StatusEffectBase
{
    public RegenerationEffect(int initialStack) : base(StatusEffectType.Regeneration, initialStack, false) { }

    public override void OnTurnEnd(IEntity target)
    {
        var heal = StackCount;
        target.Heal(heal);
        ShowEffectText();
    }
}

// ダメージを吸収する、スタック数はダメージを受けるたびに減少
public class ShieldEffect : StatusEffectBase
{
    public ShieldEffect(int initialStack) : base(StatusEffectType.Shield, initialStack, true) { }

    public override void OnTurnEnd(IEntity target) { }
    
    public override int ModifyDamage(int incomingDamage)
    {
        if (StackCount <= 0) return incomingDamage;

        var absorbed = Math.Min(StackCount, incomingDamage);
        StackCount -= absorbed;

        ShowEffectText();
        SeManager.Instance.PlaySe("shield");
        return incomingDamage - absorbed;
    }
}

// (敵専用)スタックがある限り行動できない
public class FreezeEffect : StatusEffectBase
{
    public FreezeEffect(int initialStack) : base(StatusEffectType.Freeze, initialStack, false) { }

    public override void OnTurnEnd(IEntity target) { }
}

// スタックがある限り無敵
public class InvincibleEffect : StatusEffectBase
{
    public InvincibleEffect(int initialStack) : base(StatusEffectType.Invincible, initialStack, false) { }

    public override void OnTurnEnd(IEntity target) { }
    
    public override int ModifyDamage(int incomingDamage)
    {
        // TODO: シールドよりも優先度が高いので、シールドの効果を無視する
        ShowEffectText();
        SeManager.Instance.PlaySe("shield");
        return 0;
    }
}

// (敵専用)毎ターン、スタック数に応じたダメージを敵全体に受ける
public class ShockEffect : StatusEffectBase
{
    public ShockEffect(int initialStack) : base(StatusEffectType.Shock, initialStack, false) { }

    public override void OnTurnEnd(IEntity target)
    {
        var damage = StackCount;
        EnemyContainer.Instance.DamageAllEnemies(damage);
        SeManager.Instance.PlaySe("enemyAttack");
        ShowEffectText();
    }
}