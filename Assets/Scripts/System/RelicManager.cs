using UnityEngine;
using System.Collections.Generic;

public class RelicManager : MonoBehaviour
{
    [SerializeField] private RelicDataList allRelics;
    [SerializeField] private GameObject relicPrefab;
    [SerializeField] private Transform relicContainer;
    [SerializeField] private Vector3 relicGridPosition;
    [SerializeField] private Vector2Int relicGridSize;
    [SerializeField] private Vector2 relicOffset;
    
    
    private List<RelicData> relics = new();
    private List<IRelicBehavior> behaviors = new();
    
    public void AddRelic(RelicData relic)
    {
        relics.Add(relic);
        CreateRelicUI(relic);
        ApplyEffect(relic);
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
    }
    
    private void CreateRelicUI(RelicData r)
    {
        var go = Instantiate(relicPrefab, relicContainer);
        go.transform.localPosition = relicGridPosition + new Vector3(relicOffset.x * ((relics.Count - 1) / relicGridSize.y), -relicOffset.y * ((relics.Count - 1) % relicGridSize.y));
        var relicUI = go.GetComponent<RelicUI>();
        relicUI.SetRelicData(r);
    }
    
    private void ApplyEffect(RelicData r)
    {
        IRelicBehavior behaviour = null;
        var type = System.Type.GetType(r.className);
        behaviour = this.gameObject.AddComponent(type) as IRelicBehavior;
        if(behaviour == null)
        {
            Debug.LogError("指定されたクラスは存在しないか、IRelicBehaviorを実装していません: " + r.className);
            return;
        }
        
        behaviour.ApplyEffect();
        behaviors.Add(behaviour);
    }

    private void Start()
    {
        allRelics.Register();

        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            AddRelic(allRelics.list[0]);
        }
    }
}
