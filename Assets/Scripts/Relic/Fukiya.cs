using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// オーガナイズ時にプレイヤーに10ダメージを与えるレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class Fukiya : RelicBase
{
    protected override void RegisterEffects()
    {
        // オーガナイズ時のイベント購読
        var subscription = SafeEventManager.OnOrganise.OnProcessed.Subscribe(OnOrganise);
        _simpleSubscriptions.Add(subscription);
    }
    
    private void OnOrganise((int original, int modified) _)
    {
        GameManager.Instance?.Player?.Damage(AttackType.Normal, 10);
        UI?.ActivateUI();
    }
}