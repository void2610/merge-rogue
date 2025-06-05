using UnityEngine;

/// <summary>
/// ショップに入店した時にコイン10枚を獲得する
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class SafePocketMoney : SafeRelicBase
{
    protected override void RegisterEffects()
    {
        // ショップ入店時にコイン10枚獲得
        SubscribeShopEnter(() =>
        {
            GameManager.Instance.AddCoin(10);
            ActivateUI();
        });
    }
}