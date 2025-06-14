using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using R3;

public enum StatusEffectType
{
    Burn,
    Regeneration,
    Shield,
    Freeze,
    Invincible,
    Shock,
    Power,
    Rage,
    Curse,
    Confusion,
    // Stun,
    // Weakness,
    // Drain,  
    // Dodge,
}

public abstract class StatusEffectBase
{
    public enum EffectTiming
    {
        OnTurnEnd,
        OnBattleEnd,
        OnAttack,
        OnDamage,
        Random,
    }
    
    public StatusEffectType Type { get; }
    public int StackCount { get; protected set; }
    private readonly EffectTiming _timing;
    private readonly bool _isPermanent;
    private bool _isPlayer = false;
    protected Vector3 EntityPosition;

    protected StatusEffectBase(StatusEffectType type, int initialStack, EffectTiming timing, bool isPermanent = false)
    {
        this.Type = type;
        StackCount = initialStack;
        _timing = timing;
        this._isPermanent = isPermanent;
    }
    
    public void SetEntityPosition(Vector3 position, bool isPlayer = false)
    {
        EntityPosition = position;
        _isPlayer = isPlayer;
    }

    public void AddStack(int count)
    {
        StackCount += count;
    }

    /// <summary>
    /// ターン終了時にスタック数を1減らす
    /// </summary>
    /// <returns>スタック数が0になったらtrue</returns>
    public bool ReduceStack()
    {
        if (_isPermanent && StackCount > 0) return false;
        StackCount--;
        return StackCount <= 0;
    }
    
    /// <summary>
    /// その他の要因で指定したスタック数を減らす
    /// </summary>
    /// <returns>スタック数が0になったらtrue</returns>
    public bool ReduceStack(int count)
    {
        StackCount -= count;
        return StackCount <= 0;
    }
    
    // ターン経過時の処理、スタック数が0になったらtrueを返す
    public virtual void OnTurnEnd(IEntity target)
    {
        if (_timing == EffectTiming.OnTurnEnd)
        {
            ShowEffectText();
            if(_isPlayer)
                EventManager.OnPlayerStatusEffectTriggered.OnNext(Type);
            else
                EventManager.OnEnemyStatusEffectTriggered.OnNext(Type);
        }
    }
    
    public virtual int ModifyDamage(IEntity target, int incomingDamage)
    {
        if (_timing == EffectTiming.OnDamage)
        {
            ShowEffectText(1);
            if (_isPlayer)
                EventManager.OnPlayerStatusEffectTriggered.OnNext(Type);
            else
                EventManager.OnEnemyStatusEffectTriggered.OnNext(Type);
        }
        return incomingDamage;
    }
    
    public virtual int ModifyAttack(IEntity target, AttackType type, int outgoingAttack)
    {
        if (_timing == EffectTiming.OnAttack)
        {
            ShowEffectText();
            if (_isPlayer)
                EventManager.OnPlayerStatusEffectTriggered.OnNext(Type);
            else
                EventManager.OnEnemyStatusEffectTriggered.OnNext(Type);
        }
        return outgoingAttack;
    }
    
    // 戦闘終了時の処理、スタック数が0になったらtrueを返す
    public virtual bool OnBattleEnd(IEntity target)
    {
        if (_timing == EffectTiming.OnBattleEnd)
        {
            ShowEffectText();
            if (_isPlayer)
                EventManager.OnPlayerStatusEffectTriggered.OnNext(Type);
            else
                EventManager.OnEnemyStatusEffectTriggered.OnNext(Type);
        }
        StackCount = 0;
        return true; 
    }

    protected void ShowEffectText(int priority = 0)
    {
        var effectText = Type.ToString() + "!";
        var textColor = Type.GetStatusEffectColor();
        
        var isP = _isPlayer ? 1 : -1;
        var offset = new Vector3(-priority * 0.1f, priority * 0.25f, 0);
        ParticleManager.Instance.WavyText(effectText, EntityPosition + new Vector3(0.8f * isP, 0.2f, 0) + offset, textColor);
    }
}

public static class StatusEffectFactory
{
    public static void AddStatusEffectToPlayer(StatusEffectType type, int initialStack = 1)
    {
        EventManager.OnPlayerStatusEffectAdded.OnNext(R3.Unit.Default);
        AddStatusEffect(GameManager.Instance.Player, type, initialStack);
    }
    
