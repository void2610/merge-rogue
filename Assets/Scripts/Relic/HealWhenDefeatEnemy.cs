using UnityEngine;
using R3;

public class HealWhenDefeatEnemy : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnEnemyDefeated.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var enemy = EventManager.OnEnemyDefeated.GetValue();
        if(!enemy) return;
        
        var heal = enemy.MaxHealth * 0.1f;
        GameManager.Instance.Player.Heal(Mathf.CeilToInt(heal));
    }
}
