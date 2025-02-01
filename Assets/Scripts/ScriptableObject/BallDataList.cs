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
        // ScriptableObject (このスクリプト) と同じディレクトリパスを取得
        var path = AssetDatabase.GetAssetPath(this);
        path = System.IO.Path.GetDirectoryName(path);

        // 指定ディレクトリ内の全てのRelicDataを検索
        var guids = AssetDatabase.FindAssets("t:BallData", new[] { path });

        // 検索結果をリストに追加
        list.Clear();
        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var ballData = AssetDatabase.LoadAssetAtPath<BallData>(assetPath);
            if (ballData != null)
            {
                list.Add(ballData);
            }
        }
        // レアリティでソート
        list = list.OrderBy(x => x.rarity).ToList();

        UnityEditor.EditorUtility.SetDirty(this); // ScriptableObjectを更新
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
    
    public List<BallData> GetBallListExceptNormal()
    {
        return list.Where(x => x.className != "NormalBall").ToList();
    }
}
