using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EventDataList", menuName = "Scriptable Objects/EventDataList")]
public class EventDataList : ScriptableObject
{
    [FormerlySerializedAs("eventDataList")] [SerializeField] 
    public List<EventData> list = new ();
    
    public void Register()
    {
#if UNITY_EDITOR
        // ScriptableObject (このスクリプト) と同じディレクトリパスを取得
        var path = UnityEditor.AssetDatabase.GetAssetPath(this);
        path = System.IO.Path.GetDirectoryName(path);

        // 指定ディレクトリ内の全てのEventDataを検索
        var guids = UnityEditor.AssetDatabase.FindAssets("t:EventData", new[] { path });

        // 検索結果をリストに追加
        list.Clear();
        foreach (var guid in guids)
        {
            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var eventData = UnityEditor.AssetDatabase.LoadAssetAtPath<EventData>(assetPath);
            if (eventData != null)
            {
                list.Add(eventData);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this); // ScriptableObjectを更新
#endif
    }
}
