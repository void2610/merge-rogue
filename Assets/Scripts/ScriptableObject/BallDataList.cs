using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BallDataList", menuName = "Scriptable Objects/BallDataList")]
public class BallDataList : ScriptableObject
{
    [FormerlySerializedAs("ballDataList")] [SerializeField] 
    public List<BallData> list = new ();

    public void Register()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(list, sortKeySelector: data => data.name);
#endif
    }

    public List<BallData> GetBallDataFromRarity(Rarity r)
    {
        var result = new List<BallData>();
        foreach (var bd in list)
        {
            if (bd.rarity == r)
            {
                result.Add(bd);
            }
        }
        return result;
    }
    
    public BallData GetBallDataFromClassName(string className)
    {
        return list.FirstOrDefault(bd => bd.className == className);
    }
}
