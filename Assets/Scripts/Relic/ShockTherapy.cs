using R3;

public class ShockTherapy : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnEnemyStatusEffectAdded.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {
        if (EventManager.OnEnemyStatusEffectAdded.GetValue().Item2 != StatusEffectType.Shock) return;

        StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Power, 1);
        UI?.ActivateUI();
    }
}
