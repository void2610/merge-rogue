using R3;

public class AddMaxHealthWhenSkipTreasure : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnTreasureSkipped.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        GameManager.Instance.Player.MaxHealth.Value += 10;
        UI?.ActivateUI();
    }
}
