using UnityEngine;
using System.Collections.Generic;
using R3;

public class RelicManager : MonoBehaviour
{
    [SerializeField] private RelicDataList allRelics;
    [SerializeField] private GameObject relicPrefab;
    [SerializeField] private Transform relicContainer;
    [SerializeField] private Vector3 relicGridPosition;
    [SerializeField] private Vector2Int relicGridSize;
    [SerializeField] private Vector2 relicOffset;
    
    [SerializeField] private List<RelicData> testRelics;
    
    
    private List<RelicData> relics = new();
    private List<IRelicBehavior> behaviors = new();
    private List<RelicUI> relicUIs = new();
    
    private EffectTiming currentTiming;
    
    public void AddRelic(RelicData relic)
    {
        relics.Add(relic);
        var rui = CreateRelicUI(relic);
        ApplyEffect(relic, rui);
    }
    
    public void RemoveRelic(RelicData relic)
    {
        var index = relics.FindIndex(r => r.id == relic.id);
        if(index == -1)
        {
            Debug.LogError("指定されたレリックが存在しません: " + relic.id);
            return;
        }

        var behavior = behaviors[index];
        behavior.RemoveEffect();
        behaviors.Remove(behavior);
        relics.Remove(relic);
        Destroy(relicUIs[index].gameObject);
        relicUIs.RemoveAt(index);
    }
    
    private RelicUI CreateRelicUI(RelicData r)
    {
        var go = Instantiate(relicPrefab, relicContainer);
        go.transform.localPosition = relicGridPosition + new Vector3(relicOffset.x * ((relics.Count - 1) / relicGridSize.y), -relicOffset.y * ((relics.Count - 1) % relicGridSize.y));
        var relicUI = go.GetComponent<RelicUI>();
        relicUI.SetRelicData(r);
        relicUIs.Add(relicUI);
        return relicUI;
    }
    
    private void ApplyEffect(RelicData r, RelicUI rui)
    {
        IRelicBehavior behaviour = null;
        var type = System.Type.GetType(r.className);
        behaviour = this.gameObject.AddComponent(type) as IRelicBehavior;
        if(behaviour == null)
        {
            Debug.LogError("指定されたクラスは存在しないか、IRelicBehaviorを実装していません: " + r.className);
            return;
        }
        
        behaviour.ApplyEffect(rui);
        behaviors.Add(behaviour);
    }

    private void Start()
    {
        allRelics.Register();
        
        foreach (var r in testRelics)
        {
            AddRelic(r);
        }

    }
}
