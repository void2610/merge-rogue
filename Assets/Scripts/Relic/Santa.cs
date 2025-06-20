using System.Collections.Generic;
using UnityEngine;
using R3;
using VContainer;

/// <summary>
/// 休憩時にランダムなレリックを獲得するレリック
/// </summary>
public class Santa : RelicBase
{
    public override void RegisterEffects()
    {
        // 休憩時のイベント購読
        AddSubscription(RelicHelpers.SubscribeRestEnter(OnRestEnter));
    }

    private void OnRestEnter()
    {
        var rarity = RandomService.RandomRange(0.0f, 1.0f) > 0.5f ? Rarity.Common : Rarity.Uncommon;
        var relics = ContentService?.GetRelicDataByRarity(rarity);

        if (relics is not { Count: > 0 }) return;
        var randomRelic = relics[RandomService.RandomRange(0, relics.Count)];
        RelicService.AddRelic(randomRelic);

        UI?.ActivateUI();
    }
}