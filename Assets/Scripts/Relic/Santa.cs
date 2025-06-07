using UnityEngine;
using R3;

/// <summary>
/// 休憩時にランダムなレリックを獲得するレリック
/// </summary>
public class Santa : RelicBase
{
    protected override void RegisterEffects()
    {
        // 休憩時のイベント購読
        AddSubscription(RelicHelpers.SubscribeRestEnter(OnRestEnter));
    }

    private void OnRestEnter()
    {
        var rarity = GameManager.Instance?.RandomRange(0.0f, 1.0f) > 0.5f ? Rarity.Common : Rarity.Uncommon;
        var relics = ContentProvider.Instance?.GetRelicDataByRarity(rarity);
        
        if (relics != null && relics.Count > 0)
        {
            var randomRelic = relics[GameManager.Instance.RandomRange(0, relics.Count)];
            RelicManager.Instance?.AddRelic(randomRelic);
        }
        UI?.ActivateUI();
    }
}
