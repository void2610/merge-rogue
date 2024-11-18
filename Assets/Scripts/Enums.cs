using System.Collections.Generic;
using UnityEngine;



public enum EffectTiming
{
    OnGameStart,           // ゲーム開始時
    OnAcquisition,         // レリック獲得時
    AlwaysActive,          // 常に
    OnPlayerAttack,        // プレイヤーの攻撃時
    OnPlayerDamaged,       // プレイヤーがダメージを受けた時
    OnPlayerHeal,          // プレイヤーが回復した時
    OnEnemyAttack,         // 敵の攻撃時
    OnEnemyHeal,           // 敵が回復した時
    OnEnemyInit,           // 敵初期化時
    OnEnemySpawn,          // 敵出現時
    OnEnemyDefeated,       // 敵撃破時
    OnCoinGain,        // コイン獲得時
    OnExperienceGain,    // 経験値獲得時
    OnLevelUp,             // レベルアップ時
    OnStageClear,          // ステージクリア時
    OnStageStart,          // ステージ開始時
    OnPlayerDeath,         // プレイヤー死亡時
    OnMergeStart,          // マージ開始時
    OnBallDropped,         // ボールを落とした時
    OnMerge,               // マージ時
    OnMergeEnd,            // マージ終了時
    OnTurnStart,           // ターン開始時
    OnTurnEnd,             // ターン終了時
    OnShopEnter,           // ショップ開始時
    OnShopExit,            // ショップ終了時
    OnItemPurchased        // ショップで購入時
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public static class RarityColors
{
    /// <summary>
    /// レアリティに紐づく色を保持する辞書
    /// </summary>
    private static readonly Dictionary<Rarity, Color> rarityToColorMap = new ()
    {
        { Rarity.Common, new Color(0.8f, 0.8f, 0.8f) },       // グレー
        { Rarity.Uncommon, new Color(0.3f, 1f, 0.3f) },      // 緑
        { Rarity.Rare, new Color(0.2f, 0.4f, 1f) },          // 青
        { Rarity.Epic, new Color(0.7f, 0.3f, 0.9f) },        // 紫
        { Rarity.Legendary, new Color(1f, 0.8f, 0f) }        // 金
    };

    /// <summary>
    /// レアリティに対応する色を取得する
    /// </summary>
    /// <param name="rarity">レアリティ</param>
    /// <returns>紐づけられた色</returns>
    public static Color GetColor(Rarity rarity)
    {
        if (rarityToColorMap.TryGetValue(rarity, out Color color))
        {
            return color;
        }

        // デフォルトの色を返す（例: 白）
        return Color.white;
    }
}