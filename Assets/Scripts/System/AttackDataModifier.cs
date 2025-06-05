using System;
using SafeEventSystem;

/// <summary>
/// AttackData用のモディファイア
/// </summary>
public class AttackDataModifier : ModifierBase<AttackData>
{
    private readonly Func<AttackData, AttackData, AttackData> _modifier;
    private readonly Func<bool> _condition;

    public AttackDataModifier(
        Func<AttackData, AttackData, AttackData> modifier,
        object owner,
        Func<bool> condition = null) : base(owner)
    {
        _modifier = modifier;
        _condition = condition ?? (() => true);
    }

    public override bool CanApply(AttackData originalValue, AttackData currentValue)
        => _condition();

    public override AttackData Apply(AttackData originalValue, AttackData currentValue)
    {
        return _modifier(originalValue, currentValue);
    }
}

/// <summary>
/// AttackDataの特定の攻撃タイプに加算するモディファイア
/// </summary>
public class AttackAdditionModifier : ModifierBase<AttackData>
{
    private readonly AttackType _attackType;
    private readonly int _amount;
    private readonly Func<bool> _condition;

    public AttackAdditionModifier(AttackType attackType, int amount, object owner, Func<bool> condition = null)
        : base(owner)
    {
        _attackType = attackType;
        _amount = amount;
        _condition = condition ?? (() => true);
    }

    public override bool CanApply(AttackData originalValue, AttackData currentValue) => _condition();

    public override AttackData Apply(AttackData originalValue, AttackData currentValue)
    {
        return currentValue.AddAttack(_attackType, _amount);
    }
}

/// <summary>
/// AttackDataの全攻撃に乗数を適用するモディファイア
/// </summary>
public class AttackMultiplierModifier : ModifierBase<AttackData>
{
    private readonly float _multiplier;
    private readonly Func<bool> _condition;

    public AttackMultiplierModifier(float multiplier, object owner, Func<bool> condition = null)
        : base(owner)
    {
        _multiplier = multiplier;
        _condition = condition ?? (() => true);
    }

    public override bool CanApply(AttackData originalValue, AttackData currentValue) => _condition();

    public override AttackData Apply(AttackData originalValue, AttackData currentValue)
    {
        return currentValue.Multiply(_multiplier);
    }
}

/// <summary>
/// 攻撃タイプを変換するモディファイア（例：単体→全体攻撃）
/// </summary>
public class AttackConversionModifier : ModifierBase<AttackData>
{
    private readonly AttackType _fromType;
    private readonly AttackType _toType;
    private readonly float _multiplier;
    private readonly Func<bool> _condition;

    public AttackConversionModifier(
        AttackType fromType, 
        AttackType toType, 
        object owner, 
        float multiplier = 1.0f, 
        Func<bool> condition = null)
        : base(owner)
    {
        _fromType = fromType;
        _toType = toType;
        _multiplier = multiplier;
        _condition = condition ?? (() => true);
    }

    public override bool CanApply(AttackData originalValue, AttackData currentValue) 
        => _condition() && currentValue.GetAttack(_fromType) > 0;

    public override AttackData Apply(AttackData originalValue, AttackData currentValue)
    {
        var fromValue = currentValue.GetAttack(_fromType);
        var convertedValue = (int)(fromValue * _multiplier);
        
        return currentValue
            .SetAttack(_fromType, 0)
            .AddAttack(_toType, convertedValue);
    }
}