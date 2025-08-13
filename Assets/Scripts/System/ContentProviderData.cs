using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// ContentProvider用のデータ管理ScriptableObject
/// 敵、ボス、イベント、ボール、レリック、ステータスエフェクトのデータを統合管理
/// </summary>
[CreateAssetMenu(fileName = "ContentProviderData", menuName = "Merge Rogue/ContentProviderData")]
public class ContentProviderData : ScriptableObject
{
    [Serializable]
    public class ContentData
    { 
        public Object data;
        public float probability;
    }
    
    [Serializable]
    public class ContentDataList
    { 
        public List<ContentData> list;
    }

    [Header("敵データ")]
    [SerializeField] private List<ContentDataList> enemyList;
    
    [Header("ボスデータ")]
    [SerializeField] private List<ContentDataList> bossList;
    
    [Header("イベントデータ")]
    [SerializeField] private List<ContentDataList> eventList;
    
    [Header("ボールデータ")]
    [SerializeField] private BallDataList ballList;
    
    [Header("レリックデータ")]
    [SerializeField] private RelicDataList relicList;
    
    [Header("ステータスエフェクトデータ")]
    [SerializeField] private StatusEffectDataList statusEffectList;
    
    [Header("ステージイベントデータ")]
    [SerializeField] private AllStageEventDataList stageEventDataList;
    
    [Header("レアリティ確率設定")]
    [SerializeField, Range(0f, 1f)] private float commonProbability = 0.40f;   // 40%
    [SerializeField, Range(0f, 1f)] private float uncommonProbability = 0.70f; // 30% (累積70%)
    [SerializeField, Range(0f, 1f)] private float rareProbability = 0.85f;     // 15% (累積85%)
    // Epic: 15% (残り), Legendary: 0% (現在未使用)
    
    [Header("ショップ価格設定")]
    [SerializeField] private int commonPrice = 10;
    [SerializeField] private int uncommonPrice = 20;
    [SerializeField] private int rarePrice = 50;
    [SerializeField] private int epicPrice = 100;
    [SerializeField] private int legendaryPrice = 500;
    [SerializeField, Range(0f, 1f)] private float ballPriceMultiplier = 0.75f; // ボール価格倍率
    
    [Header("特殊価格設定")]
    [SerializeField] private int ballRemovePrice = 25;
    [SerializeField] private int ballUpgradePrice = 10;
    
    [Header("レリック重複回避設定")]
    [SerializeField] private int relicRetryCount = 3; // 重複回避の再試行回数
    
    [Header("敵難易度倍率設定（バランス調整用）")]
    [SerializeField] private float baseEnemyHealthMultiplier = 1.0f;  // 基本ヘルス倍率
    [SerializeField] private float baseEnemyAttackMultiplier = 1.0f;  // 基本攻撃力倍率
    
    // プロパティでデータにアクセス
    public List<ContentDataList> EnemyList => enemyList;
    public List<ContentDataList> BossList => bossList;
    public List<ContentDataList> EventList => eventList;
    public BallDataList BallList => ballList;
    public RelicDataList RelicList => relicList;
    public StatusEffectDataList StatusEffectList => statusEffectList;
    public AllStageEventDataList StageEventDataList => stageEventDataList;
    
    // 設定値アクセス用プロパティ
    public float CommonProbability => commonProbability;
    public float UncommonProbability => uncommonProbability;
    public float RareProbability => rareProbability;
    public int CommonPrice => commonPrice;
    public int UncommonPrice => uncommonPrice;
    public int RarePrice => rarePrice;
    public int EpicPrice => epicPrice;
    public int LegendaryPrice => legendaryPrice;
    public float BallPriceMultiplier => ballPriceMultiplier;
    public int BallRemovePrice => ballRemovePrice;
    public int BallUpgradePrice => ballUpgradePrice;
    public int RelicRetryCount => relicRetryCount;
    
    // 敵難易度倍率設定
    public float BaseEnemyHealthMultiplier => baseEnemyHealthMultiplier;
    public float BaseEnemyAttackMultiplier => baseEnemyAttackMultiplier;
    
    /// <summary>
    /// データ初期化処理
    /// </summary>
    public void InitializeData()
    {
        if (ballList) ballList.Register();
        if (relicList) relicList.Register();
        if (statusEffectList) statusEffectList.Register();
    }
    
    /// <summary>
    /// デモ版でフィルタリングされたボールリストを取得
    /// </summary>
    public List<BallData> GetFilteredBallList()
    {
        #if DEMO_PLAY
        return ballList?.list?.FindAll(b => b.availableDemo) ?? new List<BallData>();
        #else
        return ballList?.list ?? new List<BallData>();
        #endif
    }
    
    /// <summary>
    /// デモ版でフィルタリングされたレリックリストを取得
    /// </summary>
    public List<RelicData> GetFilteredRelicList()
    {
        #if DEMO_PLAY
        return relicList?.list?.FindAll(r => r.availableDemo) ?? new List<RelicData>();
        #else
        return relicList?.list ?? new List<RelicData>();
        #endif
    }
    
    /// <summary>
    /// 指定されたアクトのコンテンツデータリストを取得
    /// </summary>
    /// <param name="contentLists">コンテンツデータリスト</param>
    /// <param name="act">アクト番号</param>
    /// <returns>対応するコンテンツデータリスト</returns>
    public ContentDataList GetContentDataListForAct(List<ContentDataList> contentLists, int act)
    {
        if (contentLists == null || contentLists.Count == 0)
            return new ContentDataList { list = new List<ContentData>() };
        
        // TODO: 将来的にアクトベースの選択を実装
        // 現在は最初のリストを返す
        var index = Mathf.Clamp(0, 0, contentLists.Count - 1);
        return contentLists[index];
    }
    
    /// <summary>
    /// ランダムなレアリティを確率に基づいて取得する
    /// </summary>
    /// <param name="random">0.0～1.0の乱数値</param>
    /// <returns>確率に基づいたレアリティ</returns>
    public Rarity GetRandomRarity(float random)
    {
        if (random < commonProbability) return Rarity.Common;
        if (random < uncommonProbability) return Rarity.Uncommon;
        if (random < rareProbability) return Rarity.Rare;
        return Rarity.Epic;
    }
    
    /// <summary>
    /// レアリティに基づいた価格を取得する
    /// </summary>
    /// <param name="rarity">レアリティ</param>
    /// <returns>基本価格</returns>
    public int GetPriceByRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonPrice,
            Rarity.Uncommon => uncommonPrice,
            Rarity.Rare => rarePrice,
            Rarity.Epic => epicPrice,
            Rarity.Legendary => legendaryPrice,
            _ => 100000 // デフォルト高額価格
        };
    }
    
    /// <summary>
    /// ショップアイテムの最終価格を取得する
    /// </summary>
    /// <param name="type">アイテムタイプ</param>
    /// <param name="rarity">レアリティ</param>
    /// <returns>最終価格</returns>
    public int GetShopPrice(Shop.ShopItemType type, Rarity rarity)
    {
        if (type == Shop.ShopItemType.Remove) return ballRemovePrice;
        
        var basePrice = GetPriceByRarity(rarity);
        if (type == Shop.ShopItemType.Ball)
        {
            basePrice = (int)(basePrice * ballPriceMultiplier);
        }
        return basePrice;
    }
}