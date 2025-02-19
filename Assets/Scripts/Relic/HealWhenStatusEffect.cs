using R3;

public class HealWhenStatusEffect : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerStatusEffectAdded.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        GameManager.Instance.Player.Heal(1);
        UI?.ActivateUI();
    }
}
