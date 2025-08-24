using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンテンツ提供サービスのインターフェース
/// ゲーム内のボール、レリック、敵、イベントなどのデータ取得機能を提供
/// </summary>
public interface IContentService
{
    /// <summary>
    /// 現在のアクト数
    /// </summary>
    int Act { get; }
    
    /// <summary>
    /// ステータスエフェクトデータリスト
    /// </summary>
    StatusEffectDataList StatusEffectList { get; }
    
    // ====== ランダム取得メソッド ======
    
    /// <summary>
    /// 全てのステージイベントデータを取得する
    /// </summary>
    /// <returns>ステージイベントデータのリスト</returns>
    List<StageEventData> GetAllStageEventData();
    
    /// <summary>
    /// ランダムな敵データを取得する
    /// </summary>
    /// <returns>敵データ</returns>
    EnemyData GetRandomEnemy();
    
    /// <summary>
    /// ランダムなボスデータを取得する
    /// </summary>
    /// <returns>ボスデータ</returns>
    EnemyData GetRandomBoss();
    
    /// <summary>
    /// ランダムなレリックデータを取得する
    /// </summary>
    /// <returns>レリックデータ</returns>
    RelicData GetRandomRelic();
    
    /// <summary>
    /// ランダムなレアリティを取得する
    /// </summary>
    /// <returns>レアリティ</returns>
    Rarity GetRandomRarity();
    
    // ====== ボール関連メソッド ======
    
    /// <summary>
    /// ノーマルボール以外のボールリストを取得する
    /// </summary>
    /// <returns>ノーマルボール以外のボールデータリスト</returns>
    List<BallData> GetBallListExceptNormal();
    
    /// <summary>
    /// ノーマルボールデータを取得する
    /// </summary>
    /// <returns>ノーマルボールデータ</returns>
    BallData GetNormalBallData();
    
    /// <summary>
    /// クラス名からボールデータを取得する
    /// </summary>
    /// <param name="className">クラス名</param>
    /// <returns>ボールデータ</returns>
    BallData GetBallDataFromClassName(string className);
    
    /// <summary>
    /// ボールタイプ名からボールデータを取得する
    /// </summary>
    /// <param name="ballType">ボールタイプ名</param>
    /// <returns>ボールデータ</returns>
    BallData GetBallData(string ballType);
    
    // ====== レリック関連メソッド ======
    
    /// <summary>
    /// クラス名からレリックデータを取得する
    /// </summary>
    /// <param name="className">クラス名</param>
    /// <returns>レリックデータ</returns>
    RelicData GetRelicByClassName(string className);
    
    /// <summary>
    /// 指定されたレアリティのレリックデータを全て取得する
    /// </summary>
    /// <param name="rarity">レアリティ</param>
    /// <returns>指定レアリティのレリックデータリスト</returns>
    List<RelicData> GetRelicDataByRarity(Rarity rarity);
    
    /// <summary>
    /// 全てのレリックデータを取得する
    /// </summary>
    /// <returns>全レリックデータリスト</returns>
    List<RelicData> GetAllRelicData();
    
    // ====== 価格関連メソッド ======
    
    /// <summary>
    /// ショップアイテムの価格を取得する
    /// </summary>
    /// <param name="type">アイテムタイプ</param>
    /// <param name="rarity">レアリティ</param>
    /// <returns>価格</returns>
    int GetShopPrice(Shop.ShopItemType type, Rarity rarity);
    
    /// <summary>
    /// ボール除去の価格を取得する
    /// </summary>
    /// <returns>ボール除去価格</returns>
    int GetBallRemovePrice();
    
    /// <summary>
    /// ボール強化の価格を取得する
    /// </summary>
    /// <returns>ボール強化価格</returns>
    int GetBallUpgradePrice();
    
    /// <summary>
    /// プレイヤーの初期所持コイン数を取得する
    /// </summary>
    int GetInitialPlayerCoin();
    
    // ====== ゲーム進行メソッド ======
    
    /// <summary>
    /// アクトを進める
    /// </summary>
    void AddAct();
    
    /// <summary>
    /// ショップ価格倍率を設定する（レリック効果用）
    /// </summary>
    /// <param name="multiplier">価格倍率</param>
    void SetShopPriceMultiplier(float multiplier);
    
    // ====== 敵難易度関連 ======
    
    /// <summary>
    /// 全体的な敵難易度倍率（動的変更可能）
    /// </summary>
    float GlobalEnemyDifficultyMultiplier { get; }
    
    /// <summary>
    /// 全体的な敵難易度倍率を設定する（レリック効果用）
    /// </summary>
    /// <param name="multiplier">難易度倍率</param>
    void SetGlobalEnemyDifficultyMultiplier(float multiplier);
    
    /// <summary>
    /// 敵のヘルス基本倍率を取得する（ContentProviderDataから）
    /// </summary>
    float BaseEnemyHealthMultiplier { get; }
    
    /// <summary>
    /// 敵の攻撃力基本倍率を取得する（ContentProviderDataから）
    /// </summary>
    float BaseEnemyAttackMultiplier { get; }
    
    /// <summary>
    /// デモ版かどうか
    /// </summary>
    bool IsDemoPlay { get; }
}