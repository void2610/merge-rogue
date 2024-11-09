using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "BallDataList", menuName = "Scriptable Objects/BallDataList")]
public class BallDataList : ScriptableObject
{
    [SerializeField]
    public List<BallData> list = new List<BallData>();

    public void Reset()
    {
        list.Clear();
    }

    public void Register()
    {
#if UNITY_EDITOR
        // ScriptableObject (このスクリプト) と同じディレクトリパスを取得
        string path = AssetDatabase.GetAssetPath(this);
        path = System.IO.Path.GetDirectoryName(path);

        // 指定ディレクトリ内の全てのRelicDataを検索
        string[] guids = AssetDatabase.FindAssets("t:BallData", new[] { path });
        
        // 検索結果をリストに追加
        list.Clear();
        foreach (string guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var ballData = AssetDatabase.LoadAssetAtPath<BallData>(assetPath);
            if (ballData != null)
            {
                list.Add(ballData);
            }
        }
#endif
    }

    public List<BallData> GetBallDataFromRarity(BallRarity r)
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
