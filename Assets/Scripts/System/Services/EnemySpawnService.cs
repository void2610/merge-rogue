using UnityEngine;
using VContainer;

/// <summary>
/// 敵の出現数を計算するサービス
/// </summary>
public class EnemySpawnService
{
    private readonly IRandomService _randomService;
    private readonly EnemySpawnConfiguration _configuration;
    
    [Inject]
    public EnemySpawnService(IRandomService randomService, EnemySpawnConfiguration configuration)
    {
        _randomService = randomService;
        _configuration = configuration;
    }
    
    public int CalculateEnemySpawnCount(int stage, int act)
    {
        // 最初のステージは特別扱い
        if (stage == 0)
        {
            return _configuration.FirstStageSpawnCount;
        }
        
        // ステージ進行度を計算
        float stageProgress = (stage + _configuration.StageOffset) * _configuration.GrowthRate;
        
        // 成長カーブに基づいて値を計算
        float curveValue = _configuration.CurveType switch
        {
            EnemySpawnConfiguration.GrowthCurveType.Linear => stageProgress,
            EnemySpawnConfiguration.GrowthCurveType.Logarithmic => Mathf.Log(1f + stageProgress * 2f),
            EnemySpawnConfiguration.GrowthCurveType.Exponential => Mathf.Pow(stageProgress, _configuration.GrowthPower),
            EnemySpawnConfiguration.GrowthCurveType.Custom => _configuration.CustomGrowthCurve.Evaluate(stage),
            _ => stageProgress
        };
        
        // 基本出現数を計算
        int baseCount = Mathf.FloorToInt(_configuration.BaseSpawnCount + curveValue);
        
        // ランダムボーナスを追加
        int randomBonus = 0;
        if (_configuration.RandomMode == EnemySpawnConfiguration.RandomType.Fixed)
        {
            // 固定値のランダム
            int randomRange = Mathf.FloorToInt(_configuration.RandomAmplitude);
            randomBonus = _randomService.RandomRange(-randomRange, randomRange + 1);
        }
        else
        {
            // パーセンテージベースのランダム
            float percentage = _configuration.RandomAmplitude * 0.1f; // 0.1 = 10%
            randomBonus = _randomService.RandomRange(0, Mathf.FloorToInt(baseCount * percentage) + 1);
        }
        
        // Act倍率を適用
        float actMultiplier = _configuration.GetActMultiplier(act);
        int totalCount = Mathf.FloorToInt((baseCount + randomBonus) * actMultiplier);
        
        // 制限内に収める
        return Mathf.Clamp(totalCount, _configuration.MinSpawnCount, _configuration.MaxSpawnCount);
    }
    
    public int GetFirstStageSpawnCount()
    {
        return _configuration.FirstStageSpawnCount;
    }
}