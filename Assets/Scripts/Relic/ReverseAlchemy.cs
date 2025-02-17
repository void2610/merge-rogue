using R3;

public class ReverseAlchemy : RelicBase
{
    public override void Init(RelicUI relicUI)
    {
        IsCountable = true;
        base.Init(relicUI);
    }
    
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerDamage.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
        
    protected override void EffectImpl(Unit _)    
    {
        var x = EventManager.OnPlayerDamage.GetValue();
        Count.Value += x;

        var isActivated = false;
        while (Count.Value >= 5)
        {
            Count.Value -= 5;
            GameManager.Instance.AddCoin(1);
            isActivated = true;
        }
        
        if (isActivated) UI?.ActivateUI();
    }
}