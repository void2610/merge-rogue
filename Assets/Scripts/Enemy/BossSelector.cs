using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BossSelector : MonoBehaviour
{
    [Serializable]
    private class BossData
    { 
        public List<GameObject> bossList;
    }
   public static BossSelector Instance { get; private set; }
   
    [SerializeField] private List<BossData> bossList;
   
    public GameObject GetBossEnemy(int act)
    {
        if(bossList.Count <= act) act = bossList.Count - 1;
        var r = Random.Range(0, bossList[act].bossList.Count);
        var boss = Instantiate(bossList[act].bossList[r]);
        return boss;
    }
   
   private void Awake()
   {
       if (Instance == null)
           Instance = this;
       else
           Destroy(this);
   }
}
