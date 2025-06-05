using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// オーガナイズ時にプレイヤーに10ダメージを与えるレリック
/// </summary>
public class Fukiya : RelicBase
{
    protected override void RegisterEffects()
    {
        // オーガナイズ時のイベント購読
        var subscription = SafeEventManager.OnOrganise.Subscribe(OnOrganise);
        _simpleSubscriptions.Add(subscription);
    }
    
    private void OnOrganise(Unit _)
    {
        GameManager.Instance?.Player?.Damage(AttackType.Normal, 10);
        UI?.ActivateUI();
    }
}