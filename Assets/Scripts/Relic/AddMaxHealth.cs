using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AddMaxHealth : RelicBase
{
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        if (GameManager.Instance.Player == null) return;
        GameManager.Instance.Player.MaxHealth.Value -= 10;
    }
    
    protected override void SubscribeEffect()
    {
        GameManager.Instance.Player.MaxHealth.Value += 10;
        UI?.ActiveAlways();
    }
    
    protected override void EffectImpl(Unit _) {}
}
