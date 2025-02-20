using R3;

public class AddOneToAllAttack : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var dic = EventManager.OnPlayerAttack.GetValue();
        dic[AttackType.Normal] += 5;
        EventManager.OnPlayerAttack.SetValue(dic);
        UI?.ActivateUI();
    }
}