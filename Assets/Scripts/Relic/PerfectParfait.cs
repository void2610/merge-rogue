using R3;

public class PerfectParfait : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {
        var count = MergeManager.Instance.GetBallCount();
        if(count > 0) return;

        var dic = EventManager.OnPlayerAttack.GetValue();
        EventManager.OnPlayerAttack.SetValue(dic.MultiplyAll(5.0f));
        UI?.ActivateUI();
    }
}
