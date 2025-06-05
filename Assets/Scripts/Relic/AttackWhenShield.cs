using UnityEngine;
using R3;

/// <summary>
/// シールド発動時に敵に攻撃するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class AttackWhenShield : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーのステータス効果発動時のイベント購読は
        // 新システムではより安全な方法で処理する必要があります
        // ここでは従来のEventManagerを一時的に使用
        var subscription = EventManager.OnPlayerStatusEffectTriggered.Subscribe(OnStatusEffectTriggered);
        _simpleSubscriptions.Add(subscription);
    }

    private void OnStatusEffectTriggered(Unit _)
    {
        var effectData = EventManager.OnPlayerStatusEffectTriggered.GetValue();
        if (effectData.Item1 == StatusEffectType.Shield)
        {
            var enemies = EnemyContainer.Instance?.GetAllEnemies();
            if (enemies != null && enemies.Count > 0)
            {
                enemies[0].Damage(AttackType.Normal, effectData.Item2);
                UI?.ActivateUI();
            }
        }
    }
}
