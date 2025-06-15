using UnityEngine;
using R3;

public class RageAndConfuseWhenDamage : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーがダメージを受けた時
        EventManager.OnPlayerDamage.AddProcessor(this, OnDamage);
    }
    
    private int OnDamage(int damage)
    {
        if (damage > 0) return damage;
        if (GameManager.Instance.RandomRange(0f, 1f) < 0.5f) return damage;
        
        // プレイヤーに怒りと混乱を付与
        StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Rage);
        // StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Confuse); // 混乱はコメントアウトされているので、必要に応じて有効化
        
        UI?.ActivateUI();
        return damage;
    }
}