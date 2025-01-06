using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventProcessor : MonoBehaviour
{
    public static EventProcessor Instance;
    
    [SerializeField] private EventDataList allEventData;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private List<GameObject> options;
    
    private EventData _currentEvent;
    
    public void SetRandomEvent()
    {
        var r = GameManager.Instance.RandomRange(0, allEventData.list.Count);
        _currentEvent = allEventData.list[r];
        descriptionText.text = _currentEvent.eventDescription;
        options.ForEach(option => option.SetActive(false));
        
        for (var i = 0; i < _currentEvent.options.Count; i++)
        {
            options[i].SetActive(true);
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = _currentEvent.options[i].optionDescription + GetBehaviourDescription(_currentEvent.options[i].behaviours);
            SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.options[i]);
            options[i].GetComponent<Button>().interactable = CheckBehaviourCondition(_currentEvent.options[i].behaviours);
        }
        EventManager.OnEventEnter.Trigger(_currentEvent);
    }

    private static void SetOptionBehaviour(Button button, EventOption option)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            foreach (var behaviour in option.behaviours)
            {
                switch (behaviour.behaviourType)
                {
                    case EventBehaviourType.AddHealth:
                        GameManager.Instance.Player.Heal(behaviour.intValue);
                        break;
                    case EventBehaviourType.SubHealth:
                        GameManager.Instance.Player.Damage(behaviour.intValue);
                        break;
                    case EventBehaviourType.AddCoin:
                        GameManager.Instance.AddCoin(behaviour.intValue);
                        break;
                    case EventBehaviourType.SubCoin:
                        GameManager.Instance.SubCoin(behaviour.intValue);
                        break;
                    case EventBehaviourType.GetBall:
                        InventoryManager.Instance.AddBall(behaviour.ballValue);
                        break;
                    case EventBehaviourType.RemoveBall:
                        for(var i = 0; i < behaviour.intValue; i++){
                            var index = InventoryManager.Instance.InventorySize - 1; // 一番後ろのボールを削除
                            InventoryManager.Instance.RemoveAndShiftBall(index);
                        }
                        break;
                    case EventBehaviourType.GetRelic:
                        RelicManager.Instance.AddRelic(behaviour.relicValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            UIManager.Instance.EnableCanvasGroup("Event", false);
            GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        });
    }
    
    private bool CheckBehaviourCondition(List<EventBehaviour> behaviours)
    {
        foreach (var behaviour in behaviours)
        {
            switch (behaviour.behaviourType)
            {
                case EventBehaviourType.SubHealth:
                    if (GameManager.Instance.Player.Health.Value - behaviour.intValue <= 0) return false;
                    break;
                case EventBehaviourType.SubCoin:
                    if (GameManager.Instance.Coin.Value - behaviour.intValue < 0) return false;
                    break;
                case EventBehaviourType.RemoveBall:
                    if (InventoryManager.Instance.InventorySize <= behaviour.intValue) return false;
                    break;
                case EventBehaviourType.AddHealth:
                case EventBehaviourType.AddCoin:
                case EventBehaviourType.GetBall:
                case EventBehaviourType.GetRelic:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        return true;
    }
    
    private string GetBehaviourDescription(List<EventBehaviour> behaviours)
    {
        if(behaviours.Count == 0) return "";
        
        var description = "(";
        foreach (var behaviour in behaviours)
        {
            description += CreateDescription(behaviour) + " ";
        }
        
        // 最後のスペースを閉じ括弧に変更
        description = description.Remove(description.Length - 1);
        description += ")";
        return description;
    }
    
    private string CreateDescription(EventBehaviour behaviour)
    {
        return behaviour.behaviourType switch
        {
            EventBehaviourType.AddHealth => $"HPを{behaviour.intValue}回復",
            EventBehaviourType.SubHealth => $"{behaviour.intValue}ダメージを受ける",
            EventBehaviourType.AddCoin => $"{behaviour.intValue}コインを獲得",
            EventBehaviourType.SubCoin => $"{behaviour.intValue}コインを消費",
            EventBehaviourType.GetBall => $"{behaviour.ballValue.displayName}を獲得",
            EventBehaviourType.RemoveBall => $"ボールを{behaviour.intValue}個削除",
            EventBehaviourType.GetRelic => $"{behaviour.relicValue.displayName}を獲得",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        allEventData.Register();
    }
}
