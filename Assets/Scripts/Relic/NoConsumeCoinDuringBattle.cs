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
        if(GameManager.Instance.state != GameManager.GameState.Merge && GameManager.Instance.state != GameManager.GameState.PlayerAttack && GameManager.Instance.state != GameManager.GameState.EnemyAttack)
            return;
        
        EventManager.OnCoinConsume.SetValue(0);
        UI?.ActivateUI();
    }
}
