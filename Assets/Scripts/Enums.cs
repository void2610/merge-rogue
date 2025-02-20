using System.Collections.Generic;
using UnityEngine;

public enum CursorType
{
    Default,
    Select,
    Help,
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Boss
}

public enum StageType
{
    Enemy,
    Shop,
    Treasure,
    Rest,
    Events,
    Boss,
    Undefined
}

public enum AttackType
{
    Normal,
    All,
    Random,
    First,
    Last,
    Second,
    Third,
}

public static class MyEnumUtil
{
    /// <summary>
    /// 攻撃タイプに紐付く色を取得する
    /// </summary>
    public static Color GetColor(this AttackType type)
    {
        return type switch
        {
            AttackType.Normal => Color.white,
            AttackType.All => Color.red,
            AttackType.Random => Color.green,
            AttackType.First => Color.blue,
            AttackType.Last => Color.yellow,
            AttackType.Second => Color.magenta,
            AttackType.Third => Color.cyan,
            _ => Color.white
        };
    }
    
    /// <summary>
    /// ボールの色を保持する辞書
    /// </summary>
    private static readonly List<Color> _ballColors = new()
    {
        new Color(0.5f,0.5f,0.5f),
        Color.green,
        Color.blue,
        Color.magenta,
        Color.yellow,
        Color.red,
        Color.cyan,
        Color.black
    };
    
    /// <summary>
    /// レアリティに対応する色を取得する
    /// </summary>
    public static Color GetColor(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => new Color(0.8f, 0.8f, 0.8f),      // グレー
            Rarity.Uncommon => new Color(0.3f, 1f, 0.3f),      // 緑
            Rarity.Rare => new Color(0.2f, 0.4f, 1f),          // 青
            Rarity.Epic => new Color(0.7f, 0.3f, 0.9f),        // 紫
            Rarity.Legendary => new Color(1f, 0.8f, 0f),       // 金
            Rarity.Boss => new Color(1f, 0.1f, 0.1f),           // 赤
            _ => Color.white
        };
    }
    
    /// <summary>
    /// ボールの色を取得する
    /// </summary>
    /// <param name="level">ボールのレベル</param>
    /// <returns>ボールの色</returns>
    public static Color GetBallColor(int level)
    {
        if (level < 0 || level >= _ballColors.Count)
        {
            return Color.white;
        }
        return _ballColors[level];
    }
}