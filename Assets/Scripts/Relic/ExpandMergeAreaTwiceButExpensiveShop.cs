using UnityEngine;
using R3;

public class ExpandMergeAreaTwiceButExpensiveShop : RelicBase
{
    public override void RegisterEffects()
    {
        // マージエリアを2回拡張
        MergeManager.Instance.LevelUpWallWidth();
        MergeManager.Instance.LevelUpWallWidth();
        
        // ショップ価格を1.5倍に設定
        ContentService?.SetShopPriceMultiplier(1.5f);
    }
}