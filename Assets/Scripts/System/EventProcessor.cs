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
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = _currentEvent.options[i].optionDescription;
            SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.options[i]);
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
                        InventoryManager.Instance.RemoveAndShiftBall(behaviour.intValue);
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
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        allEventData.Register();
    }
}
