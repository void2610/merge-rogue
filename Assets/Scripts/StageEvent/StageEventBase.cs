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
    protected IRandomService RandomService;
    protected IInventoryService InventoryService;
    protected IRelicService RelicService;
    
    [Inject]
    public void InjectDependencies(IContentService contentService, IRandomService randomService, IInventoryService inventoryService, IRelicService relicService)
    {
        ContentService = contentService;
        RandomService = randomService;
        InventoryService = inventoryService;
        RelicService = relicService;
    }

    public abstract void Init();
}
     