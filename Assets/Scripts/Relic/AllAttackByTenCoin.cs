using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AllAttackByTenCoin : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);

        EffectImpl(Unit.Default);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var coin = GameManager.Instance.Coin.Value;
        var e = GameManager.Instance.EnemyContainer.GetCurrentEnemyCount();
        var x = EventManager.OnPlayerAttack.GetValue();

        // 消費するコインが存在し、単体攻撃力が1以上、敵が2体以上いる場合
        if (coin >= 10 && x.Item1 > 0 && e >= 2)
        {
            GameManager.Instance.SubCoin(10);
            EventManager.OnPlayerAttack.SetValue((0, x.Item1 + x.Item2));
            UI?.ActivateUI();
        }
    }
}
