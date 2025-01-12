using System;
using System.Collections.Generic;
using UnityEngine;

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
    
    public int intValue;
    public RelicData relicValue;
    public BallData ballValue;
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
