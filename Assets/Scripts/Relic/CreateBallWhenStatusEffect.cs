using R3;

public class CreateBallWhenStatusEffect : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerStatusEffectAdded.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        MergeManager.Instance.CreateRandomBall();
        UI?.ActivateUI();
    }
}
