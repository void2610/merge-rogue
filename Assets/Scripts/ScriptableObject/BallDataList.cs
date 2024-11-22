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
    public List<BallData> allBalls = new List<BallData>();
    public List<BallData> ballsExceptNormal = new List<BallData>();
    [SerializeField]
    public BallData normalBall;

    public void Reset()
    {
        allBalls.Clear();
    }

    public void Register()
    {
#if UNITY_EDITOR
        // ScriptableObject (このスクリプト) と同じディレクトリパスを取得
        var path = AssetDatabase.GetAssetPath(this);
        path = System.IO.Path.GetDirectoryName(path);

        // 指定ディレクトリ内の全てのRelicDataを検索
        var guids = AssetDatabase.FindAssets("t:BallData", new[] { path });
        
        // 検索結果をリストに追加
        allBalls.Clear();
        foreach (string guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var ballData = AssetDatabase.LoadAssetAtPath<BallData>(assetPath);
            if (ballData != null)
            {
                allBalls.Add(ballData);
            }
        }
        
        ballsExceptNormal = allBalls.Where(x => x.className != "NormalBall").ToList();
#endif
    }

    public List<BallData> GetBallDataFromRarity(Rarity r)
    {
        var result = new List<BallData>();
        foreach (var bd in allBalls)
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
        return allBalls.FirstOrDefault(bd => bd.className == className);
    }
}
