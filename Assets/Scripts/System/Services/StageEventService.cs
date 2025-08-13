using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ステージイベントサービスの実装
/// </summary>
public class StageEventService : IStageEventService
{
    private readonly IContentService _contentService;
    private readonly IRandomService _randomService;
    
    public StageEventService(IContentService contentService, IRandomService randomService)
    {
        _contentService = contentService;
        _randomService = randomService;
    }
    
    public StageEventData GetRandomStageEvent()
    {
        var allEvents = _contentService.GetAllStageEventData();
        if (allEvents == null || allEvents.Count == 0)
        {
            return null;
        }
        
        // 重み付き確率で選択
        var totalWeight = allEvents.Sum(e => e.weight);
        var randomValue = _randomService.RandomRange(0f, totalWeight);
        
        var currentWeight = 0f;
        foreach (var eventData in allEvents)
        {
            currentWeight += eventData.weight;
            if (randomValue <= currentWeight)
            {
                return eventData;
            }
        }
        
        return allEvents.LastOrDefault();
    }
    
    public bool IsOptionAvailable(StageEventData.EventOptionData option)
    {
        foreach (var action in option.actions)
        {
            if (!action.CanExecute())
                return false;
        }
        return true;
    }
    
    public void ExecuteOption(StageEventData.EventOptionData option)
    {
        foreach (var action in option.actions)
        {
            action.Execute();
        }
    }
}