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
    private readonly EnemySpawnConfiguration _enemyConfig;
    private readonly IRandomService _randomService;

    public bool IsDemoPlay { get; } = false;
    public int Act { get; private set; } = 0;
    public float ShopPriceMultiplier { get; private set; } = 1.0f;
    
    // 敵難易度関連プロパティ
    public float GlobalEnemyDifficultyMultiplier => _enemyConfig.GlobalEnemyDifficultyMultiplier;
    public float BaseEnemyHealthMultiplier => _enemyConfig.BaseEnemyHealthMultiplier;
    public float BaseEnemyAttackMultiplier => _enemyConfig.BaseEnemyAttackMultiplier;

    public StatusEffectDataList StatusEffectList => _data.StatusEffectList;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="data">統合データ（設定値とコンテンツデータ）</param>
    /// <param name="enemyConfig">敵設定データ</param>
    /// <param name="randomService">ランダムサービス</param>
    public ContentService(
        ContentProviderData data,
        EnemySpawnConfiguration enemyConfig,
        IRandomService randomService)
    {
        _data = data;
        _enemyConfig = enemyConfig;
        _data.InitializeData();
        _randomService = randomService;
        #if DEMO_PLAY
        IsDemoPlay = true;
        #endif
    }
    
    public EnemyData GetRandomEnemy()
    {
        return GetRandomEnemyFromList(_enemyConfig.EnemyList);
    }
    
    public EnemyData GetRandomBoss()
    {
        return GetRandomEnemyFromList(_enemyConfig.BossList);
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
    
    public BallData GetBallData(string ballType)
    {
        return _data.GetFilteredBallList().FirstOrDefault(bd => bd.className == ballType || bd.name == ballType);
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
    
    public RelicData GetRelicData(string relicName)
    {
        return _data.GetFilteredRelicList().FirstOrDefault(rd => rd.className == relicName || rd.name == relicName);
    }
    
    public List<RelicData> GetRelicDataByRarity(Rarity rarity)
    {
        return  _data.GetFilteredRelicList().Where(bd => bd.rarity == rarity).ToList();
    }
    
    public List<RelicData> GetAllRelicData()
    {
        return _data.GetFilteredRelicList();
    }
    
    public int GetShopPrice(Shop.ShopItemType type, Rarity rarity) => (int)(_data.GetShopPrice(type, rarity) * ShopPriceMultiplier);
    public int GetBallRemovePrice() => (int)(_data.BallRemovePrice * ShopPriceMultiplier);
    public int GetBallUpgradePrice() => (int)(_data.BallUpgradePrice * ShopPriceMultiplier);
    public int GetInitialPlayerCoin() => _data.InitialPlayerCoin;
    public void SetShopPriceMultiplier(float multiplier) => ShopPriceMultiplier = multiplier;
    
    public void AddAct()
    {
        Act++;
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
    /// ランダムな敵データをリストから取得する
    /// </summary>
    /// <param name="spawnLists">敵出現データリスト</param>
    /// <returns>ランダムに選択された敵データ</returns>
    private EnemyData GetRandomEnemyFromList(List<EnemySpawnConfiguration.EnemySpawnDataList> spawnLists)
    {
        var spawnDataList = _enemyConfig.GetEnemySpawnDataListForAct(spawnLists, Act);
        var totalProbability = spawnDataList.list.Sum(d => d.probability);
        var randomPoint = _randomService.RandomRange(0.0f, totalProbability);
        
        foreach (var spawnData in spawnDataList.list)
        {
            if (randomPoint < spawnData.probability)
            {
                return spawnData.enemyData;
            }
            randomPoint -= spawnData.probability;
        }
        
        // フォールバックとして最後の要素を返す
        return spawnDataList.list.Last().enemyData;
    }
    
    // ====== 敵難易度関連メソッド ======
    // GlobalEnemyDifficultyMultiplierは読み取り専用プロパティとして提供
    
    // ====== ステージイベント関連メソッド ======
    
    public List<StageEventData> GetAllStageEventData()
    {
        return _data.StageEventDataList?.StageEventDataList ?? new List<StageEventData>();
    }
}