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
        var obj = GetRandomObjectFromList(eventList) as StageEventData;
        if (!obj) throw new Exception("Event is null");
        StageEventBase e = null;
        var type = System.Type.GetType(obj.className);
        e = this.gameObject.AddComponent(type) as StageEventBase;
        if(!e) throw new Exception("Event is not a subclass of StageEventBase");
        
        e.Init();
        return e;
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
