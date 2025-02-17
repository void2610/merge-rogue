using R3;

public class HaisuiNoJin : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnEnemyDefeated.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);

        var max = GameManager.Instance.Player.MaxHealth.Value;
        GameManager.Instance.Player.MaxHealth.Value = max / 4;
        if (GameManager.Instance.Player.Health.Value > GameManager.Instance.Player.MaxHealth.Value)
            GameManager.Instance.Player.Health.Value = GameManager.Instance.Player.MaxHealth.Value;
    }

    protected override void EffectImpl(Unit _)
    {   
        StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Invincible, 1);
        UI?.ActivateUI();
    }
}
