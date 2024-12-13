using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class Santa : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        disposable = EventManager.OnRest.Subscribe(Effect).AddTo(this);
        ui = relicUI;

        Effect(Unit.Default);
    }

    public void RemoveEffect()
    {
        disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var rarity = GameManager.Instance.RandomRange(0.0f, 1.0f) > 0.5f ? Rarity.Common : Rarity.Uncommon;
        var relics = RelicManager.Instance.allRelicDataList.GetRelicDataFromRarity(rarity);
        var r = GameManager.Instance.RandomRange(0, relics.Count);
        
        RelicManager.Instance.AddRelic(relics[r]);
        ui?.ActivateUI();
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
