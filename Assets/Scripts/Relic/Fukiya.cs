using R3;

public class Fukiya : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnOrganise.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        GameManager.Instance.Player.Damage(AttackType.Normal, 10);
        UI?.ActivateUI();
    }
}