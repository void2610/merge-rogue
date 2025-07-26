using System;
using UnityEngine;

/// <summary>
/// ランダム数値生成を担当するサービス実装クラス
/// GameSettingsServiceから取得したシード値を使用した再現可能なランダム生成を提供
/// </summary>
public class RandomService : IRandomService
{
    public string SeedText { get; private set; }
    
    private System.Random _random;
    private readonly string _seedText;
    private int _seed;
    
    public RandomService(string seedText)
    {
        _seedText = seedText;
        InitializeSeed();
    }
    
    /// <summary>
    /// シード値を初期化する
    /// </summary>
    private void InitializeSeed()
    {
        // シードテキストが空の場合は新規生成
        if (string.IsNullOrEmpty(_seedText))
        {
            var guid = Guid.NewGuid();
            SeedText = guid.ToString("N")[..8];
            _seed = SeedText.GetHashCode();
            
            Debug.Log($"RandomService: Generated new seed: {SeedText}");
        }
        else
        {
            SeedText = _seedText;
            _seed = SeedText.GetHashCode();
            Debug.Log($"RandomService: Using existing seed: {SeedText}");
        }
        
        _random = new System.Random(_seed);
    }
    
    /// <summary>
    /// 指定された範囲内のランダムなfloat値を生成する
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    /// <returns>指定範囲内のランダムfloat値</returns>
    public float RandomRange(float min, float max)
    {
        return (float)(_random.NextDouble() * (max - min) + min);
    }
    
    /// <summary>
    /// 指定された範囲内のランダムなint値を生成する
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値（exclusive）</param>
    /// <returns>指定範囲内のランダムint値</returns>
    public int RandomRange(int min, int max)
    {
        return _random.Next(min, max);
    }
}