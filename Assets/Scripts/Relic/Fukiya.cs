using UnityEngine;
using R3;

/// <summary>
/// オーガナイズ時にプレイヤーに10ダメージを与えるレリック
/// </summary>
public class Fukiya : RelicBase
{
    public override void RegisterEffects()
    {
        // オーガナイズ時のイベント購読
        var subscription = EventManager.OnOrganise.Subscribe(OnOrganise);
        SimpleSubscriptions.Add(subscription);
    }
    
    private void OnOrganise(Unit _)
    {
        GameManager.Instance?.Player?.Damage(AttackType.Normal, 10);
        UI?.ActivateUI();
    }
}