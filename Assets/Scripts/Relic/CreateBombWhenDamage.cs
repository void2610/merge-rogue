using UnityEngine;
using R3;

public class CreateBombWhenDamage : RelicBase
{
    public override void Init(RelicUI relicUI)
    {
        IsCountable = true;
        base.Init(relicUI);
    }
    
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerDamage.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var x = EventManager.OnPlayerDamage.GetValue();
        Count.Value += x;

        var isActivated = false;
        while (Count.Value >= 20)
        {
            Count.Value -= 20;
            MergeManager.Instance.CreateBombBall();
            isActivated = true;
        }
    
        if (isActivated) UI?.ActivateUI();
    }
}
