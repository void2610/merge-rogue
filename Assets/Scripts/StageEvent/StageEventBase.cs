using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public abstract class StageEventBase : MonoBehaviour
{
    [Serializable]
    public class OptionData
    {
        public string description;
        public string resultDescription;
        public Action Action;
        public Func<bool> IsAvailable = () => true;
        public bool isEndless = false;
    }
    
    public string EventName;
    public string MainDescription;
    public List<OptionData> Options;

    public abstract void Init();
}
     