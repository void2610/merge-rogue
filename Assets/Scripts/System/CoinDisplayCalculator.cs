using UnityEngine;

/// <summary>
/// コイン表示数の計算を行うユーティリティクラス
/// </summary>
public static class CoinDisplayCalculator
{
    /// <summary>
    /// コイン枚数に応じた適切な表示数を計算
    /// 1-3枚: そのまま表示
    /// 4-10枚: 3-5枚で表示 
    /// 11-50枚: 5-8枚で表示
    /// 51-200枚: 8-12枚で表示
    /// 201枚以上: 12-15枚で表示
    /// </summary>
    /// <param name="totalCoins">実際のコイン枚数</param>
    /// <returns>表示するコインprefabの数</returns>
    public static int CalculateDisplayCount(int totalCoins)
    {
        return totalCoins switch
        {
            <= 3 => totalCoins,
            <= 10 => Mathf.Clamp(totalCoins / 2 + 1, 3, 5),
            <= 50 => Mathf.Clamp(totalCoins / 6 + 2, 5, 8),
            <= 200 => Mathf.Clamp(totalCoins / 20 + 3, 8, 12),
            _ => Mathf.Clamp(totalCoins / 50 + 5, 12, 15)
        };
    }
}