using UnityEngine;
using System.Collections.Generic;

public class RelicManager : MonoBehaviour
{
    [SerializeField] RelicData testRelic;
    
    private List<RelicData> relics = new();
    
    public void AddRelic(RelicData relic)
    {
        relics.Add(relic);
        ApplyEffect(relic);
    }
    
    public void RemoveRelic(RelicData relic)
    {
        relics.Remove(relic);
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
    }

    private void Start()
    {
        AddRelic(testRelic);
    }
}
