using R3;

public class ShockTherapy : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnEnemyStatusEffect.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {
        if (EventManager.OnEnemyStatusEffect.GetValue().Item2 != StatusEffectType.Shock) return;

        StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Power, 1);
        UI?.ActivateUI();
    }
}
