using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemySelector : MonoBehaviour
{
    [Serializable]
    private class EnemyData
    { 
        public GameObject prefab;
        public float probability;
    }
    [Serializable]
    private class EnemyDataList
    { 
        public List<EnemyData> list;
    }
    
   public static EnemySelector Instance { get; private set; }
   
    [SerializeField] private List<EnemyDataList> enemyList;
   
    public List<GameObject> GetEnemy(int count, int act)
    {
        var enemies = new List<GameObject>();
        if(enemyList.Count <= act) act = enemyList.Count - 1;
        
        for (int i = 0; i < count; i++)
        {
            var total = enemyList[act].list.Sum(enemyData => enemyData.probability);
            var randomPoint = GameManager.Instance.RandomRange(0.0f, total);

            foreach (var enemyData in enemyList[act].list)
            {
                if (randomPoint < enemyData.probability)
                {
                    enemies.Add(Instantiate(enemyData.prefab));
                    break;
                }
                randomPoint -= enemyData.probability;
            }
        }
        return enemies;
    }
   
   private void Awake()
   {
       if (Instance == null)
           Instance = this;
       else
           Destroy(this);
   }
}
