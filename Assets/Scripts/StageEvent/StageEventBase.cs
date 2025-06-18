using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using VContainer;

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
    
    protected IContentService ContentService;
    
    [Inject]
    public void InjectDependencies(IContentService contentService)
    {
        ContentService = contentService;
    }

    public abstract void Init();
}
     