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
    }

    private static void SetOptionBehaviour(Button button, EventOption option)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            switch (option.optionType)
            {
                case EventOptionType.AddHealth:
                    GameManager.Instance.Player.Heal(option.intValue);
                    break;
                case EventOptionType.SubHealth:
                    GameManager.Instance.Player.Damage(option.intValue);
                    break;
                case EventOptionType.AddCoin:
                    GameManager.Instance.AddCoin(option.intValue);
                    break;
                case EventOptionType.SubCoin:
                    GameManager.Instance.SubCoin(option.intValue);
                    break;
                case EventOptionType.GetBall:
                    InventoryManager.Instance.AddBall(option.ballValue); 
                    break;
                case EventOptionType.RemoveBall:
                    InventoryManager.Instance.RemoveAndShiftBall(option.intValue);
                    break;
                case EventOptionType.GetRelic:
                    RelicManager.Instance.AddRelic(option.relicValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
