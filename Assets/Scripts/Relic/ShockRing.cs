using R3;

public class ShockRing : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {
        var enemies = EnemyContainer.Instance.GetAllEnemies();
        if (enemies.Count == 0) return;
        
        StatusEffectFactory.AddStatusEffect(enemies[0], StatusEffectType.Shock, 1);
        UI?.ActivateUI();
    }
}