using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "RelicDataList", menuName = "Scriptable Objects/RelicDataList")]
public class RelicDataList : ScriptableObject
{
    public static readonly List<RelicData> list = new List<RelicData>();
    
    public void Register()
    {
#if UNITY_EDITOR
        // ScriptableObject (このスクリプト) と同じディレクトリパスを取得
        var path = AssetDatabase.GetAssetPath(this);
        path = System.IO.Path.GetDirectoryName(path);

        // 指定ディレクトリ内の全てのRelicDataを検索
        var guids = AssetDatabase.FindAssets("t:RelicData", new[] { path });
        
        // 検索結果をリストに追加
        list.Clear();
        foreach (string guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var relicData = AssetDatabase.LoadAssetAtPath<RelicData>(assetPath);
            if (relicData != null)
            {
                list.Add(relicData);
            }
        }
#endif
    }
        
    public　static List<RelicData> GetRelicDataFromRarity(Rarity r)
    {
        return list.Where(bd => bd.rarity == r).ToList();
    }
}
