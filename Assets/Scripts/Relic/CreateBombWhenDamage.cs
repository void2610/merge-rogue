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
            var width = MergeManager.Instance.Wall.WallWidth;
            var r = GameManager.Instance.RandomRange(-width / 2 + 0.1f, width / 2 - 0.1f);
            var p = new Vector3(r, 0.8f, 0);
            MergeManager.Instance.CreateBombBall(p);
            isActivated = true;
        }
    
        if (isActivated) UI?.ActivateUI();
    }
}
