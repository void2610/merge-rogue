using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StatusEffectDataList", menuName = "StatusEffect/StatusEffectDataList")]
public class StatusEffectDataList : ScriptableObject
{
    [SerializeField] 
    public List<StatusEffectData> list = new();
    
    private Dictionary<StatusEffectType, StatusEffectData> _dataCache;

    public void Register()
    {
#if UNITY_EDITOR
        // ScriptableObject (このスクリプト) と同じディレクトリパスを取得
        var path = AssetDatabase.GetAssetPath(this);
        path = System.IO.Path.GetDirectoryName(path);

        // 指定ディレクトリ内の全てのStatusEffectDataを検索
        var guids = AssetDatabase.FindAssets("t:StatusEffectData", new[] { path });

        // 検索結果をリストに追加
        list.Clear();
        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var statusEffectData = AssetDatabase.LoadAssetAtPath<StatusEffectData>(assetPath);
            if (statusEffectData != null)
            {
                list.Add(statusEffectData);
            }
        }
        
        // タイプでソート
        list = list.OrderBy(x => x.type).ToList();

        UnityEditor.EditorUtility.SetDirty(this); // ScriptableObjectを更新
#endif
    }
    
    /// <summary>
    /// StatusEffectTypeからデータを取得
    /// </summary>
    public StatusEffectData GetStatusEffectData(StatusEffectType type)
    {
        if (_dataCache == null)
        {
            _dataCache = new Dictionary<StatusEffectType, StatusEffectData>();
            foreach (var data in list)
            {
                _dataCache[data.type] = data;
            }
        }
        
        return _dataCache.TryGetValue(type, out var result) ? result : null;
    }
    
    /// <summary>
    /// クラス名からデータを取得
    /// </summary>
    public StatusEffectData GetStatusEffectDataFromClassName(string className)
    {
        return list.FirstOrDefault(data => data.className == className);
    }
}