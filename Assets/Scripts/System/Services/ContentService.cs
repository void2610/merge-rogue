using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// コンテンツ提供サービスの完全実装クラス
/// ContentProviderDataとFactoryパターンを使った純粋なC#実装
/// </summary>
public class ContentService : IContentService
{
    private readonly ContentProviderData _data;
    private readonly IRandomService _randomService;

    public int Act { get; private set; } = 0;
    public float ShopPriceMultiplier { get; private set; } = 1.0f;

    public StatusEffectDataList StatusEffectList => _data.StatusEffectList;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="data">統合データ（設定値とコンテンツデータ）</param>
    /// <param name="randomService">ランダムサービス</param>
    public ContentService(
        ContentProviderData data,
        IRandomService randomService)
    {
        _data = data;
        _data.InitializeData();
        _randomService = randomService;
    }
    
    /// <summary>
    /// StageEventをランダムで取得する
    /// </summary>
    /// <returns></returns>
    public Type GetRandomEventType()
    {
        var eventScript = GetRandomObjectFromList(_data.EventList);
        var type = Type.GetType(eventScript.name);
        if (type != null && type.IsSubclassOf(typeof(StageEventBase)))
        {
            return type;
        } 
        throw new Exception("Event not found or not a valid StageEventBase subclass");
    } 
    
    public EnemyData GetRandomEnemy()
    {
        var enemy = GetRandomObjectFromList(_data.EnemyList) as EnemyData;
        return enemy;
    }
    
    public EnemyData GetRandomBoss()
    {
        var boss = GetRandomObjectFromList(_data.BossList) as EnemyData;
        return boss;
    }
    
    public Rarity GetRandomRarity()
    {
        var r = _randomService.RandomRange(0.0f, 1.0f);
        return _data.GetRandomRarity(r);
    }
    
    public List<BallData> GetBallListExceptNormal()
    {
        return _data.GetFilteredBallList().Where(bd => bd.className != "NormalBall").ToList();
    }
    
    public BallData GetNormalBallData()
    {
        return _data.GetFilteredBallList().Find(bd => bd.className == "NormalBall");
    }
    
    public BallData GetBallDataFromClassName(string className)
    {
        return _data.GetFilteredBallList().FirstOrDefault(bd => bd.className == className);
    }
    
    public RelicData GetRandomRelic()
    {
        return GetRandomRelicDataByRarity(GetRandomRarity());
    }
    
    public RelicData GetRelicByClassName(string className)
    {
        var r = _data.GetFilteredRelicList().Find(relic => relic.name == className);
        if (!r) throw new Exception("Relic not found");
        return r;
    }
    
    public List<RelicData> GetRelicDataByRarity(Rarity rarity)
    {
        return  _data.GetFilteredRelicList().Where(bd => bd.rarity == rarity).ToList();
    }
    
    public int GetShopPrice(Shop.ShopItemType type, Rarity rarity) => (int)(_data.GetShopPrice(type, rarity) * ShopPriceMultiplier);
    public int GetBallRemovePrice() => (int)(_data.BallRemovePrice * ShopPriceMultiplier);
    public int GetBallUpgradePrice() => (int)(_data.BallUpgradePrice * ShopPriceMultiplier);
    public void SetShopPriceMultiplier(float multiplier) => ShopPriceMultiplier = multiplier;
    
    public void AddAct()
    {
        Act++;
    }
    
    public Sprite GetBallBaseImage(BallShapeType type)
    {
        return _data.BallBaseImages[type];
    }
    
    // ====== プライベートメソッド ======
    
    /// <summary>
    /// 指定されたレアリティのランダムなレリックデータを取得（重複回避あり）
    /// </summary>
    /// <param name="rarity">レアリティ</param>
    /// <returns>レリックデータ</returns>
    private RelicData GetRandomRelicDataByRarity(Rarity rarity)
    {
        var targets = _data.GetFilteredRelicList().Where(bd => bd.rarity == rarity).ToList();
        if (targets.Count == 0) throw new Exception($"No relic found for rarity: {rarity}");
        
        var randomIndex = _randomService.RandomRange(0, targets.Count);
        var relic = targets[randomIndex];
        
        // TODO: 重複回避ロジックの実装
        // RelicManagerへの依存を避けるため、基本的なランダム選択のみ
        return relic;
    }
    
    /// <summary>
    /// ランダムなオブジェクトをリストから取得する
    /// </summary>
    /// <param name="contentLists">コンテンツデータリスト</param>
    /// <returns>ランダムに選択されたオブジェクト</returns>
    private Object GetRandomObjectFromList(List<ContentProviderData.ContentDataList> contentLists)
    {
        var contentDataList = _data.GetContentDataListForAct(contentLists, Act);
        var totalProbability = contentDataList.list.Sum(d => d.probability);
        var randomPoint = _randomService.RandomRange(0.0f, totalProbability);
        
        foreach (var contentData in contentDataList.list)
        {
            if (randomPoint < contentData.probability)
            {
                return contentData.data;
            }
            randomPoint -= contentData.probability;
        }
        
        // フォールバックとして最後の要素を返す
        return contentDataList.list.Last().data;
    }
}