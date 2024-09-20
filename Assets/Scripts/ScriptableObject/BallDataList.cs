using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
}
