using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

/// <summary>
/// InventoryManagerから抽出したサービス実装
/// MonoBehaviourの依存関係を除去し、純粋なサービスとして機能
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly InventoryManager _inventoryManager;
    
    public InventoryService(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager;
    }
    
    // プロパティの委譲
    public int InventorySize => _inventoryManager.InventorySize;
    public bool IsFull => _inventoryManager.IsFull;
    public InventoryUI InventoryUI => _inventoryManager.InventoryUI;
    public List<float> Sizes => _inventoryManager.Sizes;
    
    // メソッドの委譲
    public void UpgradeBall(int index) => _inventoryManager.UpgradeBall(index);
    public void AddBall(BallData data) => _inventoryManager.AddBall(data);
    public void ReplaceBall(BallData data, int rank) => _inventoryManager.ReplaceBall(data, rank);
    public UniTask SwapBall(int index1, int index2) => _inventoryManager.SwapBall(index1, index2);
    public void RemoveAndShiftBall(int index) => _inventoryManager.RemoveAndShiftBall(index);
    
    public GameObject GetSpecialBallByClassName(string ballClassName, int rank) => 
        _inventoryManager.GetSpecialBallByClassName(ballClassName, rank);
    public GameObject GetBallByRank(int rank) => _inventoryManager.GetBallByRank(rank);
    public GameObject GetRandomBall(Vector3 position = default) => _inventoryManager.GetRandomBall(position);
    
    public BallData GetBallData(int index) => _inventoryManager.GetBallData(index);
    public int GetBallLevel(int index) => _inventoryManager.GetBallLevel(index);
}