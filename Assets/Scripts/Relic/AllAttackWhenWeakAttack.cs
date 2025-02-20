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
        var dic = EventManager.OnPlayerAttack.GetValue();
        if (dic[AttackType.Normal] <= 30)
        {
            // 全体攻撃に変換
            dic[AttackType.All] += dic[AttackType.Normal];
            dic[AttackType.Normal] = 0;
            EventManager.OnPlayerAttack.SetValue(dic);
            UI?.ActivateUI();
        }
    }
}
