using System;
using System.Collections.Generic;
using UnityEngine;
using  Alchemy.Inspector;

public enum EventBehaviourType
{
    AddHealth,
    SubHealth,
    AddCoin,
    SubCoin,
    GetBall,
    RemoveBall,
    GetRelic,
}

[Serializable]
public class EventBehaviour
{
    public EventBehaviourType behaviourType;
    
    [ShowIf("IsIntValue")]
    public int intValue;
    [ShowIf("IsRelicValue")]
    public RelicData relicValue;
    [ShowIf("IsBallValue")]
    public BallData ballValue;
    
    private bool IsIntValue()
    {
        return behaviourType is EventBehaviourType.AddHealth or EventBehaviourType.SubHealth or EventBehaviourType.AddCoin or EventBehaviourType.SubCoin or EventBehaviourType.RemoveBall;
    }
    private bool IsRelicValue()
    {
        return behaviourType is EventBehaviourType.GetRelic;
    }
    private bool IsBallValue()
    {
        return behaviourType is EventBehaviourType.GetBall;
    }
}

[Serializable]
public class EventOption
{
    public string optionDescription;
    public List<EventBehaviour> behaviours;
}

[CreateAssetMenu(fileName = "EventData", menuName = "Scriptable Objects/EventData")]
public class EventData : ScriptableObject
{
    public string eventName;
    public string eventDescription;
    public List<EventOption> options;
}
