using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ContentProvider : MonoBehaviour
{
    [Serializable]
    private class ContentData
    { 
        public Object data;
        public float probability;
    }
    [Serializable]
    private class ContentDataList
    { 
        public List<ContentData> list;
    }
    public static ContentProvider Instance { get; private set; }
    
    [SerializeField] private List<ContentDataList> enemyList;
    [SerializeField] private List<ContentDataList> bossList;
    [SerializeField] private List<ContentDataList> eventList;
    [SerializeField] private BallDataList ballList;
    [SerializeField] private RelicDataList relicList;
    
    private int _act = 0;

    /// <summary>
    /// StageEventをランダムで取得する
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public StageEventBase GetRandomEvent()
    {
        var eventScript = GetRandomObjectFromList(eventList);
        var type = Type.GetType(eventScript.name);
        if (type != null && type.IsSubclassOf(typeof(StageEventBase)))
        {
            var eventInstance = this.gameObject.AddComponent(type) as StageEventBase;
            if (eventInstance)
            {
                eventInstance.Init();
                return eventInstance;
            }
        }

        throw new Exception("Event not found");
    }
    
    /// <summary>
    /// 敵のPrefabをランダムで取得する
    /// </summary>
    /// <returns></returns>
    public GameObject GetRandomEnemy()
    {
        var enemy = GetRandomObjectFromList(enemyList) as GameObject;
        return Instantiate(enemy);
    }
    
    /// <summary>
    /// ボスのPrefabをランダムで取得する
    /// </summary>
    /// <returns></returns>
    public GameObject GetRandomBoss()
    {
        var boss = GetRandomObjectFromList(bossList) as GameObject;
        return Instantiate(boss);
    }
    
    /// <summary>
    /// ランダムなレアリティを取得する
    /// </summary>
    public Rarity GetRandomRarity()
    {
        var r = GameManager.Instance.RandomRange(0.0f, 1.0f);
        return r switch
        {
            < 0.40f => Rarity.Common,   // 40%
            < 0.70f => Rarity.Uncommon, // 30%
            < 0.85f => Rarity.Rare,     // 15%
            _ => Rarity.Epic,     // 15%
            // _ => Rarity.Legendary       //  5%
        };
    }

    /// <summary>
    /// ノーマルボール以外のボールを取得する
    /// </summary>
    public List<BallData> GetBallListExceptNormal() => ballList.list.Where(bd => bd.className != "NormalBall").ToList();
    
    /// <summary>
    /// ノーマルボールを取得する
    /// </summary>
    public BallData GetNormalBallData() => ballList.list.Find(bd => bd.className == "NormalBall");
    
    /// <summary>
    /// クラス名からボールデータを取得する
    /// </summary>
    public BallData GetBallDataFromClassName(string className) => ballList.GetBallDataFromClassName(className);
    
    /// <summary>
    /// RelicDataをランダムで取得する
    /// </summary>
    /// <returns></returns>
    public RelicData GetRandomRelic() => GetRandomRelicDataByRarity(GetRandomRarity());

    /// <summary>
    /// 指定された名前のRelicDataを取得する
    /// </summary>
    /// <param name="n">レリックの名前</param>
    /// <returns></returns>
    public RelicData GetRelicByClassName(string n)
    {
        var r = relicList.list.Find(relic => relic.name == n);
        if (!r) throw new Exception("Relic not found");
        return r;
    }
    
    /// <summary>
    /// 指定されたレアリティのRelicDataを全て取得する
    /// </summary>
    /// <param name="r">レアリティ</param>
    /// <returns></returns>
    public List<RelicData> GetRelicDataByRarity(Rarity r) => relicList.list.Where(bd => bd.rarity == r).ToList();
    
    /// <summary>
    /// ショップの売値を返す
    /// </summary>
    /// <param name="type">アイテムの種類</param>
    /// <param name="rarity">アイテムのレアリティ</param>
    public static int GetSHopPrice(Shop.ShopItemType type, Rarity rarity)
    {
        if(type == Shop.ShopItemType.Remove) return GetBallRemovePrice();
        var price = rarity switch
        {
            Rarity.Common => 10,
            Rarity.Uncommon => 20,
            Rarity.Rare => 50,
            Rarity.Epic => 100,
            Rarity.Legendary => 500,
            _ => 100000
        };
        if (type == Shop.ShopItemType.Ball) price = (int)(price * 0.75f);
        return price;
    }
    
    /// <summary>
    /// ボール除去の価格を返す
    /// </summary>
    public static int GetBallRemovePrice() => 25;
    
    /// <summary>
    /// ボール強化の価格を返す
    /// </summary>
    public static int GetBallUpgradePrice() => 10;
    
    /// <summary>
    /// アクトを進める
    /// </summary>
    public void AddAct() => _act++;
    
    // =============================================================================================================
    // PRIVATE METHODS
    // =============================================================================================================
    
    private RelicData GetRandomRelicDataByRarity(Rarity r)
    {
        // 既に取得済みのレリックは低確率にする
        var current = RelicManager.Instance.GetCurrentRelics();
        // 指定されたレアリティのリストを取得
        var targets = relicList.list.Where(bd => bd.rarity == r).ToList();
        var randomIndex = GameManager.Instance.RandomRange(0, targets.Count);
        var relic = targets[randomIndex];
        // 3回だけ再試行する
        for (var i = 0; i < 3; i++)
        {
            if (current.Contains(relic)) relic = targets[GameManager.Instance.RandomRange(0, targets.Count)];
            else break;
        }
        return relic;
    }
    
    private Object GetRandomObjectFromList(List<ContentDataList> contentLists)
    {
        // アクトに基づいてリストを選択
        if (contentLists.Count <= _act) _act = contentLists.Count - 1;

        var contentDataList = contentLists[_act].list;
        var totalProbability = contentDataList.Sum(d => d.probability);
        var randomPoint = GameManager.Instance.RandomRange(0.0f, totalProbability);

        foreach (var contentData in contentDataList)
        {
            if (randomPoint < contentData.probability)
            {
                // データをそのまま返す
                return contentData.data;
            }
            randomPoint -= contentData.probability;
        }

        // フォールバックとして最後の要素を返す
        return contentDataList.Last().data;
    }
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        
        relicList.Register();
    }
}
