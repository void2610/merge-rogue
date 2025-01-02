using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class Alchemy : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        disposable = EventManager.OnPlayerAttack.Subscribe(Effect).AddTo(this);
        ui = relicUI;

        Effect(Unit.Default);
    }

    public void RemoveEffect()
    {
        disposable?.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var coin = GameManager.Instance.Coin.Value;
        var e = GameManager.Instance.EnemyContainer.GetCurrentEnemyCount();
        var x = EventManager.OnPlayerAttack.GetValue();

        // 消費するコインが存在し、単体攻撃力が1以上、敵が2体以上いる場合
        if (coin >= 10 && x.Item1 > 0 && e >= 2)
        {
            GameManager.Instance.SubtractCoin(10);
            EventManager.OnPlayerAttack.SetValue((0, x.Item1 + x.Item2));
            ui?.ActivateUI();
        }
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
