using UnityEngine;

/// <summary>
/// プレイヤーの攻撃時に通常攻撃力に+5する
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class SafeAddOneToAllAttack : SafeRelicBase
{
    protected override void RegisterEffects()
    {
        // 通常攻撃力に+5する
        RegisterAttackAddition(AttackType.Normal, 5);
    }
}