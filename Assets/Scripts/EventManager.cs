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
    // プレイヤーの攻撃時: プレイヤーの攻撃範囲
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
    // プレイヤー死亡時: 復活するかどうか
    public static readonly GameEvent<bool> OnPlayerDeath = new (false);
    // ボールを落とした時: ボールの個数
    public static readonly GameEvent<int> OnBallDropped = new (0);
    
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