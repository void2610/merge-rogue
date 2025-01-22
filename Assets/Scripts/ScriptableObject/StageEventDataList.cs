using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "StageEventDataList", menuName = "Scriptable Objects/StageEventDataList")]
public class StageEventDataList : ScriptableObject
{
    [SerializeField] public List<StageEventData> list = new ();

    public void Register()
    {
#if UNITY_EDITOR
        // ScriptableObject (このスクリプト) と同じディレクトリパスを取得
        var path = UnityEditor.AssetDatabase.GetAssetPath(this);
        path = System.IO.Path.GetDirectoryName(path);

        // 指定ディレクトリ内の全てのStageEventDataを検索
        var guids = UnityEditor.AssetDatabase.FindAssets("t:StageEventData", new[] { path });

        // 検索結果をリストに追加
        list.Clear();
        foreach (var guid in guids)
        {
            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var stageEventData = UnityEditor.AssetDatabase.LoadAssetAtPath<StageEventData>(assetPath);
            if (stageEventData != null)
            {
                list.Add(stageEventData);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this); // ScriptableObjectを更新
#endif
    }
}