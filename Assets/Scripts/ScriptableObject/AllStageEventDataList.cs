using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全てのステージイベントデータを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllStageEventDataList", menuName = "ScriptableObjects/AllStageEventDataList")]
public class AllStageEventDataList : ScriptableObject
{
    [SerializeField] private List<StageEventData> stageEventDataList = new List<StageEventData>();
    
    /// <summary>
    /// 全てのステージイベントデータ
    /// </summary>
    public List<StageEventData> StageEventDataList => stageEventDataList;
    
    /// <summary>
    /// IDからステージイベントデータを取得
    /// </summary>
    public StageEventData GetEventData(string eventId)
    {
        return stageEventDataList.Find(data => data.eventId == eventId);
    }

    /// <summary>
    /// 全てのステージイベントデータを登録
    /// </summary>
    public void Register()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(stageEventDataList, sortKeySelector: data => data.eventId);
#endif
    }
}