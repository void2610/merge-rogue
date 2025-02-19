using R3;

public class HealWhenMergeLastBall : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnBallMerged.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var maxRank = InventoryManager.Instance.InventorySize;
        if (EventManager.OnBallMerged.GetValue().Item1.Rank == maxRank)
        {
            int heal = GameManager.Instance.Player.MaxHealth.Value / 4;
            GameManager.Instance.Player.Heal(heal);
            UI?.ActivateUI();
        }
    }
}
