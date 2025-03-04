using R3;
using UnityEngine;

public class StrongBurnWhenManyBalls : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnEnemyStatusEffectTriggered.Subscribe(EffectImpl);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        var value = EventManager.OnEnemyStatusEffectTriggered.GetValue();
        if (value.Item2 != StatusEffectType.Burn) return;
        // if (MergeManager.Instance.GetBallCount() < 10) return;

        var idx = EnemyContainer.Instance.GetEnemyIndex(value.Item1);
        if (EnemyContainer.Instance.GetCurrentEnemyCount() < idx + 2) return;
        StatusEffectFactory.AddStatusEffect(EnemyContainer.Instance.GetAllEnemies()[idx+1], StatusEffectType.Burn);

        UI?.ActivateUI();
    }
}
