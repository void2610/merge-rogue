using R3;

public class MergeAllWhenLowHealth : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerDamage.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        if(GameManager.Instance.Player.Health.Value > 20) return;
        
        MergeManager.Instance.MergeAll();
        UI?.ActivateUI();
    }
}
