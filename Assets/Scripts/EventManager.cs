using System;
using R3;
using UnityEngine;

public static class EventManager
{
    // ゲーム開始時: なし
    public static readonly GameEvent<int> OnGameStart = new (0);
    // コイン獲得時: コイン獲得量
    public static readonly GameEvent<int> OnCoinGain = new (0);
    // 経験値獲得時: 経験値獲得量
    public static readonly GameEvent<int> OnPlayerExpGain = new (0);
    // プレイヤーの攻撃時: (単体攻撃の攻撃力, 全体攻撃の攻撃力)
    public static readonly GameEvent<(int, int)> OnPlayerAttack = new ((0, 0));
    // プレイヤーのダメージ時: プレイヤーのダメージ量
    public static readonly GameEvent<int> OnPlayerDamage = new (0);
    // プレイヤーの回復時: プレイヤーの回復量
    public static readonly GameEvent<int> OnPlayerHeal = new (0);
    // 敵出現時: 敵の出現数
    public static readonly GameEvent<int> OnEnemySpawn = new (0);
    // 敵の初期化時: 敵のステータス倍率
    public static readonly GameEvent<float> OnEnemyInit = new (1.0f);
    // 敵の攻撃時: 敵の攻撃力
    public static readonly GameEvent<int> OnEnemyAttack = new (0);
    // 敵のダメージ時: 敵のダメージ量
    public static readonly GameEvent<int> OnEnemyDamage = new (0);
    // 敵の回復時: 敵の回復量
    public static readonly GameEvent<int> OnEnemyHeal = new (0);
    // 敵撃破時: EnemyBase
    public static readonly GameEvent<EnemyBase> OnEnemyDefeated = new (null);
    // プレイヤー死亡時: 復活するかどうか
    public static readonly GameEvent<bool> OnPlayerDeath = new (false);
    // ボールを落とした時: なし
    public static readonly GameEvent<int> OnBallDrop = new (0);
    // ボールを消費した時: なし
    public static readonly GameEvent<int> OnBallSkip = new (0);
    // ボールをマージした時: マージしたボールのレベル
    public static readonly GameEvent<int> OnBallMerged = new (0);
    // 休憩に入った時: なし
    public static readonly GameEvent<int> OnRestEnter = new (0);
    // 休憩した時: HP回復量
    public static readonly GameEvent<int> OnRest = new (0);
    // ボールを整理した時: なし
    public static readonly GameEvent<int> OnOrganise = new (0);
    // 休憩から出た時: なし
    public static readonly GameEvent<int> OnRestExit = new (0);
    // ショップに入った時: なし
    public static readonly GameEvent<int> OnShopEnter = new (0);
    // ボールを削除したとき: なし
    public static readonly GameEvent<int> OnBallRemove = new (0);
    // ショップから出た時: なし
    public static readonly GameEvent<int> OnShopExit = new (0);
    // ショップでアイテムを購入した時: なし
    public static readonly GameEvent<int> OnItemPurchased = new (0);
    
    // EnumからGameEventを取得
    public static object GetEventFromEnum(GameEventTypes type)
    {
        return type switch
        {
            GameEventTypes.OnGameStart => OnGameStart,
            GameEventTypes.OnCoinGain => OnCoinGain,
            GameEventTypes.OnPlayerExpGain => OnPlayerExpGain,
            GameEventTypes.OnPlayerAttack => OnPlayerAttack,
            GameEventTypes.OnPlayerDamage => OnPlayerDamage,
            GameEventTypes.OnPlayerHeal => OnPlayerHeal,
            GameEventTypes.OnEnemySpawn => OnEnemySpawn,
            GameEventTypes.OnEnemyInit => OnEnemyInit,
            GameEventTypes.OnEnemyAttack => OnEnemyAttack,
            GameEventTypes.OnEnemyDamage => OnEnemyDamage,
            GameEventTypes.OnEnemyHeal => OnEnemyHeal,
            GameEventTypes.OnEnemyDefeated => OnEnemyDefeated,
            GameEventTypes.OnPlayerDeath => OnPlayerDeath,
            GameEventTypes.OnBallDrop => OnBallDrop,
            GameEventTypes.OnBallSkip => OnBallSkip,
            GameEventTypes.OnBallMerged => OnBallMerged,
            GameEventTypes.OnRestEnter => OnRestEnter,
            GameEventTypes.OnRest => OnRest,
            GameEventTypes.OnOrganise => OnOrganise,
            GameEventTypes.OnRestExit => OnRestExit,
            GameEventTypes.OnShopEnter => OnShopEnter,
            GameEventTypes.OnBallRemove => OnBallRemove,
            GameEventTypes.OnShopExit => OnShopExit,
            GameEventTypes.OnItemPurchased => OnItemPurchased,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    // ゲーム開始時にイベントをリセット
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEventManager()
    {
        foreach(var p in typeof(EventManager).GetFields())
        {
            if(p.FieldType == typeof(GameEvent<int>))
            {
                var e = (GameEvent<int>)p.GetValue(null);
                e.ResetAll();
            }
            else if(p.FieldType == typeof(GameEvent<float>))
            {
                var e = (GameEvent<float>)p.GetValue(null);
                e.ResetAll();
            }
            else if(p.FieldType == typeof(GameEvent<bool>))
            {
                var e = (GameEvent<bool>)p.GetValue(null);
                e.ResetAll();
            }
        }
    }
}