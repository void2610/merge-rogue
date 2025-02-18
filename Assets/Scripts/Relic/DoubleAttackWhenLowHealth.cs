using R3;

public class DoubleAttackWhenLowHealth : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnBattleStart.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        if (GameManager.Instance.Player.Health.Value <= GameManager.Instance.Player.MaxHealth.Value * 0.2f　|| GameManager.Instance.Player.Health.Value <= 20)
        {
            StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Rage, 10);
            UI?.ActivateUI();
        }
    }
}
