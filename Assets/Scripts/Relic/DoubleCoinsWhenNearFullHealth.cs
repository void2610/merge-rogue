using R3;

public class DoubleCoinsWhenNearFullHealth : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnCoinGain.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var health = GameManager.Instance.Player.Health.Value;
        var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
        if(health > maxHealth * 0.8f)
        {
            var x = EventManager.OnCoinGain.GetValue();
            EventManager.OnCoinGain.SetValue(x * 2);
            UI?.ActivateUI();
        }
    }
}
