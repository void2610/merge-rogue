using System;
using UnityEngine;

/// <summary>
/// 何もしないアクション
/// </summary>
[Serializable]
public class DoNothingAction : StageEventActionBase
{
    public override void Execute()
    {
    }
}

/// <summary>
/// コイン追加アクション
/// </summary>
[Serializable]
public class AddCoinAction : StageEventActionBase
{
    [SerializeField] private int amount = 50;
    
    public override void Execute()
    {
        GameManager.Instance.AddCoin(amount);
    }
}

/// <summary>
/// コイン減少アクション
/// </summary>
[Serializable]
public class SubCoinAction : StageEventActionBase
{
    [SerializeField] private int amount = 50;
    
    public override void Execute()
    {
        GameManager.Instance.SubCoin(amount);
    }
    
    public override bool CanExecute()
    {
        return GameManager.Instance.Coin.Value >= amount;
    }
}

/// <summary>
/// 体力回復アクション
/// </summary>
[Serializable]
public class HealAction : StageEventActionBase
{
    [SerializeField] private int amount = 5;
    
    public override void Execute()
    {
        GameManager.Instance.Player.Heal(amount);
    }
    
    public override bool CanExecute()
    {
        var player = GameManager.Instance.Player;
        return player.Health.Value < player.MaxHealth.Value;
    }
}

/// <summary>
/// ダメージアクション
/// </summary>
[Serializable]
public class DamageAction : StageEventActionBase
{
    [SerializeField] private int amount = 10;
    
    public override void Execute()
    {
        GameManager.Instance.Player.Damage(AttackType.Normal, amount);
    }
    
    public override bool CanExecute()
    {
        var player = GameManager.Instance.Player;
        return player.Health.Value > 0;
    }
}

/// <summary>
/// ボール追加アクション
/// </summary>
[Serializable]
public class AddBallAction : StageEventActionBase
{
    [SerializeField] private BallData ballData;
    
    public override void Execute()
    {
        GameManager.Instance.InventoryService.AddBall(ballData);
    }
    
    public override bool CanExecute()
    {
        return !GameManager.Instance.InventoryService.IsFull;
    }
}

/// <summary>
/// ランダムボール削除アクション
/// </summary>
[Serializable]
public class RemoveRandomBallAction : StageEventActionBase
{
    public override void Execute()
    {
        var inventoryService = GameManager.Instance.InventoryService;
        var idx = UnityEngine.Random.Range(0, inventoryService.InventorySize);
        inventoryService.RemoveAndShiftBall(idx);
    }
    
    public override bool CanExecute()
    {
        // インベントリにボールが1個以上あるかチェック（InventorySizeは空きスロットも含むため）
        var inventoryService = GameManager.Instance.InventoryService;
        for (int i = 0; i < inventoryService.InventorySize; i++)
        {
            if (inventoryService.GetBallData(i) != null)
                return true;
        }
        return false;
    }
}

/// <summary>
/// レリック追加アクション
/// </summary>
[Serializable]
public class AddRelicAction : StageEventActionBase
{
    [SerializeField] private RelicData relicData;
    
    public override void Execute()
    {
        GameManager.Instance.RelicService.AddRelic(relicData);
    }
    
    public override bool CanExecute()
    {
        var relicService = GameManager.Instance.RelicService;
        return relicService.Relics.Count < relicService.MaxRelics;
    }
}