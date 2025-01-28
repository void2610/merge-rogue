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
    [SerializeField] private RelicDataList relicList;
    [SerializeField] private List<ContentDataList> eventList;
    [SerializeField] private List<ContentDataList> ballList;
    
    private int _act = 0;

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
    
    public GameObject GetRandomEnemy()
    {
        var enemy = GetRandomObjectFromList(enemyList) as GameObject;
        return Instantiate(enemy);
    }
    
    public GameObject GetRandomBoss()
    {
        var boss = GetRandomObjectFromList(bossList) as GameObject;
        return Instantiate(boss);
    }
    
    public RelicData GetRandomRelic()
    {
        // TODO: 取得しているレリックの出現確率を調整する
        // 全てのレリックは同じ確率
        var randomIndex = GameManager.Instance.RandomRange(0, relicList.list.Count);
        return relicList.list[randomIndex];
    }

    public RelicData GetRelicByName(string n)
    {
        var r = relicList.list.Find(relic => relic.name == n);
        if (!r) throw new Exception("Relic not found");
        return r;
    }
    
    public RelicData GetRandomRelicDataByRarity(Rarity r)
    {
        var relics = relicList.list.Where(bd => bd.rarity == r).ToList();
        return relics[GameManager.Instance.RandomRange(0, relics.Count)];
    }
    
    public List<RelicData> GetRelicDataByRarity(Rarity r)
    {
        return relicList.list.Where(bd => bd.rarity == r).ToList();
    }
    
    /// <summary>
    /// ショップの売値を返す
    /// </summary>
    /// <param name="type">アイテムの種類</param>
    /// <param name="rarity">アイテムのレアリティ</param>
    public static int GetSHopPrice(Shop.ShopItemType type, Rarity rarity)
    {
        if(type == Shop.ShopItemType.Remove) return 50;
        var price = rarity switch
        {
            Rarity.Common => 10,
            Rarity.Uncommon => 20,
            Rarity.Rare => 50,
            Rarity.Epic => 100,
            Rarity.Legendary => 500,
            _ => 100000
        };
        if (type == Shop.ShopItemType.Ball) price = (int)(price * 1.5f);
        return price;
    }
    
    /// <summary>
    /// ボール除去の価格を返す
    /// </summary>
    public static int GetBallRemovePrice()
    {
        return 75;
    }
    
    public void AddAct() => _act++;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        
        relicList.Register();
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
}
