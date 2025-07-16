using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// インベントリシステムの設定を管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "InventoryConfiguration", menuName = "ScriptableObjects/InventoryConfiguration")]
public class InventoryConfiguration : ScriptableObject
{
    [Header("基本設定")]
    [SerializeField] private int maxInventorySize = 10;
    [SerializeField] private int minInventorySize = 1;
    [SerializeField] private int firstInventorySize = 3;
    
    [Header("配置設定")]
    [SerializeField] private Vector3 inventoryPosition = new(5.5f, -1.0f, 0);
    [SerializeField] private float ballSpacing = 0.6f;
    
    [Header("サイズとウェイト設定")]
    [SerializeField] private float[] sizes = {0.5f, 0.7f, 0.9f, 1.1f, 1.3f, 1.5f, 1.7f, 1.9f, 2.1f, 2.3f};
    [SerializeField] private float[] weights = {1.0f, 1.2f, 1.4f, 1.6f, 1.8f, 2.0f, 2.2f, 2.4f, 2.6f, 2.8f};
    [SerializeField] private float[] probabilities = {0.3f, 0.25f, 0.2f, 0.15f, 0.1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
    
    [Header("プレハブ参照")]
    [SerializeField] private GameObject ballBasePrefab;
    [SerializeField] private BallData normalBallData;
    
    [Header("データリスト")]
    [SerializeField] private BallDataList allBallDataList;
    
    [Header("テスト設定")]
    [SerializeField] private List<BallData> testBalls = new(10);
    [SerializeField] private List<RelicData> testRelics = new();
    
    // プロパティアクセス
    public int MaxInventorySize => maxInventorySize;
    public int MinInventorySize => minInventorySize;
    public int FirstInventorySize => firstInventorySize;
    public Vector3 InventoryPosition => inventoryPosition;
    public float BallSpacing => ballSpacing;
    public float[] Sizes => sizes;
    public float[] Weights => weights;
    public float[] Probabilities => probabilities;
    public GameObject BallBasePrefab => ballBasePrefab;
    public BallData NormalBallData => normalBallData;
    public BallDataList AllBallDataList => allBallDataList;
    public List<BallData> TestBalls => testBalls;
    public List<RelicData> TestRelics => testRelics;
    
    /// <summary>
    /// インベントリ位置を計算
    /// </summary>
    /// <param name="index">インベントリインデックス</param>
    /// <returns>計算された位置</returns>
    public Vector3 CalcInventoryPosition(int index)
    {
        return inventoryPosition + new Vector3(index * (ballSpacing + sizes[index] * 0.5f), 0, 0);
    }
    
    /// <summary>
    /// 設定の妥当性をチェック
    /// </summary>
    private void OnValidate()
    {
        // 配列のサイズチェック
        if (sizes.Length != maxInventorySize)
        {
            Debug.LogWarning($"Sizes配列の長さ({sizes.Length})がMaxInventorySize({maxInventorySize})と一致しません");
        }
        
        if (weights.Length != maxInventorySize)
        {
            Debug.LogWarning($"Weights配列の長さ({weights.Length})がMaxInventorySize({maxInventorySize})と一致しません");
        }
        
        if (probabilities.Length != maxInventorySize)
        {
            Debug.LogWarning($"Probabilities配列の長さ({probabilities.Length})がMaxInventorySize({maxInventorySize})と一致しません");
        }
        
        if (testBalls.Count != maxInventorySize)
        {
            // TestBallsのサイズを自動調整
            while (testBalls.Count < maxInventorySize)
                testBalls.Add(null);
            while (testBalls.Count > maxInventorySize)
                testBalls.RemoveAt(testBalls.Count - 1);
        }
        
        // 範囲チェック
        if (firstInventorySize > maxInventorySize)
        {
            Debug.LogWarning($"FirstInventorySize({firstInventorySize})がMaxInventorySize({maxInventorySize})を超えています");
        }
        
        if (minInventorySize > maxInventorySize)
        {
            Debug.LogWarning($"MinInventorySize({minInventorySize})がMaxInventorySize({maxInventorySize})を超えています");
        }
    }
}