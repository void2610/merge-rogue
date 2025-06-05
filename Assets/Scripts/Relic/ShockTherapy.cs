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
        var subscription = SafeEventManager.OnEnemyStatusEffectAdded.Subscribe(OnEnemyStatusEffectAdded);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnEnemyStatusEffectAdded(Unit _)
    {
        // 敌にShockが付与された場合、プレイヤーにPowerを付与
        // 簡略化版では詳細な情報を取得できないため、常に発動
        StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Power, 1);
        UI?.ActivateUI();
    }
}
