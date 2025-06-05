using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// 敌にShock付与時にプレイヤーにPowerを付与するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class ShockTherapy : RelicBase
{
    protected override void RegisterEffects()
    {
        // 敌ステータス効果追加時のイベント購読
        var subscription = SafeEventManager.OnEnemyStatusEffectAdded.OnProcessed.Subscribe(OnEnemyStatusEffectAdded);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnEnemyStatusEffectAdded(((EnemyBase enemy, StatusEffectType type, int stack) original, (EnemyBase enemy, StatusEffectType type, int stack) modified) data)
    {
        var effectData = data.modified;
        if (effectData.type == StatusEffectType.Shock)
        {
            StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Power, 1);
            UI?.ActivateUI();
        }
    }
}
