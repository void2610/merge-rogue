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
    /// ランダムなステージイベントを取得する
    /// </summary>
    /// <returns>ステージイベントのインスタンス</returns>
    StageEventBase GetRandomEvent();
    
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
    /// ボールベース画像を取得する
    /// </summary>
    /// <param name="type">ボール形状タイプ</param>
    /// <returns>ボールベース画像</returns>
    Sprite GetBallBaseImage(BallShapeType type);
    
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
    
    // ====== ゲーム進行メソッド ======
    
    /// <summary>
    /// アクトを進める
    /// </summary>
    void AddAct();
}