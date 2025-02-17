using System;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EventManager
{
    // ゲーム開始時: なし
    public static readonly GameEvent<int> OnGameStart = new (0);
    // バトル開始時: なし
    public static readonly GameEvent<int> OnBattleStart = new (0);
    // コイン獲得時: コイン獲得量
    public static readonly GameEvent<int> OnCoinGain = new (0);
    // コイン消費時: コイン消費量
    public static readonly GameEvent<int> OnCoinConsume = new (0);
    // 経験値獲得時: 経験値獲得量
    public static readonly GameEvent<int> OnPlayerExpGain = new (0);
    // プレイヤーの攻撃時: (単体攻撃の攻撃力, 全体攻撃の攻撃力)
    public static readonly GameEvent<(int, int)> OnPlayerAttack = new ((0, 0));
    // プレイヤーのダメージ時: プレイヤーのダメージ量
    public static readonly GameEvent<int> OnPlayerDamage = new (0);
    // プレイヤーの回復時: プレイヤーの回復量
    public static readonly GameEvent<int> OnPlayerHeal = new (0);
    // プレイヤーの状態異常時: 状態異常の種類
    public static readonly GameEvent<StatusEffectType> OnPlayerStatusEffect = new (StatusEffectType.Burn);
    // 敵の状態異常時: (敵, 状態異常の種類)
    public static readonly GameEvent<(EnemyBase, StatusEffectType)> OnEnemyStatusEffect = new ((null, StatusEffectType.Burn));
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
    // マップでイベントステージに入った時: 実際に入るステージ
    public static readonly GameEvent<StageType> OnEventStageEnter = new (StageType.Undefined);
    // ステージイベントに入った時: 再生するStageEventBase
    public static readonly GameEvent<StageEventBase> OnStageEventEnter = new (null);
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
    // 宝箱を開いた時: なし
    public static readonly GameEvent<int> OnTreasureOpened = new (0);
    // 宝箱でレリックを取得した時: RelicData
    public static readonly GameEvent<RelicData> OnRelicObtainedTreasure = new (null);
    // 宝箱をスキップした時: なし
    public static readonly GameEvent<int> OnTreasureSkipped = new (0);
    
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