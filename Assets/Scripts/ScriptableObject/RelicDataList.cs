using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "RelicDataList", menuName = "Scriptable Objects/RelicDataList")]
public class RelicDataList : ScriptableObject
{
    [SerializeField]
    public List<RelicData> list = new List<RelicData>();

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
        string[] guids = AssetDatabase.FindAssets("t:RelicData", new[] { path });
        
        // 検索結果をリストに追加
        list.Clear();
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            RelicData relicData = AssetDatabase.LoadAssetAtPath<RelicData>(assetPath);
            if (relicData != null)
            {
                list.Add(relicData);
            }
        }
#endif
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
