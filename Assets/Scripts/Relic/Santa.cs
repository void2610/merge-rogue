using R3;

public class Santa : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnRest.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {
        var rarity = GameManager.Instance.RandomRange(0.0f, 1.0f) > 0.5f ? Rarity.Common : Rarity.Uncommon;
        var relics = ContentProvider.Instance.GetRelicDataByRarity(rarity);
        
        RelicManager.Instance.AddRelic(relics[GameManager.Instance.RandomRange(0, relics.Count)]);
        UI?.ActivateUI();
    }
}
