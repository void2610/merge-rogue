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
        this.RegisterAssetsInSameDirectory(list, sortKeySelector: data => data.name);
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