    public static void AddStatusEffect(IEntity target, StatusEffectType type, int initialStack = 1)
    {
        var tar = target;
        StatusEffectType ty;
        int stack;
        if (target is EnemyBase enemyBase)
        {
            EventManager.OnEnemyStatusEffectAdded.OnNext(type);
            // In the new system, the ProcessModifications call above already handled the modification
            ty = type;
            stack = initialStack;
        }
        else
        {
            // In the new system, the ProcessModifications call above already handled the modification
            ty = type;
            stack = initialStack;
        }

        StatusEffectBase newEffect = ty switch
        {
            StatusEffectType.Burn => new BurnEffect(stack),
            StatusEffectType.Regeneration => new RegenerationEffect(stack),
            StatusEffectType.Shield => new ShieldEffect(stack),
            StatusEffectType.Freeze => new FreezeEffect(stack),
            StatusEffectType.Invincible => new InvincibleEffect(stack),
            StatusEffectType.Shock => new ShockEffect(stack),
            StatusEffectType.Power => new PowerEffect(stack),
            StatusEffectType.Rage => new RageEffect(stack),
            StatusEffectType.Curse => new CurseEffect(stack),
            StatusEffectType.Confusion => new ConfusionEffect(stack),
            _ => throw new ArgumentException("Invalid StatusEffectType")
        };
        
        tar.AddStatusEffect(newEffect);
    }
    
    public static void RemoveStatusEffectFromPlayer(StatusEffectType type, int stack = 1)
    {
        RemoveStatusEffect(GameManager.Instance.Player, type, stack);
    }
    
    public static void RemoveStatusEffect(IEntity target, StatusEffectType type, int stack = 1)
    {
        target.RemoveStatusEffect(type, stack);
    }
    
    // 状態異常の色を取得する拡張メソッド
    public static Color GetStatusEffectColor(this StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Burn => new Color(1, 0.4f, 0),
            StatusEffectType.Regeneration => new Color(0, 1, 0.3f),
            StatusEffectType.Shield => new Color(0, 0.8f, 1f),
            StatusEffectType.Freeze => new Color(0, 0.5f, 1),
            StatusEffectType.Invincible => new Color(1, 1, 0),
            StatusEffectType.Shock => new Color(0.7f, 0, 0.7f),
            StatusEffectType.Power => new Color(1, 0.3f, 0),
            StatusEffectType.Rage => new Color(1, 0.2f, 0.5f),
            StatusEffectType.Curse => new Color(0.3f, 0.1f, 0.3f),
            StatusEffectType.Confusion => new Color(1, 0.8f, 0.2f),
            _ => throw new ArgumentException("Invalid StatusEffectType")
        };
    }
    
    // 状態異常の名前を取得する拡張メソッド
    public static string GetStatusEffectWord(this StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Burn => "火傷",
            StatusEffectType.Regeneration => "再生",
            StatusEffectType.Shield => "シールド",
            StatusEffectType.Freeze => "凍結",
            StatusEffectType.Invincible => "無敵",
            StatusEffectType.Shock => "感電",
            StatusEffectType.Power => "パワー",
            StatusEffectType.Rage => "怒り",
            StatusEffectType.Curse => "呪い",
            StatusEffectType.Confusion => "混乱",
            _ => throw new ArgumentException("Invalid StatusEffectType")
        };
    }
}

// 毎ターン、スタック数に応じたダメージを受ける
public class BurnEffect : StatusEffectBase
{
    public BurnEffect(int initialStack) : base(StatusEffectType.Burn, initialStack, EffectTiming.OnTurnEnd, false) { }

    public override void OnTurnEnd(IEntity target)
    {
        base.OnTurnEnd(target);
        var damage = StackCount;
        SeManager.Instance.PlaySe("playerAttack");
        target.Damage(AttackType.Normal, damage);
        SeManager.Instance.PlaySe("burn");
    }
}

// 毎ターン、スタック数に応じてHPを回復する
public class RegenerationEffect : StatusEffectBase
{
    public RegenerationEffect(int initialStack) : base(StatusEffectType.Regeneration, initialStack, EffectTiming.OnTurnEnd, false) { }

    public override void OnTurnEnd(IEntity target)
    {
        var heal = StackCount;
        target.Heal(heal);
    }
}

