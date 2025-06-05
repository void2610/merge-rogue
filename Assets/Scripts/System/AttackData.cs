using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 攻撃データを値型として表現する構造体
/// Dictionary<AttackType, int>の代替として使用
/// </summary>
[System.Serializable]
public struct AttackData : IEquatable<AttackData>
{
    public int Normal;
    public int All;
    public int Random;
    public int Last;
    public int Second;
    public int Third;

    public AttackData(int normal = 0, int all = 0, int random = 0, int last = 0, int second = 0, int third = 0)
    {
        Normal = normal;
        All = all;
        Random = random;
        Last = last;
        Second = second;
        Third = third;
    }

    /// <summary>
    /// Dictionary<AttackType, int>から変換
    /// </summary>
    public static AttackData FromDictionary(Dictionary<AttackType, int> dictionary)
    {
        if (dictionary == null) return new AttackData();
        
        return new AttackData(
            normal: dictionary.GetValueOrDefault(AttackType.Normal, 0),
            all: dictionary.GetValueOrDefault(AttackType.All, 0),
            random: dictionary.GetValueOrDefault(AttackType.Random, 0),
            last: dictionary.GetValueOrDefault(AttackType.Last, 0),
            second: dictionary.GetValueOrDefault(AttackType.Second, 0),
            third: dictionary.GetValueOrDefault(AttackType.Third, 0)
        );
    }

    /// <summary>
    /// Dictionary<AttackType, int>に変換
    /// </summary>
    public Dictionary<AttackType, int> ToDictionary()
    {
        var result = new Dictionary<AttackType, int>();
        
        if (Normal != 0) result[AttackType.Normal] = Normal;
        if (All != 0) result[AttackType.All] = All;
        if (Random != 0) result[AttackType.Random] = Random;
        if (Last != 0) result[AttackType.Last] = Last;
        if (Second != 0) result[AttackType.Second] = Second;
        if (Third != 0) result[AttackType.Third] = Third;
        
        return result;
    }

    /// <summary>
    /// 指定された攻撃タイプの値を取得
    /// </summary>
    public int GetAttack(AttackType type)
    {
        return type switch
        {
            AttackType.Normal => Normal,
            AttackType.All => All,
            AttackType.Random => Random,
            AttackType.Last => Last,
            AttackType.Second => Second,
            AttackType.Third => Third,
            _ => 0
        };
    }

    /// <summary>
    /// 指定された攻撃タイプの値を設定
    /// </summary>
    public AttackData SetAttack(AttackType type, int value)
    {
        var result = this;
        switch (type)
        {
            case AttackType.Normal: result.Normal = value; break;
            case AttackType.All: result.All = value; break;
            case AttackType.Random: result.Random = value; break;
            case AttackType.Last: result.Last = value; break;
            case AttackType.Second: result.Second = value; break;
            case AttackType.Third: result.Third = value; break;
        }
        return result;
    }

    /// <summary>
    /// 指定された攻撃タイプの値を加算
    /// </summary>
    public AttackData AddAttack(AttackType type, int value)
    {
        return SetAttack(type, GetAttack(type) + value);
    }

    /// <summary>
    /// 全ての攻撃値に乗数を適用
    /// </summary>
    public AttackData Multiply(float multiplier)
    {
        return new AttackData(
            normal: (int)(Normal * multiplier),
            all: (int)(All * multiplier),
            random: (int)(Random * multiplier),
            last: (int)(Last * multiplier),
            second: (int)(Second * multiplier),
            third: (int)(Third * multiplier)
        );
    }

    /// <summary>
    /// 攻撃データが空かどうか
    /// </summary>
    public bool IsEmpty => Normal == 0 && All == 0 && Random == 0 && Last == 0 && Second == 0 && Third == 0;

    /// <summary>
    /// 総攻撃力を取得
    /// </summary>
    public int TotalAttack => Normal + All + Random + Last + Second + Third;

    /// <summary>
    /// 0以外の攻撃タイプの一覧を取得
    /// </summary>
    public IEnumerable<(AttackType type, int value)> GetNonZeroAttacks()
    {
        if (Normal != 0) yield return (AttackType.Normal, Normal);
        if (All != 0) yield return (AttackType.All, All);
        if (Random != 0) yield return (AttackType.Random, Random);
        if (Last != 0) yield return (AttackType.Last, Last);
        if (Second != 0) yield return (AttackType.Second, Second);
        if (Third != 0) yield return (AttackType.Third, Third);
    }

    // 演算子オーバーロード
    public static AttackData operator +(AttackData a, AttackData b)
    {
        return new AttackData(
            a.Normal + b.Normal,
            a.All + b.All,
            a.Random + b.Random,
            a.Last + b.Last,
            a.Second + b.Second,
            a.Third + b.Third
        );
    }

    public static AttackData operator -(AttackData a, AttackData b)
    {
        return new AttackData(
            a.Normal - b.Normal,
            a.All - b.All,
            a.Random - b.Random,
            a.Last - b.Last,
            a.Second - b.Second,
            a.Third - b.Third
        );
    }

    public static AttackData operator *(AttackData a, float multiplier)
    {
        return a.Multiply(multiplier);
    }

    // IEquatable実装
    public bool Equals(AttackData other)
    {
        return Normal == other.Normal &&
               All == other.All &&
               Random == other.Random &&
               Last == other.Last &&
               Second == other.Second &&
               Third == other.Third;
    }

    public override bool Equals(object obj)
    {
        return obj is AttackData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Normal, All, Random, Last, Second, Third);
    }

    public static bool operator ==(AttackData left, AttackData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AttackData left, AttackData right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        var attacks = GetNonZeroAttacks().Select(x => $"{x.type}:{x.value}");
        return string.Join(", ", attacks);
    }
}