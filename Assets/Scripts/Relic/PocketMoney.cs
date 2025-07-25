using UnityEngine;

/// <summary>
/// ショップに入店した時にコイン10枚を獲得する
/// </summary>
public class PocketMoney : RelicBase
{
    public override void RegisterEffects()
    {
        // ショップ入店時にコイン10枚獲得
        AddSubscription(RelicHelpers.SubscribeShopEnter(() =>
        {
            GameManager.Instance.AddCoin(10);
            ActivateUI();
        }));
    }
}