// ダメージを吸収する、スタック数はダメージを受けるたびに減少
public class ShieldEffect : StatusEffectBase
{
    public ShieldEffect(int initialStack) : base(StatusEffectType.Shield, initialStack, EffectTiming.OnDamage, true) { }
    
    public override int ModifyDamage(IEntity target, int incomingDamage)
    {
        base.ModifyDamage(target, incomingDamage);
        if (StackCount <= 0) return incomingDamage;

        var absorbed = Math.Min(StackCount, incomingDamage);
        StackCount -= absorbed;

        SeManager.Instance.PlaySe("shield");
        return incomingDamage - absorbed;
    }
}

// スタック数*10%の確率で行動不能、行動不能時にスタック数が半分になる
public class FreezeEffect : StatusEffectBase
{
    public FreezeEffect(int initialStack) : base(StatusEffectType.Freeze, initialStack, EffectTiming.Random, true) { }

    public bool IsFrozen()
    {
        if (StackCount <= 0) return false;
        
        var rand = GameManager.Instance.RandomRange(0.0f, 100.0f);
        if (rand < StackCount * 10)
        {
            StackCount /= 2;
            ShowEffectText();
            SeManager.Instance.PlaySe("freeze");
            return true;
        }
        return false;
    }
}

// スタックがある限り無敵
public class InvincibleEffect : StatusEffectBase
{
    public InvincibleEffect(int initialStack) : base(StatusEffectType.Invincible, initialStack, EffectTiming.OnDamage, false) { }
    
    public override int ModifyDamage(IEntity target, int incomingDamage)
    {
        base.ModifyDamage(target, incomingDamage);
        // TODO: シールドよりも優先度を高くする
        SeManager.Instance.PlaySe("shield");
        return 0;
    }
}

// (敵専用)毎ターン、スタック数に応じたダメージを敵全体に受ける
public class ShockEffect : StatusEffectBase
{
    public ShockEffect(int initialStack) : base(StatusEffectType.Shock, initialStack, EffectTiming.OnTurnEnd, false) { }

    public override void OnTurnEnd(IEntity target)
    {
        base.OnTurnEnd(target);
        var damage = StackCount;
        EnemyContainer.Instance.DamageAllEnemies(damage);
        ParticleManager.Instance.ThunderParticle(EntityPosition + new Vector3(0, 0.3f, 0));
        SeManager.Instance.PlaySe("shock");
    }
}

// スタック数に応じて通常攻撃で追加ダメージを与える
public class PowerEffect : StatusEffectBase
{
    public PowerEffect(int initialStack) : base(StatusEffectType.Power, initialStack, EffectTiming.OnAttack, true) { }

    public override int ModifyAttack(IEntity target, AttackType type, int outgoingAttack)
    {
        base.ModifyAttack(target, type, outgoingAttack);
        if (type == AttackType.Normal)
        {
            return outgoingAttack + StackCount;
        }
        return outgoingAttack;
    }
}

// スタック数に応じて攻撃力に倍率をかける　(1 + 0.1 * n)倍
public class RageEffect : StatusEffectBase
{
    public RageEffect(int initialStack) : base(StatusEffectType.Rage, initialStack, EffectTiming.OnAttack, true) { }

    public override int ModifyAttack(IEntity target, AttackType type, int outgoingAttack)
    {
        base.ModifyAttack(target, type, outgoingAttack);
        var multiplier = (1 + StackCount * 0.1f);
        return (int)(outgoingAttack * multiplier);
    }
}

// (プレイヤー専用)毎ターン、スタック数に応じてお邪魔ボールが降ってくる
public class CurseEffect : StatusEffectBase
{
    public CurseEffect(int initialStack) : base(StatusEffectType.Curse, initialStack, EffectTiming.OnTurnEnd, false) { }
    
    public override void OnTurnEnd(IEntity target)
    {
        base.OnTurnEnd(target);
        var count = StackCount;
        for (var i = 0; i < count; i++)
            MergeManager.Instance.CreateDisturbBall();
    }
}

// (プレイヤー専用)マージ時のカーソル操作が勝手に動く
public class ConfusionEffect : StatusEffectBase
{
    public ConfusionEffect(int initialStack) : base(StatusEffectType.Confusion, initialStack, EffectTiming.OnTurnEnd, false) { }
}