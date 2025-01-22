using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private List<ContentDataList> relicList;
    [SerializeField] private List<ContentDataList> eventList;
    [SerializeField] private List<ContentDataList> ballList;
    
    private int _act = 0;
    
    public GameObject GetRandomEnemy()
    {
        if(enemyList.Count <= _act) _act = enemyList.Count - 1;
        
        var total = enemyList[_act].list.Sum(d => d.probability);
        var randomPoint = GameManager.Instance.RandomRange(0.0f, total);

        foreach (var enemyData in enemyList[_act].list)
        {
            if (randomPoint < enemyData.probability)
            {
                var prefab = enemyData.data as GameObject;
                return Instantiate(prefab);
                break;
            }
            randomPoint -= enemyData.probability;
        }
        
        var lastPrefab = enemyList[_act].list.Last().data as GameObject;
        return Instantiate(lastPrefab);
    }
    
    public GameObject GetRandomBoss()
    {
        if(bossList.Count <= _act) _act = bossList.Count - 1;
        
        var total = bossList[_act].list.Sum(d => d.probability);
        var randomPoint = GameManager.Instance.RandomRange(0.0f, total);

        foreach (var bossData in bossList[_act].list)
        {
            if (randomPoint < bossData.probability)
            {
                var prefab = bossData.data as GameObject;
                return Instantiate(prefab);
                break;
            }
            randomPoint -= bossData.probability;
        }
        
        var lastPrefab = bossList[_act].list.Last().data as GameObject;
        return Instantiate(lastPrefab);
    }
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}
