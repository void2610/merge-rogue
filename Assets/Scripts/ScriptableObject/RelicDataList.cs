using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RelicDataList", menuName = "Scriptable Objects/RelicDataList")]
public class RelicDataList : ScriptableObject
{
    [FormerlySerializedAs("relicDataList")] [SerializeField] 
    public List<RelicData> list = new ();

    // リストを取得する
    public List<RelicData> GetRelicDataFromRarity(Rarity r)
    {
        return list.Where(bd => bd.rarity == r).ToList();
    }

    public void Register()
    {
#if UNITY_EDITOR
        // ScriptableObject (このスクリプト) と同じディレクトリパスを取得
        var path = UnityEditor.AssetDatabase.GetAssetPath(this);
        path = System.IO.Path.GetDirectoryName(path);

        // 指定ディレクトリ内の全てのRelicDataを検索
        var guids = UnityEditor.AssetDatabase.FindAssets("t:RelicData", new[] { path });

        // 検索結果をリストに追加
        list.Clear();
        foreach (var guid in guids)
        {
            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var relicData = UnityEditor.AssetDatabase.LoadAssetAtPath<RelicData>(assetPath);
            if (relicData != null)
            {
                list.Add(relicData);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this); // ScriptableObjectを更新
#endif
    }
}
