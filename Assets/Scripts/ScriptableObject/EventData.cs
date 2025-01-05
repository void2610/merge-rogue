using System;
using System.Collections.Generic;
using UnityEngine;
using  Alchemy.Inspector;

public enum EventOptionType
{
    AddHealth,
    SubHealth,
    AddGold,
    SubGold,
    GetBall,
    RemoveBall,
    GetRelic,
}

[Serializable]
public class EventOption
{
    public string optionDescription;
    public EventOptionType optionType;
    
    [ShowIf("IsIntValue")]
    public int intValue;
    [ShowIf("IsRelicValue")]
    public RelicData relicValue;
    [ShowIf("IsBallValue")]
    public BallData ballValue;
    
    private bool IsIntValue()
    {
        return optionType is EventOptionType.AddHealth or EventOptionType.SubHealth or EventOptionType.AddGold or EventOptionType.SubGold;
    }
    private bool IsRelicValue()
    {
        return optionType is EventOptionType.GetRelic;
    }
    private bool IsBallValue()
    {
        return optionType is EventOptionType.GetBall;
    }
}

[CreateAssetMenu(fileName = "EventData", menuName = "Scriptable Objects/EventData")]
public class EventData : ScriptableObject
{
    public string eventName;
    public string eventDescription;
    public List<EventOption> options;
}
