using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 敵の出現数、出現データ、難易度設定を統合管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "EnemySpawnConfiguration", menuName = "ScriptableObjects/EnemySpawnConfiguration")]
public class EnemySpawnConfiguration : ScriptableObject
{
    [Serializable]
    public class EnemySpawnData
    { 
        public EnemyData enemyData;
        public float probability;
    }
    
    [Serializable]
    public class EnemySpawnDataList
    { 
        public List<EnemySpawnData> list;
    }

    [System.Serializable]
    public enum GrowthCurveType
    {
        Linear,       // 線形増加
        Logarithmic,  // 対数的増加（序盤急速、後半緩やか）
        Exponential,  // 指数的増加（序盤緩やか、後半急速）
        Custom        // AnimationCurveで自由設定
    }
    
    [System.Serializable]
    public enum RandomType
    {
        Fixed,        // 固定値のランダム
        Percentage    // パーセンテージベースのランダム
    }
    
    [Header("敵データ")]
    [SerializeField] private List<EnemySpawnDataList> enemyList;
    
    [Header("ボスデータ")]
    [SerializeField] private List<EnemySpawnDataList> bossList;
    
    [Header("敵難易度倍率設定（バランス調整用）")]
    [SerializeField] private float baseEnemyHealthMultiplier = 1.0f;
    [SerializeField] private float baseEnemyAttackMultiplier = 1.0f;
    [SerializeField] private float globalEnemyDifficultyMultiplier = 1.0f;
    
    [Header("出現数カーブ設定")]
    [SerializeField] private GrowthCurveType growthCurveType = GrowthCurveType.Linear;
    [SerializeField, Range(0f, 2f)] private float growthRate = 0.3f;
    [SerializeField, Range(0.5f, 3f)] private float growthPower = 1.5f;
    [SerializeField] private int stageOffset = 0;
    [SerializeField] private AnimationCurve customGrowthCurve = AnimationCurve.Linear(0, 0, 10, 5);
    
    [Header("ランダム設定")]
    [SerializeField] private RandomType randomType = RandomType.Fixed;
    [SerializeField, Range(0f, 3f)] private float randomAmplitude = 1f;
    
    [Header("Act倍率設定")]
    [SerializeField] private AnimationCurve actMultiplierCurve = AnimationCurve.Linear(0, 1f, 5, 2f);
    
    [Header("制限設定")]
    [SerializeField] private int minSpawnCount = 1;
    [SerializeField] private int maxSpawnCount = 6;
    
    // プロパティ
    public List<EnemySpawnDataList> EnemyList => enemyList;
    public List<EnemySpawnDataList> BossList => bossList;
    public float BaseEnemyHealthMultiplier => baseEnemyHealthMultiplier;
    public float BaseEnemyAttackMultiplier => baseEnemyAttackMultiplier;
    public float GlobalEnemyDifficultyMultiplier => globalEnemyDifficultyMultiplier;
    public GrowthCurveType CurveType => growthCurveType;
    public float GrowthRate => growthRate;
    public float GrowthPower => growthPower;
    public int StageOffset => stageOffset;
    public AnimationCurve CustomGrowthCurve => customGrowthCurve;
    public RandomType RandomMode => randomType;
    public float RandomAmplitude => randomAmplitude;
    public AnimationCurve ActMultiplierCurve => actMultiplierCurve;
    public int MinSpawnCount => minSpawnCount;
    public int MaxSpawnCount => maxSpawnCount;
    
    /// <summary>
    /// Actに対応する倍率を取得
    /// </summary>
    public float GetActMultiplier(int act)
    {
        return actMultiplierCurve.Evaluate(act);
    }
    
    /// <summary>
    /// 指定されたアクトの敵出現データリストを取得
    /// </summary>
    /// <param name="spawnLists">敵出現データリスト</param>
    /// <param name="act">アクト番号</param>
    /// <returns>対応する敵出現データリスト</returns>
    public EnemySpawnDataList GetEnemySpawnDataListForAct(List<EnemySpawnDataList> spawnLists, int act)
    {
        if (spawnLists == null || spawnLists.Count == 0)
            return new EnemySpawnDataList { list = new List<EnemySpawnData>() };
        
        // TODO: 将来的にアクトベースの選択を実装
        // 現在は最初のリストを返す
        var index = Mathf.Clamp(0, 0, spawnLists.Count - 1);
        return spawnLists[index];
    }
    
}