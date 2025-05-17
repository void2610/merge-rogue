using R3;

public class AttackWhenShield : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerStatusEffectTriggered.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        var v = EventManager.OnPlayerStatusEffectTriggered.GetValue();
        if(v.Item1 == StatusEffectType.Shield)
        {
            EnemyContainer.Instance.GetAllEnemies()[0].Damage(AttackType.Normal, v.Item2);
            UI?.ActivateUI();
        }
    }
}
