using R3;

public class AllAttackWhenWeakAttack : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var x = EventManager.OnPlayerAttack.GetValue();
        if (x.Item1 <= 10)
        {
            // 全体攻撃に変換
            EventManager.OnPlayerAttack.SetValue((0, x.Item1 + x.Item2));
            UI?.ActivateUI();
        }
    }
}
