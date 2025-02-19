using R3;

public class CreateTwoBallWhenSkip : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnBallSkip.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {
        MergeManager.Instance.CreateRandomBall();
        MergeManager.Instance.CreateRandomBall();
        UI?.ActivateUI();
    }
}
