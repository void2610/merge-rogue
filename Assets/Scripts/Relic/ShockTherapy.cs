using UnityEngine;
using R3;

/// <summary>
/// 敌にShock付与時にプレイヤーにPowerを付与するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class ShockTherapy : RelicBase
{
    protected override void RegisterEffects()
    {
        // 敌ステータス効果追加時のイベント購読
        var subscription = EventManager.OnEnemyStatusEffectAdded.Subscribe(OnEnemyStatusEffectAdded);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnEnemyStatusEffectAdded(Unit _)
    {
        var effectData = EventManager.OnEnemyStatusEffectAdded.GetValue();
        if (effectData.Item2 == StatusEffectType.Shock)
        {
            StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Power, 1);
            UI?.ActivateUI();
        }
    }
}
