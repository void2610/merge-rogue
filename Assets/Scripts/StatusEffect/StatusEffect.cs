using System;
using UnityEngine;

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
    // Poison,
    // Stun,
    // Barrier,
    // Weakness,
    // Drain,  
    // Reflect,
    // Absorb,
    // Counter,
    // Dodge,
}

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
    public virtual void OnTurnEnd(IEntity target) { }
    
    public virtual int ModifyDamage(int incomingDamage)
    {
        return incomingDamage;
    }
    
    public virtual int ModifyAttack(int outgoingAttack)
    {
        return outgoingAttack;
    }
    
    // 戦闘終了時の処理、スタック数が0になったらtrueを返す
    public virtual bool OnBattleEnd()
    {
        StackCount = 0;
        return true; 
    }

    protected void ShowEffectText(int priority = 0)
    {
        var effectText = Type.ToString() + "!";
        var textColor = Type.GetStatusEffectColor();
        
        var isP = _isPlayer ? 1 : -1;
        var offset = new Vector3(-priority * 0.1f, priority * 0.25f, 0);
        ParticleManager.Instance.WavyText(effectText, _entityPosition + new Vector3(0.8f * isP, 0.2f, 0) + offset, textColor);
    }
}

public static class StatusEffectFactory
{
    public static void AddStatusEffectToPlayer(StatusEffectType type, int initialStack = 1)
    {
        EventManager.OnPlayerStatusEffect.Trigger(type);
        AddStatusEffect(GameManager.Instance.Player, type, initialStack);
    }
    
    public static void AddStatusEffect(IEntity target, StatusEffectType type, int initialStack = 1)
    {
        if (target is EnemyBase enemyBase)
        {
            EventManager.OnEnemyStatusEffect.Trigger((enemyBase, type));
            target = enemyBase;
        }

        StatusEffectBase newEffect = type switch
        {
            StatusEffectType.Burn => new BurnEffect(initialStack),
            StatusEffectType.Regeneration => new RegenerationEffect(initialStack),
            StatusEffectType.Shield => new ShieldEffect(initialStack),
            StatusEffectType.Freeze => new FreezeEffect(initialStack),
            StatusEffectType.Invincible => new InvincibleEffect(initialStack),
            StatusEffectType.Shock => new ShockEffect(initialStack),
            StatusEffectType.Power => new PowerEffect(initialStack),
            StatusEffectType.Rage => new RageEffect(initialStack),
            _ => throw new ArgumentException("Invalid StatusEffectType")
        };
        
        target.AddStatusEffect(newEffect);
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
            _ => throw new ArgumentException("Invalid StatusEffectType")
        };
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
    
    public override int ModifyDamage(int incomingDamage)
    {
        if (StackCount <= 0) return incomingDamage;

        var absorbed = Math.Min(StackCount, incomingDamage);
        StackCount -= absorbed;

        ShowEffectText(1);
        SeManager.Instance.PlaySe("shield");
        return incomingDamage - absorbed;
    }
}

// (敵専用)スタックがある限り行動できない
public class FreezeEffect : StatusEffectBase
{
    public FreezeEffect(int initialStack) : base(StatusEffectType.Freeze, initialStack, false) { }
}

// スタックがある限り無敵
public class InvincibleEffect : StatusEffectBase
{
    public InvincibleEffect(int initialStack) : base(StatusEffectType.Invincible, initialStack, false) { }
    
    public override int ModifyDamage(int incomingDamage)
    {
        // TODO: シールドよりも優先度を高くする
        ShowEffectText(1);
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

// スタック数に応じて追加ダメージを与える
public class PowerEffect : StatusEffectBase
{
    public PowerEffect(int initialStack) : base(StatusEffectType.Power, initialStack, true) { }

    public override int ModifyAttack(int outgoingAttack)
    {
        ShowEffectText();
        return outgoingAttack + StackCount;
    }
}

// スタック数に応じて攻撃力に倍率をかける　(1 + 0.1 * n)倍
public class RageEffect : StatusEffectBase
{
    public RageEffect(int initialStack) : base(StatusEffectType.Rage, initialStack, true) { }

    public override int ModifyAttack(int outgoingAttack)
    {
        ShowEffectText();
        return (int)(outgoingAttack * (1 + StackCount * 0.1f));
    }
}