using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "RelicDataList", menuName = "Scriptable Objects/RelicDataList")]
public class RelicDataList : ScriptableObject
{
    [SerializeField]
    public List<RelicData> list = new List<RelicData>();

    public void Reset()
    {
        list.Clear();
    }

    public void InitId()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].id = i;
        }
    }

    public List<RelicData> GetRelicDataFromRarity(RelicData.RelicRarity r)
    {
        var result = new List<RelicData>();
        foreach (var bd in list)
        {
            if (bd.rarity == r)
            {
                result.Add(bd);
            }
        }
        return result;
    }
}
