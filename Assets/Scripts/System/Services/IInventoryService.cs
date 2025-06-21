using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// インベントリの管理を行うサービスのインターフェース
/// </summary>
public interface IInventoryService
{
    // プロパティ
    int InventorySize { get; }
    bool IsFull { get; }
    List<float> Sizes { get; }
    
    // UI設定メソッド（循環依存回避）
    void SetInventoryUI(InventoryUI inventoryUI);
    void Initialize();
    
    // UI操作メソッド
    void StartEditSwap();
    void StartEditUpgrade();
    void StartEditReplace(BallData ballData);
    void StartEditRemove();
    void CancelEdit();
    
    
    // ボール管理メソッド
    void UpgradeBall(int index);
    void AddBall(BallData data);
    void ReplaceBall(BallData data, int rank);
    UniTask SwapBall(int index1, int index2);
    void RemoveAndShiftBall(int index);
    
    // ボール取得メソッド
    GameObject GetSpecialBallByClassName(string ballClassName, int rank);
    GameObject GetBallByRank(int rank);
    GameObject GetRandomBall(Vector3 position = default);
    
    // データ取得メソッド
    BallData GetBallData(int index);
    int GetBallLevel(int index);
}