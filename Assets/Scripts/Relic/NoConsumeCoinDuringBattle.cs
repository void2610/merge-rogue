using R3;

public class NoConsumeCoinDuringBattle : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnCoinConsume.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        if(GameManager.Instance.State.CurrentValue != GameManager.GameState.Merge && GameManager.Instance.State.CurrentValue != GameManager.GameState.EnemyAttack)
            return;
        
        EventManager.OnCoinConsume.SetValue(0);
        UI?.ActivateUI();
    }
}
