using UnityEngine;
using System.Collections.Generic;
using R3;
using UnityEngine.Serialization;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;
    
    [SerializeField] public RelicDataList allRelicDataList;
    [SerializeField] private GameObject relicPrefab;
    [SerializeField] private Transform relicContainer;
    [SerializeField] private Vector3 relicGridPosition;
    [SerializeField] private Vector2Int relicGridSize;
    [SerializeField] private Vector2 relicOffset;
    
    [SerializeField] private List<RelicData> testRelics;
    
    
    private readonly List<RelicData> _relics = new();
    private readonly List<RelicBase> _behaviors = new();
    private readonly List<RelicUI> _relicUIs = new();
    
    public void AddRelic(RelicData relic)
    {
        _relics.Add(relic);
        var rui = CreateRelicUI(relic);
        ApplyEffect(relic, rui);
    }
    
    public void RemoveRelic(RelicData relic)
    {
        var index = _relics.FindIndex(r => r.id == relic.id);
        if(index == -1)
        {
            Debug.LogError("指定されたレリックが存在しません: " + relic.id);
            return;
        }

        var behavior = _behaviors[index];
        behavior.RemoveEffect();
        _behaviors.Remove(behavior);
        _relics.Remove(relic);
        Destroy(_relicUIs[index].gameObject);
        _relicUIs.RemoveAt(index);
    }
    
    private RelicUI CreateRelicUI(RelicData r)
    {
        var go = Instantiate(relicPrefab, relicContainer);
        go.transform.localPosition = relicGridPosition + new Vector3(relicOffset.x * ((_relics.Count - 1) / relicGridSize.y), -relicOffset.y * ((_relics.Count - 1) % relicGridSize.y));
        var relicUI = go.GetComponent<RelicUI>();
        relicUI.SetRelicData(r);
        _relicUIs.Add(relicUI);
        return relicUI;
    }
    
    private void ApplyEffect(RelicData r, RelicUI rui)
    {
        RelicBase behaviour = null;
        var type = System.Type.GetType(r.className);
        behaviour = this.gameObject.AddComponent(type) as RelicBase;
        if(!behaviour)
        {
            Debug.LogError("指定されたクラスは存在しません: " + r.className);
            return;
        }
        
        behaviour.Init(rui);
        _behaviors.Add(behaviour);
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        allRelicDataList.Register();
    }

    private void Start()
    {
        if (UnityEngine.Application.isEditor)
        {
            foreach (var r in testRelics)
            {
                AddRelic(r);
            }
        }
    }
}
