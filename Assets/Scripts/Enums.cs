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

public enum RelicRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum BallRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}