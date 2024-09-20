using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "BallDataList", menuName = "Scriptable Objects/BallDataList")]
public class BallDataList : ScriptableObject
{
    [SerializeField]
    public List<BallData> list = new List<BallData>();

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

    public List<BallData> GetBallDataFromRarity(BallData.BallRarity r)
    {
        List<BallData> result = new List<BallData>();
        foreach (BallData bd in list)
        {
            if (bd.rarity == r)
            {
                result.Add(bd);
            }
        }
        return result;
    }

    public BallData GetNormalBallData()
    {
        return list.FirstOrDefault(x => x.className == "NormalBall");
    }
}
